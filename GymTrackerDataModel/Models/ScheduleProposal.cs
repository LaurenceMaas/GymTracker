using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymTrackerDataModel.Models
{
    public class ScheduleProposal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public ProposalStatus Status { get; set; }
        public int FitnessLevelId { get; set; }
        public FitnessLevel FitnessLevel { get; set; } = null!;
        public int RecommendedSessionCount { get; set; }
        public string? Explanation { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? AbandonedAt { get; set; }
        public ICollection<ScheduleProposalGoal> Goals { get; set; } = new List<ScheduleProposalGoal>();
        public ICollection<ScheduleProposalSlot> Slots { get; set; } = new List<ScheduleProposalSlot>();
    }
}
