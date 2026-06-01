using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerDataModel.Models
{
    public class TemplateSet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TemplateExerciseId { get; set; }
        public TemplateExercise TemplateExercise { get; set; } = null!;
        [Required]
        [Range(1, int.MaxValue)]
        public int Order { get; set; }
        public ICollection<TemplateSetMetric> TemplateSetMetrics { get; set; } = new List<TemplateSetMetric>();
    }
}
