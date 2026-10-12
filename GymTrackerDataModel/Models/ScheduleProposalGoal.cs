namespace GymTrackerDataModel.Models
{
    public class ScheduleProposalGoal
    {
        public int ScheduleProposalId { get; set; }
        public ScheduleProposal ScheduleProposal { get; set; } = null!;
        public int TrainingGoalId { get; set; }
        public TrainingGoal TrainingGoal { get; set; } = null!;
    }
}
