using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerDataModel.Models
{
    public class WorkoutExercise
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int WorkoutId { get; set; }
        public WorkOut Workout { get; set; } = null!;
        public int ExerciseId { get; set; }
        public Exercise Exercise { get; set; } = null!;
        [Required]
        [Range(1, int.MaxValue)]
        public int Order { get; set; }
        public ICollection<WorkoutSet> Sets { get; set; } = new List<WorkoutSet>();
    }
}
