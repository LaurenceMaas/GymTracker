using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerDataModel.Models
{
    public class ActualSet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ActualExerciseId { get; set; }
        public ActualExercise? ActualExercise { get; set; }
        public int TemplateSetId { get; set; }
        public TemplateSet? TemplateSet { get; set; }
        public int ExecutionOrder { get; set; }
        public bool Completed { get; set; }
        public DateTime Createdatetime { get; set; }
    }
}
