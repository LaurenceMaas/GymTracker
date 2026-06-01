using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerDataModel.Models
{
    public class TemplateSetMetric
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TemplateSetId { get; set; }
        public TemplateSet TemplateSet { get; set; } = null!;
        public int MetricId { get; set; }
        public Metric Metric { get; set; }        
        public decimal? NumericValue { get; set; }
        [MaxLength(255)]
        public string? TextValue { get; set; }
    }
}
