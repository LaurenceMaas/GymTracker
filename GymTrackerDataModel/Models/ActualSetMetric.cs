using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace GymTrackerDataModel.Models
{
    public class ActualSetMetric
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ActualSetId { get; set; }
        public ActualSet? ActualSet { get; set; }
        public int TemplateSetMetricId { get; set; }
        public TemplateSetMetric? TemplateSetMetric { get; set; }
        [MaxLength(255)]
        public string? PlannedTextValue { get; set; }
        public decimal PlannedNumericValue { get; set; }
        [MaxLength(255)]
        public string? ActualTextValue { get; set; }
        public decimal ActualNumericValue { get; set; }
        public DateTime Createdatetime { get; set; }

    }
}
