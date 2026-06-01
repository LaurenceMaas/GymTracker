using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerDataModel.Models
{
    public class ExerciseType 
    {
        public int Id { get; set; }
        [MaxLength(500)]
        [DisplayName("Exercise Type")]
        public string? Description { get; set; }
    }
}
