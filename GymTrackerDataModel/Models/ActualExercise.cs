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
        public int TemplateExerciseId { get; set; }
        public TemplateExercise? TemplateExercise { get; set; }
        public int ActualTemplateExerciseId { get; set; }
        public TemplateExercise? ActualTemplateExercise { get; set; }
        public DateTime Createdatetime { get; set; }
    }
}
