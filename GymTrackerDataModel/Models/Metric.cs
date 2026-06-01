using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerDataModel.Models
{
    public class Metric
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(255)]
        public string? Name { get; set; }
        [MaxLength(255)]// Reps, Weight, Duration
        public string? Unit { get; set; }       // kg, min, km
        public MetricValueType ValueType { get; set; } // Numeric, Text
        public ICollection<TemplateSetMetric> TemplateSetMetrics { get; set; } = new List<TemplateSetMetric>();
        public ICollection<ExerciseMetric> ExerciseMetrics { get; set; }
    }

    public enum MetricValueType
    {
        Numeric,
        Text
    }
}
