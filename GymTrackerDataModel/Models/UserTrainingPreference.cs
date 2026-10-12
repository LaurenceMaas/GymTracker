using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymTrackerDataModel.Models
{
    public class UserTrainingPreference
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int FitnessLevelId { get; set; }
        public FitnessLevel FitnessLevel { get; set; } = null!;
        public ICollection<UserTrainingPreferenceGoal> Goals { get; set; } = new List<UserTrainingPreferenceGoal>();
    }
}
