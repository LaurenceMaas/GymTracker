namespace GymTrackerBusinessService.Scheduling
{
    public sealed class ProposalSlotInput
    {
        public int ScheduleProposalId { get; set; }
        public int DayOfWeek { get; set; }
        public int WorkoutPeriodId { get; set; }
        public int TemplateWorkoutId { get; set; }
        public int SortOrder { get; set; }
    }

    public sealed class ProposalValidationResult
    {
        public bool IsValid { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = [];

        public static ProposalValidationResult Success()
        {
            return new ProposalValidationResult { IsValid = true };
        }

        public static ProposalValidationResult Failure(IReadOnlyList<string> errors)
        {
            return new ProposalValidationResult { IsValid = false, Errors = errors };
        }
    }

    public sealed class ScheduleProposalValidator
    {
        public ProposalValidationResult ValidateAiProposal(
            int recommendedSessionCount,
            IReadOnlyList<ProposalSlotInput>? slots,
            IReadOnlySet<int> templateWorkoutIds,
            IReadOnlySet<int> workoutPeriodIds)
        {
            ArgumentNullException.ThrowIfNull(templateWorkoutIds);
            ArgumentNullException.ThrowIfNull(workoutPeriodIds);

            var errors = ValidateSlots(slots, templateWorkoutIds, workoutPeriodIds);
            int slotCount = slots?.Count ?? 0;
            if (slotCount > 0 && recommendedSessionCount != slotCount)
            {
                errors.Add($"The recommended session count is {recommendedSessionCount}, but the proposal has {slotCount} slots.");
            }

            return ToResult(errors);
        }

        public ProposalValidationResult ValidateUserProposal(
            IReadOnlyList<ProposalSlotInput>? slots,
            IReadOnlySet<int> templateWorkoutIds,
            IReadOnlySet<int> workoutPeriodIds)
        {
            ArgumentNullException.ThrowIfNull(templateWorkoutIds);
            ArgumentNullException.ThrowIfNull(workoutPeriodIds);

            return ToResult(ValidateSlots(slots, templateWorkoutIds, workoutPeriodIds));
        }

        private static List<string> ValidateSlots(
            IReadOnlyList<ProposalSlotInput>? slots,
            IReadOnlySet<int> templateWorkoutIds,
            IReadOnlySet<int> workoutPeriodIds)
        {
            var errors = new List<string>();
            if (slots == null || slots.Count == 0)
            {
                errors.Add("The proposal has no slots.");
                return errors;
            }

            var seen = new Dictionary<(int ProposalId, int Day, int PeriodId, int TemplateId), int>();
            for (int index = 0; index < slots.Count; index++)
            {
                ProposalSlotInput? slot = slots[index];
                string label = $"Slot {index + 1}";
                if (slot == null)
                {
                    errors.Add($"{label} is missing.");
                    continue;
                }

                if (!templateWorkoutIds.Contains(slot.TemplateWorkoutId))
                {
                    errors.Add($"{label} references template {slot.TemplateWorkoutId}, which does not exist.");
                }

                if (!workoutPeriodIds.Contains(slot.WorkoutPeriodId))
                {
                    errors.Add($"{label} references workout period {slot.WorkoutPeriodId}, which does not exist.");
                }

                if (!Enum.IsDefined(typeof(DayOfWeek), slot.DayOfWeek))
                {
                    errors.Add($"{label} has day {slot.DayOfWeek}. Valid days are Sunday (0) through Saturday (6).");
                }

                var key = (slot.ScheduleProposalId, slot.DayOfWeek, slot.WorkoutPeriodId, slot.TemplateWorkoutId);
                if (seen.TryGetValue(key, out int firstIndex))
                {
                    errors.Add($"{label} duplicates slot {firstIndex + 1}: same proposal, day, period, and template.");
                }
                else
                {
                    seen[key] = index;
                }
            }

            return errors;
        }

        private static ProposalValidationResult ToResult(List<string> errors)
        {
            return errors.Count == 0
                ? ProposalValidationResult.Success()
                : ProposalValidationResult.Failure(errors);
        }
    }
}
