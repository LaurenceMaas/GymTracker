using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerDataModel.Models
{
    public class Model
    {
        [IgnoreInGrid]
        public DateTime CreatedDate { get; set; }
        [IgnoreInGrid]
        public DateTime UpdatedDate { get; set; }
        [IgnoreInGrid]
        [Required]
        [MaxLength(500)]
        public string CreatedBy  { get; set; } = "";
        [IgnoreInGrid]
        [Required]
        [MaxLength(500)]
        public string UpdatedBy { get; set; } = "";
    }
}
