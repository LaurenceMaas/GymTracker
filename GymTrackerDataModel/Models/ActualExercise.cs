using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerDataModel.Models
{
    public class ActualExercise
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ActualWorkoutId { get; set; }
        public ActualWorkout? ActualWorkout { get; set; }
        public int ExerciseId { get; set; }
        public Exercise? Exercise { get; set; }
        [MaxLength(250)]
        public string ExerciseName { get; set; } = "";
        [MaxLength(500)]
        public string? ExerciseType { get; set; }
        public DateTime Createdatetime { get; set; }
    }
}
