namespace GymTrackerDataModel.Models
{
    public class TemplateTrainingGoal
    {
        public int TemplateWorkoutId { get; set; }
        public TemplateWorkout TemplateWorkout { get; set; } = null!;
        public int TrainingGoalId { get; set; }
        public TrainingGoal TrainingGoal { get; set; } = null!;
    }
}
