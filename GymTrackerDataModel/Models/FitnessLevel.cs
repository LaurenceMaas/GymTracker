using System.ComponentModel.DataAnnotations;

namespace GymTrackerDataModel.Models
{
    public class FitnessLevel : LookupModel
    {
        public string? Qualification { get; set; }
        public string? StrengthLoading { get; set; }
        [MaxLength(200)]
        public string? WeeklyTraining { get; set; }
        public int MinSessionsPerWeek { get; set; }
        public int MaxSessionsPerWeek { get; set; }
        [MaxLength(1000)]
        public string? Reference { get; set; }
    }
}
