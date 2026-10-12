using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymTrackerDataModel.Models
{
    public class ScheduleProposalSlot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ScheduleProposalId { get; set; }
        public ScheduleProposal ScheduleProposal { get; set; } = null!;
        public DayOfWeek DayOfWeek { get; set; }
        public int WorkoutPeriodId { get; set; }
        public WorkoutPeriod WorkoutPeriod { get; set; } = null!;
        public int TemplateWorkoutId { get; set; }
        public TemplateWorkout TemplateWorkout { get; set; } = null!;
        public int SortOrder { get; set; }
    }
}
