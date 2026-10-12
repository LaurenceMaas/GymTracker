using System.ComponentModel;
using GymTrackerDataModel.Models;

namespace GymTrackerBusinessService.Scheduling
{
    public sealed class SchedulePreferenceVM
    {
        [IgnoreInGrid]
        public int FitnessLevelId { get; set; }

        [DisplayName("Fitness level")]
        [NavigationProperty(typeof(FitnessLevel), TextField = "Description")]
        public FitnessLevel? FitnessLevel { get; set; }

        [DisplayName("Weekly training")]
        [ReadOnly(true)]
        public string? WeeklyTraining { get; set; }

        public List<SchedulePreferenceGoalVM> Goals { get; set; } = new();
    }

    public sealed class SchedulePreferenceGoalVM
    {
        [IgnoreInGrid]
        public int TrainingGoalId { get; set; }

        [DisplayName("Training goal")]
        [NavigationProperty(typeof(TrainingGoal), TextField = "Description")]
        public TrainingGoal? TrainingGoal { get; set; }
    }
}
