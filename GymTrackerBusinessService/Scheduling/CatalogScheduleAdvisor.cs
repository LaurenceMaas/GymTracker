using System.Text.Json;
using System.Text.Json.Serialization;

namespace GymTrackerBusinessService.Scheduling
{
    public sealed class CatalogScheduleAdvisor : IScheduleAdvisor
    {
        public const string Instructions =
            """
            You are the GymTracker workout scheduling advisor. Recommend one weekly schedule from the supplied fitness level, goals, weekly training guidance, periods, and workout catalogue.

            Consider the fitness level and its characteristics, the selected goals, a session count inside the supplied minimum and maximum, variety across the catalogue, a sensible spread of days, and a suitable period. Prefer workouts that fit the level and the goals. Repeat a workout only when the catalogue does not offer a better alternative.

            Use only templateWorkoutId and workoutPeriodId values from the supplied lists. dayOfWeek is an integer from 0 (Sunday) through 6 (Saturday). recommendedSessionCount must equal the number of slots. Number sortOrder from 1. Explain the recommendation briefly in explanation.
            """;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        private readonly IScheduleLanguageModel _model;

        public CatalogScheduleAdvisor(IScheduleLanguageModel model)
        {
            _model = model;
        }

        public async Task<ScheduleAdvisorResponse> RecommendAsync(ScheduleAdvisorRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            IReadOnlyList<ScheduleAdvisorTemplate> templates = request.Templates ?? [];
            IReadOnlyList<ScheduleAdvisorPeriod> periods = request.Periods ?? [];
            if (templates.Count == 0 || periods.Count == 0)
            {
                return new ScheduleAdvisorResponse
                {
                    RecommendedSessionCount = 0,
                    Explanation = "No template in the supplied catalog matches this fitness level and these goals.",
                    Slots = []
                };
            }

            string context = JsonSerializer.Serialize(BuildContext(request, templates, periods), JsonOptions);
            string json = await _model.CompleteAsync(Instructions, context);
            return Read(json);
        }

        private static AdvisorContext BuildContext(
            ScheduleAdvisorRequest request,
            IReadOnlyList<ScheduleAdvisorTemplate> templates,
            IReadOnlyList<ScheduleAdvisorPeriod> periods)
        {
            return new AdvisorContext
            {
                FitnessLevel = new FitnessContext
                {
                    Id = request.FitnessLevelId,
                    Name = request.FitnessLevelName,
                    Qualification = request.Qualification,
                    StrengthLoading = request.StrengthLoading,
                    WeeklyTraining = request.WeeklyTraining,
                    MinSessionsPerWeek = request.MinSessionsPerWeek,
                    MaxSessionsPerWeek = request.MaxSessionsPerWeek
                },
                Goals = (request.Goals ?? [])
                    .Select(item => new GoalContext { Id = item.Id, Name = item.Name })
                    .ToArray(),
                Periods = periods
                    .Select(item => new PeriodContext { Id = item.Id, Name = item.Name })
                    .ToArray(),
                Workouts = templates
                    .Select(item => new WorkoutContext
                    {
                        Id = item.Id,
                        Name = item.Name,
                        FitnessLevelId = item.FitnessLevelId,
                        FitnessLevel = item.FitnessLevelName,
                        GoalIds = item.GoalIds ?? []
                    })
                    .ToArray()
            };
        }

        private static ScheduleAdvisorResponse Read(string json)
        {
            RecommendationDto? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<RecommendationDto>(json, JsonOptions);
            }
            catch (JsonException)
            {
                throw new ScheduleAdvisorException("The schedule advisor returned a response that could not be read.");
            }

            if (parsed?.RecommendedSessionCount == null || string.IsNullOrWhiteSpace(parsed.Explanation) || parsed.Slots == null)
            {
                throw new ScheduleAdvisorException("The schedule advisor returned a response that could not be read.");
            }

            var slots = new List<ScheduleAdvisorSlot>(parsed.Slots.Length);
            foreach (RecommendationSlotDto slot in parsed.Slots)
            {
                if (slot.DayOfWeek == null || slot.WorkoutPeriodId == null || slot.TemplateWorkoutId == null || slot.SortOrder == null)
                {
                    throw new ScheduleAdvisorException("The schedule advisor returned a response that could not be read.");
                }

                slots.Add(new ScheduleAdvisorSlot
                {
                    DayOfWeek = slot.DayOfWeek.Value,
                    WorkoutPeriodId = slot.WorkoutPeriodId.Value,
                    TemplateWorkoutId = slot.TemplateWorkoutId.Value,
                    SortOrder = slot.SortOrder.Value
                });
            }

            return new ScheduleAdvisorResponse
            {
                RecommendedSessionCount = parsed.RecommendedSessionCount.Value,
                Explanation = parsed.Explanation.Trim(),
                Slots = slots
            };
        }

        private sealed class AdvisorContext
        {
            public FitnessContext FitnessLevel { get; init; } = new();
            public GoalContext[] Goals { get; init; } = [];
            public PeriodContext[] Periods { get; init; } = [];
            public WorkoutContext[] Workouts { get; init; } = [];
        }

        private sealed class FitnessContext
        {
            public int Id { get; init; }
            public string Name { get; init; } = "";
            public string? Qualification { get; init; }
            public string? StrengthLoading { get; init; }
            public string? WeeklyTraining { get; init; }
            public int MinSessionsPerWeek { get; init; }
            public int MaxSessionsPerWeek { get; init; }
        }

        private sealed class GoalContext
        {
            public int Id { get; init; }
            public string Name { get; init; } = "";
        }

        private sealed class PeriodContext
        {
            public int Id { get; init; }
            public string Name { get; init; } = "";
        }

        private sealed class WorkoutContext
        {
            public int Id { get; init; }
            public string Name { get; init; } = "";
            public int FitnessLevelId { get; init; }
            public string FitnessLevel { get; init; } = "";
            public IReadOnlyList<int> GoalIds { get; init; } = [];
        }

        private sealed class RecommendationDto
        {
            public int? RecommendedSessionCount { get; set; }
            public string? Explanation { get; set; }
            public RecommendationSlotDto[]? Slots { get; set; }
        }

        private sealed class RecommendationSlotDto
        {
            public int? DayOfWeek { get; set; }
            public int? WorkoutPeriodId { get; set; }
            public int? TemplateWorkoutId { get; set; }
            public int? SortOrder { get; set; }
        }
    }
}
