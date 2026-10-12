namespace GymTrackerBusinessService.Scheduling
{
    public sealed class ScheduleAdvisorGoal
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
    }

    public sealed class ScheduleAdvisorPeriod
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
    }

    public sealed class ScheduleAdvisorTemplate
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public int FitnessLevelId { get; init; }
        public string FitnessLevelName { get; init; } = "";
        public IReadOnlyList<int> GoalIds { get; init; } = [];
    }

    public sealed class ScheduleAdvisorRequest
    {
        public int FitnessLevelId { get; init; }
        public string FitnessLevelName { get; init; } = "";
        public int MinSessionsPerWeek { get; init; }
        public int MaxSessionsPerWeek { get; init; }
        public string? WeeklyTraining { get; init; }
        public string? Qualification { get; init; }
        public string? StrengthLoading { get; init; }
        public IReadOnlyList<ScheduleAdvisorGoal> Goals { get; init; } = [];
        public IReadOnlyList<ScheduleAdvisorPeriod> Periods { get; init; } = [];
        public IReadOnlyList<ScheduleAdvisorTemplate> Templates { get; init; } = [];
    }

    public sealed class ScheduleAdvisorSlot
    {
        public int DayOfWeek { get; init; }
        public int WorkoutPeriodId { get; init; }
        public int TemplateWorkoutId { get; init; }
        public int SortOrder { get; init; }
    }

    public sealed class ScheduleAdvisorResponse
    {
        public int RecommendedSessionCount { get; init; }
        public string? Explanation { get; init; }
        public IReadOnlyList<ScheduleAdvisorSlot> Slots { get; init; } = [];
    }

    public interface IScheduleAdvisor
    {
        Task<ScheduleAdvisorResponse> RecommendAsync(ScheduleAdvisorRequest request);
    }
}
