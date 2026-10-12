using GymTrackerBusinessService.Generic;
using GymTrackerDataModel.Models;

namespace GymTrackerBusinessService.Scheduling
{
    public interface IGenerateScheduleWorkflow
    {
        Task<GenerateScheduleState> LoadAsync(Guid userId);
        Task<GenerateScheduleCommand> GenerateAsync(Guid userId, SchedulePreferenceVM preference, bool replaceExistingDraft);
        Task<GenerateScheduleCommand> SaveSlotsAsync(Guid userId, int proposalId, IReadOnlyList<ScheduleProposalSlotVM>? slots);
        Task<GenerateScheduleCommand> AcceptAsync(Guid userId, int proposalId, IReadOnlyList<ScheduleProposalSlotVM>? slots);
        Task<GenerateScheduleCommand> AbandonAsync(Guid userId);
        Task PopulatePreferenceDisplayAsync(SchedulePreferenceVM preference);
        Task PopulateSlotDisplayAsync(IList<ScheduleProposalSlotVM> slots);
    }

    public sealed class GenerateScheduleState
    {
        public SchedulePreferenceVM Preference { get; init; } = new();
        public ScheduleProposalVM? Proposal { get; init; }
        public IReadOnlyList<string> Warnings { get; init; } = [];
        public bool HasDraft => Proposal != null;
    }

    public sealed class GenerateScheduleCommand
    {
        public bool Succeeded { get; init; }
        public bool RequiresConfirmation { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = [];
        public GenerateScheduleState State { get; init; } = new();

        public static GenerateScheduleCommand Success(GenerateScheduleState state)
        {
            return new GenerateScheduleCommand { Succeeded = true, State = state };
        }

        public static GenerateScheduleCommand Failure(IReadOnlyList<string> errors, GenerateScheduleState state)
        {
            return new GenerateScheduleCommand { Succeeded = false, Errors = errors, State = state };
        }
    }

    public sealed class GenerateScheduleWorkflow : IGenerateScheduleWorkflow
    {
        public const string ReplaceDraftConfirmation = "Generate a new schedule? This will replace your current draft proposal.";

        private readonly ITrainingPreferenceService _preferences;
        private readonly IScheduleProposalService _proposals;
        private readonly IGenericRepoService<FitnessLevel> _fitnessLevels;
        private readonly IGenericRepoService<TrainingGoal> _trainingGoals;
        private readonly IGenericRepoService<WorkoutPeriod> _periods;
        private readonly IGenericRepoService<TemplateWorkout> _templates;

        public GenerateScheduleWorkflow(
            ITrainingPreferenceService preferences,
            IScheduleProposalService proposals,
            IGenericRepoService<FitnessLevel> fitnessLevels,
            IGenericRepoService<TrainingGoal> trainingGoals,
            IGenericRepoService<WorkoutPeriod> periods,
            IGenericRepoService<TemplateWorkout> templates)
        {
            _preferences = preferences;
            _proposals = proposals;
            _fitnessLevels = fitnessLevels;
            _trainingGoals = trainingGoals;
            _periods = periods;
            _templates = templates;
        }

        public async Task<GenerateScheduleState> LoadAsync(Guid userId)
        {
            TrainingPreferenceDto? preference = await _preferences.GetAsync(userId);
            ScheduleProposal? draft = await _proposals.GetDraftAsync(userId);
            IReadOnlyList<string> warnings = draft == null ? [] : await _proposals.GetWarningsAsync(userId);
            return new GenerateScheduleState
            {
                Preference = await MapPreferenceAsync(preference),
                Proposal = draft == null ? null : MapProposal(draft),
                Warnings = warnings
            };
        }

        public async Task<GenerateScheduleCommand> GenerateAsync(Guid userId, SchedulePreferenceVM preference, bool replaceExistingDraft)
        {
            ScheduleProposal? existing = await _proposals.GetDraftAsync(userId);
            if (existing != null && !replaceExistingDraft)
            {
                return new GenerateScheduleCommand
                {
                    Succeeded = false,
                    RequiresConfirmation = true,
                    Errors = [ReplaceDraftConfirmation],
                    State = await LoadAsync(userId)
                };
            }

            int[] goalIds = (preference.Goals ?? new List<SchedulePreferenceGoalVM>())
                .Select(item => item.TrainingGoalId)
                .Where(item => item > 0)
                .Distinct()
                .ToArray();
            SaveTrainingPreferenceResult saved = await _preferences.SaveAsync(userId, preference.FitnessLevelId, goalIds);
            if (!saved.Succeeded)
            {
                return GenerateScheduleCommand.Failure(saved.Errors, await LoadAsync(userId));
            }

            ProposalCommandResult generated = await _proposals.GenerateAsync(userId);
            GenerateScheduleState state = await LoadAsync(userId);
            return generated.Succeeded
                ? GenerateScheduleCommand.Success(state)
                : GenerateScheduleCommand.Failure(generated.Errors, state);
        }

        public async Task<GenerateScheduleCommand> SaveSlotsAsync(Guid userId, int proposalId, IReadOnlyList<ScheduleProposalSlotVM>? slots)
        {
            List<ProposalSlotInput> inputs = (slots ?? Array.Empty<ScheduleProposalSlotVM>())
                .Select(item => new ProposalSlotInput
                {
                    ScheduleProposalId = proposalId,
                    DayOfWeek = (int)item.DayOfWeek,
                    WorkoutPeriodId = item.WorkoutPeriodId,
                    TemplateWorkoutId = item.TemplateWorkoutId,
                    SortOrder = item.SortOrder
                })
                .ToList();
            ProposalCommandResult saved = await _proposals.UpdateSlotsAsync(userId, inputs);
            GenerateScheduleState state = await LoadAsync(userId);
            return saved.Succeeded
                ? GenerateScheduleCommand.Success(state)
                : GenerateScheduleCommand.Failure(saved.Errors, state);
        }

        public async Task<GenerateScheduleCommand> AcceptAsync(Guid userId, int proposalId, IReadOnlyList<ScheduleProposalSlotVM>? slots)
        {
            GenerateScheduleCommand saved = await SaveSlotsAsync(userId, proposalId, slots);
            if (!saved.Succeeded)
            {
                return saved;
            }

            ProposalCommandResult accepted = await _proposals.AcceptAsync(userId, proposalId);
            GenerateScheduleState state = await LoadAsync(userId);
            return accepted.Succeeded
                ? GenerateScheduleCommand.Success(state)
                : GenerateScheduleCommand.Failure(accepted.Errors, state);
        }

        public async Task<GenerateScheduleCommand> AbandonAsync(Guid userId)
        {
            ProposalCommandResult abandoned = await _proposals.AbandonAsync(userId);
            GenerateScheduleState state = await LoadAsync(userId);
            return abandoned.Succeeded
                ? GenerateScheduleCommand.Success(state)
                : GenerateScheduleCommand.Failure(abandoned.Errors, state);
        }

        public async Task PopulatePreferenceDisplayAsync(SchedulePreferenceVM preference)
        {
            SchedulePreferenceVM mapped = await MapPreferenceAsync(new TrainingPreferenceDto
            {
                UserId = Guid.Empty,
                FitnessLevelId = preference.FitnessLevelId,
                FitnessLevel = preference.FitnessLevel?.Description ?? "",
                Goals = (preference.Goals ?? new List<SchedulePreferenceGoalVM>())
                    .Where(item => item.TrainingGoalId > 0)
                    .Select(item => new TrainingGoalDto
                    {
                        Id = item.TrainingGoalId,
                        Description = item.TrainingGoal?.Description ?? ""
                    })
                    .ToArray()
            });
            preference.WeeklyTraining = mapped.WeeklyTraining;
            preference.FitnessLevel = mapped.FitnessLevel;
            preference.Goals = mapped.Goals;
        }

        public async Task PopulateSlotDisplayAsync(IList<ScheduleProposalSlotVM> slots)
        {
            IReadOnlyList<WorkoutPeriod> periods = await _periods.QueryAsync(item => true, trackChanges: false);
            IReadOnlyList<TemplateWorkout> templates = await _templates.QueryAsync(item => true, trackChanges: false);
            foreach (ScheduleProposalSlotVM slot in slots)
            {
                WorkoutPeriod? period = periods.SingleOrDefault(item => item.Id == slot.WorkoutPeriodId);
                slot.WorkoutPeriod = period == null
                    ? null
                    : new WorkoutPeriod { Id = period.Id, Description = period.Description };
                TemplateWorkout? template = templates.SingleOrDefault(item => item.Id == slot.TemplateWorkoutId);
                slot.TemplateWorkout = template == null
                    ? null
                    : new TemplateWorkout { Id = template.Id, Name = template.Name, FitnessLevelId = template.FitnessLevelId };
            }
        }

        private async Task<SchedulePreferenceVM> MapPreferenceAsync(TrainingPreferenceDto? preference)
        {
            if (preference == null)
            {
                return new SchedulePreferenceVM();
            }

            FitnessLevel? level = (await _fitnessLevels.QueryAsync(
                item => item.Id == preference.FitnessLevelId,
                trackChanges: false)).SingleOrDefault();
            IReadOnlyList<TrainingGoal> goals = await _trainingGoals.QueryAsync(item => true, trackChanges: false);
            return new SchedulePreferenceVM
            {
                FitnessLevelId = preference.FitnessLevelId,
                WeeklyTraining = level?.WeeklyTraining,
                FitnessLevel = level == null
                    ? null
                    : new FitnessLevel
                    {
                        Id = level.Id,
                        Description = level.Description,
                        WeeklyTraining = level.WeeklyTraining,
                        MinSessionsPerWeek = level.MinSessionsPerWeek,
                        MaxSessionsPerWeek = level.MaxSessionsPerWeek
                    },
                Goals = preference.Goals.Select(item =>
                {
                    TrainingGoal? goal = goals.SingleOrDefault(candidate => candidate.Id == item.Id);
                    return new SchedulePreferenceGoalVM
                    {
                        TrainingGoalId = item.Id,
                        TrainingGoal = new TrainingGoal
                        {
                            Id = item.Id,
                            Description = goal?.Description ?? item.Description
                        }
                    };
                }).ToList()
            };
        }

        private static ScheduleProposalVM MapProposal(ScheduleProposal proposal)
        {
            return new ScheduleProposalVM
            {
                Id = proposal.Id,
                RecommendedSessionCount = proposal.RecommendedSessionCount,
                Explanation = proposal.Explanation,
                Slots = proposal.Slots
                    .OrderBy(item => item.SortOrder)
                    .Select(item => new ScheduleProposalSlotVM
                    {
                        DayOfWeek = item.DayOfWeek,
                        WorkoutPeriodId = item.WorkoutPeriodId,
                        WorkoutPeriod = item.WorkoutPeriod == null
                            ? null
                            : new WorkoutPeriod { Id = item.WorkoutPeriod.Id, Description = item.WorkoutPeriod.Description },
                        TemplateWorkoutId = item.TemplateWorkoutId,
                        TemplateWorkout = item.TemplateWorkout == null
                            ? null
                            : new TemplateWorkout
                            {
                                Id = item.TemplateWorkout.Id,
                                Name = item.TemplateWorkout.Name,
                                FitnessLevelId = item.TemplateWorkout.FitnessLevelId
                            },
                        SortOrder = item.SortOrder
                    })
                    .ToList()
            };
        }
    }
}
