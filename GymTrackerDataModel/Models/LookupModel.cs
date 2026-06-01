using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerDataModel.Models
{
    public class LookupModel
    {
        public int Id { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
