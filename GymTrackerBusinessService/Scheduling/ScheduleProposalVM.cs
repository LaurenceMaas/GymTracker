using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GymTrackerDataModel.Models;

namespace GymTrackerBusinessService.Scheduling
{
    public sealed class ScheduleProposalVM
    {
        [Key]
        [DisplayName("Proposal")]
        public int Id { get; set; }

        [DisplayName("Recommended sessions")]
        [ReadOnly(true)]
        public int RecommendedSessionCount { get; set; }

        [ReadOnly(true)]
        public string? Explanation { get; set; }

        public List<ScheduleProposalSlotVM> Slots { get; set; } = new();
    }

    public sealed class ScheduleProposalSlotVM
    {
        [DisplayName("Day")]
        public DayOfWeek DayOfWeek { get; set; }

        [IgnoreInGrid]
        public int WorkoutPeriodId { get; set; }

        [DisplayName("Workout period")]
        [NavigationProperty(typeof(WorkoutPeriod), TextField = "Description")]
        public WorkoutPeriod? WorkoutPeriod { get; set; }

        [IgnoreInGrid]
        public int TemplateWorkoutId { get; set; }

        [DisplayName("Workout")]
        [NavigationProperty(typeof(TemplateWorkout), TextField = "Name")]
        public TemplateWorkout? TemplateWorkout { get; set; }

        [DisplayName("Sort order")]
        public int SortOrder { get; set; }
    }
}
