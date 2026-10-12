namespace GymTrackerDataModel.Models
{
    public class UserTrainingPreferenceGoal
    {
        public int UserTrainingPreferenceId { get; set; }
        public UserTrainingPreference UserTrainingPreference { get; set; } = null!;
        public int TrainingGoalId { get; set; }
        public TrainingGoal TrainingGoal { get; set; } = null!;
    }
}
