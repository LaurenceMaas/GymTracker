using GymTrackerBusinessService.Generic;
using GymTrackerDataModel.Models;

namespace GymTrackerBusinessService.Scheduling
{
    public interface IScheduleProposalService
    {
        Task<ProposalCommandResult> GenerateAsync(Guid userId);
        Task<ScheduleProposal?> GetDraftAsync(Guid userId);
        Task<ProposalCommandResult> UpdateSlotsAsync(Guid userId, IReadOnlyList<ProposalSlotInput>? slots);
        Task<IReadOnlyList<string>> GetWarningsAsync(Guid userId);
        Task<ProposalCommandResult> AbandonAsync(Guid userId);
        Task<ProposalCommandResult> AcceptAsync(Guid userId, int proposalId);
    }

    public sealed class ProposalCommandResult
    {
        public bool Succeeded { get; init; }
        public ScheduleProposal? Proposal { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = [];

        public static ProposalCommandResult Success(ScheduleProposal? proposal)
        {
            return new ProposalCommandResult { Succeeded = true, Proposal = proposal };
        }

        public static ProposalCommandResult Failure(IReadOnlyList<string> errors)
        {
            return new ProposalCommandResult { Succeeded = false, Errors = errors };
        }
    }

    public sealed class ScheduleProposalService : IScheduleProposalService
    {
        private static readonly string[] DraftIncludes =
        [
            "FitnessLevel",
            "Goals.TrainingGoal",
            "Slots.WorkoutPeriod",
            "Slots.TemplateWorkout.FitnessLevel"
        ];

        private readonly ITrainingPreferenceService _preferences;
        private readonly IGenericRepoService<ScheduleProposal> _proposals;
        private readonly IGenericRepoService<ScheduleProposalGoal> _goals;
        private readonly IGenericRepoService<ScheduleProposalSlot> _slots;
        private readonly IGenericRepoService<FitnessLevel> _fitnessLevels;
        private readonly IGenericRepoService<TemplateWorkout> _templates;
        private readonly IGenericRepoService<TemplateTrainingGoal> _templateGoals;
        private readonly IGenericRepoService<WorkoutPeriod> _periods;
        private readonly IGenericRepoService<DOWWorkout> _schedule;
        private readonly IScheduleAdvisor _advisor;
        private readonly ScheduleProposalValidator _validator;

        public ScheduleProposalService(
            ITrainingPreferenceService preferences,
            IGenericRepoService<ScheduleProposal> proposals,
            IGenericRepoService<ScheduleProposalGoal> goals,
            IGenericRepoService<ScheduleProposalSlot> slots,
            IGenericRepoService<FitnessLevel> fitnessLevels,
            IGenericRepoService<TemplateWorkout> templates,
            IGenericRepoService<TemplateTrainingGoal> templateGoals,
            IGenericRepoService<WorkoutPeriod> periods,
            IGenericRepoService<DOWWorkout> schedule,
            IScheduleAdvisor advisor,
            ScheduleProposalValidator validator)
        {
            _preferences = preferences;
            _proposals = proposals;
            _goals = goals;
            _slots = slots;
            _fitnessLevels = fitnessLevels;
            _templates = templates;
            _templateGoals = templateGoals;
            _periods = periods;
            _schedule = schedule;
            _advisor = advisor;
            _validator = validator;
        }

        public async Task<ProposalCommandResult> GenerateAsync(Guid userId)
        {
            TrainingPreferenceDto? preference = await _preferences.GetAsync(userId);
            if (preference == null)
            {
                return ProposalCommandResult.Failure(["Save a fitness level and at least one training goal before generating a schedule."]);
            }

            FitnessLevel? fitnessLevel = (await _fitnessLevels.QueryAsync(
                item => item.Id == preference.FitnessLevelId,
                trackChanges: false)).SingleOrDefault();
            if (fitnessLevel == null)
            {
                return ProposalCommandResult.Failure([$"Fitness level {preference.FitnessLevelId} does not exist."]);
            }

            IReadOnlyList<WorkoutPeriod> periods = await LoadPeriodsAsync();
            IReadOnlyList<TemplateWorkout> templates = await LoadTemplatesAsync();
            IReadOnlyList<TemplateTrainingGoal> templateGoals = await _templateGoals.QueryAsync(item => true, trackChanges: false);
            HashSet<int> selectedGoalIds = preference.Goals.Select(item => item.Id).ToHashSet();
            var goalsByTemplate = templateGoals
                .GroupBy(item => item.TemplateWorkoutId)
                .ToDictionary(item => item.Key, item => item.Select(goal => goal.TrainingGoalId).ToArray());

            ScheduleAdvisorTemplate[] pool = templates
                .Where(item => item.FitnessLevelId == preference.FitnessLevelId)
                .Where(item => goalsByTemplate.TryGetValue(item.Id, out int[]? goalIds) && goalIds.Any(selectedGoalIds.Contains))
                .Select(item => new ScheduleAdvisorTemplate
                {
                    Id = item.Id,
                    Name = item.Name,
                    FitnessLevelId = item.FitnessLevelId,
                    FitnessLevelName = item.FitnessLevel?.Description ?? fitnessLevel.Description ?? "",
                    GoalIds = goalsByTemplate.TryGetValue(item.Id, out int[]? ids) ? ids : []
                })
                .ToArray();

            var request = new ScheduleAdvisorRequest
            {
                FitnessLevelId = fitnessLevel.Id,
                FitnessLevelName = fitnessLevel.Description ?? "",
                MinSessionsPerWeek = fitnessLevel.MinSessionsPerWeek,
                MaxSessionsPerWeek = fitnessLevel.MaxSessionsPerWeek,
                WeeklyTraining = fitnessLevel.WeeklyTraining,
                Qualification = fitnessLevel.Qualification,
                StrengthLoading = fitnessLevel.StrengthLoading,
                Goals = preference.Goals.Select(item => new ScheduleAdvisorGoal { Id = item.Id, Name = item.Description }).ToArray(),
                Periods = periods.Select(item => new ScheduleAdvisorPeriod { Id = item.Id, Name = item.Description ?? "" }).ToArray(),
                Templates = pool
            };

            ScheduleAdvisorResponse response;
            try
            {
                response = await _advisor.RecommendAsync(request);
            }
            catch (ScheduleAdvisorException exception)
            {
                return ProposalCommandResult.Failure([exception.Message]);
            }
            List<ProposalSlotInput> slots = (response.Slots ?? [])
                .Select(item => new ProposalSlotInput
                {
                    DayOfWeek = item.DayOfWeek,
                    WorkoutPeriodId = item.WorkoutPeriodId,
                    TemplateWorkoutId = item.TemplateWorkoutId,
                    SortOrder = item.SortOrder
                })
                .ToList();

            ProposalValidationResult validation = _validator.ValidateAiProposal(
                response.RecommendedSessionCount,
                slots,
                templates.Select(item => item.Id).ToHashSet(),
                periods.Select(item => item.Id).ToHashSet());
            if (!validation.IsValid)
            {
                return ProposalCommandResult.Failure(validation.Errors);
            }

            var draft = new ScheduleProposal
            {
                UserId = userId,
                Status = ProposalStatus.Draft,
                FitnessLevelId = fitnessLevel.Id,
                RecommendedSessionCount = response.RecommendedSessionCount,
                Explanation = response.Explanation,
                CreatedAt = DateTime.Now,
                Goals = preference.Goals.Select(item => new ScheduleProposalGoal { TrainingGoalId = item.Id }).ToList(),
                Slots = slots.Select(item => new ScheduleProposalSlot
                {
                    DayOfWeek = (DayOfWeek)item.DayOfWeek,
                    WorkoutPeriodId = item.WorkoutPeriodId,
                    TemplateWorkoutId = item.TemplateWorkoutId,
                    SortOrder = item.SortOrder
                }).ToList()
            };

            await ReplaceDraftAsync(userId, draft);
            return ProposalCommandResult.Success(await GetDraftAsync(userId));
        }
        public async Task<ScheduleProposal?> GetDraftAsync(Guid userId)
        {
            return (await _proposals.QueryAsync(
                item => item.UserId == userId && item.Status == ProposalStatus.Draft,
                trackChanges: false,
                DraftIncludes)).SingleOrDefault();
        }
        public async Task<ProposalCommandResult> UpdateSlotsAsync(Guid userId, IReadOnlyList<ProposalSlotInput>? slots)
        {
            ScheduleProposal? existing = await GetDraftAsync(userId);
            if (existing == null)
            {
                return ProposalCommandResult.Failure(["There is no draft proposal to update."]);
            }

            List<ProposalSlotInput> proposed = (slots ?? [])
                .Select(item => new ProposalSlotInput
                {
                    ScheduleProposalId = existing.Id,
                    DayOfWeek = item.DayOfWeek,
                    WorkoutPeriodId = item.WorkoutPeriodId,
                    TemplateWorkoutId = item.TemplateWorkoutId,
                    SortOrder = item.SortOrder
                })
                .ToList();

            IReadOnlyList<TemplateWorkout> templates = await LoadTemplatesAsync();
            IReadOnlyList<WorkoutPeriod> periods = await LoadPeriodsAsync();
            ProposalValidationResult validation = _validator.ValidateUserProposal(
                proposed,
                templates.Select(item => item.Id).ToHashSet(),
                periods.Select(item => item.Id).ToHashSet());
            if (!validation.IsValid)
            {
                return ProposalCommandResult.Failure(validation.Errors);
            }

            ScheduleProposal? draft = (await _proposals.QueryAsync(
                item => item.Id == existing.Id && item.UserId == userId && item.Status == ProposalStatus.Draft,
                trackChanges: true,
                "Slots")).SingleOrDefault();
            if (draft == null)
            {
                return ProposalCommandResult.Failure(["There is no draft proposal to update."]);
            }

            await SaveAtomicallyAsync(async () =>
            {
                foreach (ScheduleProposalSlot slot in draft.Slots.ToList())
                {
                    draft.Slots.Remove(slot);
                    _slots.StageRemove(slot);
                }

                await _slots.SaveAsync();
                foreach (ProposalSlotInput slot in proposed)
                {
                    _slots.StageAdd(new ScheduleProposalSlot
                    {
                        ScheduleProposal = draft,
                        ScheduleProposalId = draft.Id,
                        DayOfWeek = (DayOfWeek)slot.DayOfWeek,
                        WorkoutPeriodId = slot.WorkoutPeriodId,
                        TemplateWorkoutId = slot.TemplateWorkoutId,
                        SortOrder = slot.SortOrder
                    });
                }

                await _slots.SaveAsync();
            });
            return ProposalCommandResult.Success(await GetDraftAsync(userId));
        }
        public async Task<IReadOnlyList<string>> GetWarningsAsync(Guid userId)
        {
            ScheduleProposal? draft = await GetDraftAsync(userId);
            if (draft == null)
            {
                return [];
            }

            IReadOnlyList<TemplateTrainingGoal> templateGoals = await _templateGoals.QueryAsync(item => true, trackChanges: false);
            var goalsByTemplate = templateGoals
                .GroupBy(item => item.TemplateWorkoutId)
                .ToDictionary(item => item.Key, item => item.Select(goal => goal.TrainingGoalId).ToHashSet());
            HashSet<int> proposalGoalIds = draft.Goals.Select(item => item.TrainingGoalId).ToHashSet();
            var warnings = new List<string>();

            int sessionCount = draft.Slots.Count;
            if (sessionCount < draft.FitnessLevel.MinSessionsPerWeek || sessionCount > draft.FitnessLevel.MaxSessionsPerWeek)
            {
                warnings.Add($"The draft has {sessionCount} sessions, outside the {draft.FitnessLevel.Description} range of {draft.FitnessLevel.MinSessionsPerWeek} to {draft.FitnessLevel.MaxSessionsPerWeek}.");
            }

            foreach (ScheduleProposalSlot slot in draft.Slots.OrderBy(item => item.SortOrder))
            {
                TemplateWorkout? template = slot.TemplateWorkout;
                if (template != null && template.FitnessLevelId != draft.FitnessLevelId)
                {
                    warnings.Add($"{template.Name} is a {template.FitnessLevel?.Description} template. This proposal uses {draft.FitnessLevel.Description}.");
                }

                HashSet<int> templateGoalIds = goalsByTemplate.TryGetValue(slot.TemplateWorkoutId, out HashSet<int>? ids)
                    ? ids
                    : [];
                if (!templateGoalIds.Overlaps(proposalGoalIds))
                {
                    warnings.Add($"{template?.Name ?? "Template " + slot.TemplateWorkoutId} does not share a goal with this proposal.");
                }
            }

            foreach (var group in draft.Slots.GroupBy(item => new { item.DayOfWeek, item.WorkoutPeriodId }))
            {
                if (group.Count() < 2)
                {
                    continue;
                }

                string periodName = group.First().WorkoutPeriod?.Description ?? "that period";
                warnings.Add($"More than one workout is scheduled on {group.Key.DayOfWeek} in {periodName}. Today's Workout shows one match.");
            }

            if (draft.Slots.Any(item => string.Equals(item.WorkoutPeriod?.Description, "Evening", StringComparison.OrdinalIgnoreCase)))
            {
                warnings.Add("Evening is selected. Today's Workout does not currently match that period.");
            }

            return warnings;
        }
        public async Task<ProposalCommandResult> AbandonAsync(Guid userId)
        {
            ScheduleProposal? draft = (await _proposals.QueryAsync(
                item => item.UserId == userId && item.Status == ProposalStatus.Draft,
                trackChanges: true)).SingleOrDefault();
            if (draft == null)
            {
                return ProposalCommandResult.Failure(["There is no draft proposal to abandon."]);
            }

            draft.Status = ProposalStatus.Abandoned;
            draft.AbandonedAt = DateTime.Now;
            await SaveAtomicallyAsync(() => _proposals.SaveAsync());
            return ProposalCommandResult.Success(null);
        }
        public async Task<ProposalCommandResult> AcceptAsync(Guid userId, int proposalId)
        {
            ScheduleProposal? proposal = (await _proposals.QueryAsync(
                item => item.Id == proposalId,
                trackChanges: false,
                DraftIncludes)).SingleOrDefault();
            if (proposal == null || proposal.UserId != userId)
            {
                return ProposalCommandResult.Failure(["There is no draft proposal to accept."]);
            }

            if (proposal.Status != ProposalStatus.Draft)
            {
                return ProposalCommandResult.Failure(["Only a draft proposal can be accepted."]);
            }

            List<ProposalSlotInput> slots = proposal.Slots
                .Select(item => new ProposalSlotInput
                {
                    ScheduleProposalId = proposal.Id,
                    DayOfWeek = (int)item.DayOfWeek,
                    WorkoutPeriodId = item.WorkoutPeriodId,
                    TemplateWorkoutId = item.TemplateWorkoutId,
                    SortOrder = item.SortOrder
                })
                .ToList();
            IReadOnlyList<TemplateWorkout> templates = await LoadTemplatesAsync();
            IReadOnlyList<WorkoutPeriod> periods = await LoadPeriodsAsync();
            ProposalValidationResult validation = _validator.ValidateUserProposal(
                slots,
                templates.Select(item => item.Id).ToHashSet(),
                periods.Select(item => item.Id).ToHashSet());
            if (!validation.IsValid)
            {
                return ProposalCommandResult.Failure(validation.Errors);
            }

            ScheduleProposal? draft = (await _proposals.QueryAsync(
                item => item.Id == proposalId && item.UserId == userId && item.Status == ProposalStatus.Draft,
                trackChanges: true,
                "Slots")).SingleOrDefault();
            if (draft == null)
            {
                return ProposalCommandResult.Failure(["There is no draft proposal to accept."]);
            }

            await SaveAtomicallyAsync(async () =>
            {
                if (draft.UserId != userId || draft.Status != ProposalStatus.Draft)
                {
                    throw new InvalidOperationException("There is no draft proposal to accept.");
                }

                IReadOnlyList<DOWWorkout> existingSchedule = await _schedule.QueryAsync(
                    item => item.UserId == userId,
                    trackChanges: true);
                foreach (DOWWorkout row in existingSchedule)
                {
                    _schedule.StageRemove(row);
                }

                foreach (ScheduleProposalSlot slot in draft.Slots)
                {
                    _schedule.StageAdd(new DOWWorkout
                    {
                        UserId = userId,
                        DayOfWeek = slot.DayOfWeek,
                        WorkoutPeriodId = slot.WorkoutPeriodId,
                        TemplateWorkoutId = slot.TemplateWorkoutId,
                        SortOrder = slot.SortOrder
                    });
                }

                draft.Status = ProposalStatus.Accepted;
                draft.AcceptedAt = DateTime.UtcNow;
                await _proposals.SaveAsync();
            });

            ScheduleProposal? accepted = (await _proposals.QueryAsync(
                item => item.Id == proposalId && item.UserId == userId,
                trackChanges: false,
                DraftIncludes)).SingleOrDefault();
            return ProposalCommandResult.Success(accepted);
        }
        private async Task ReplaceDraftAsync(Guid userId, ScheduleProposal draft)
        {
            ScheduleProposal? existing = (await _proposals.QueryAsync(
                item => item.UserId == userId && item.Status == ProposalStatus.Draft,
                trackChanges: true,
                "Goals",
                "Slots")).SingleOrDefault();

            await SaveAtomicallyAsync(async () =>
            {
                if (existing != null)
                {
                    foreach (ScheduleProposalSlot slot in existing.Slots.ToList())
                    {
                        existing.Slots.Remove(slot);
                        _slots.StageRemove(slot);
                    }

                    foreach (ScheduleProposalGoal goal in existing.Goals.ToList())
                    {
                        existing.Goals.Remove(goal);
                        _goals.StageRemove(goal);
                    }

                    _proposals.StageRemove(existing);
                    await _proposals.SaveAsync();
                }

                _proposals.StageAdd(draft);
                await _proposals.SaveAsync();
            });
        }
        private async Task<IReadOnlyList<WorkoutPeriod>> LoadPeriodsAsync()
        {
            IReadOnlyList<WorkoutPeriod> periods = await _periods.QueryAsync(item => true, trackChanges: false);
            return periods.OrderBy(item => item.Id).ToArray();
        }
        private async Task<IReadOnlyList<TemplateWorkout>> LoadTemplatesAsync()
        {
            IReadOnlyList<TemplateWorkout> templates = await _templates.QueryAsync(item => true, trackChanges: false, "FitnessLevel");
            return templates.OrderBy(item => item.Id).ToArray();
        }
        private async Task SaveAtomicallyAsync(Func<Task> write)
        {
            await _proposals.BeginTransactionAsync();
            try
            {
                await write();
                await _proposals.CommitTransactionAsync();
            }
            catch
            {
                await _proposals.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
