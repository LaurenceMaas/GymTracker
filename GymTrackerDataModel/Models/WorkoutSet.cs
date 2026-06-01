using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerDataModel.Models
{
    public class WorkoutSet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int WorkoutExerciseId { get; set; }
        public WorkoutExercise WorkoutExercise { get; set; } = null!;
        [Required]
        [Range(1, int.MaxValue)]
        public int SetNumber { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Reps { get; set; }
        [Required]
        [Range(typeof(decimal), "0", "9999.99")]
        public decimal Weight { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int? DurationSeconds { get; set; }
    }
}
