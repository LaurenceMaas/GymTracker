using GymTrackerDataModel.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerDataModel.Models
{
    public class TemplateWorkout : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayName("Nr")]
        public int Id { get; set; }
        [Required]
        [MaxLength(250)]
        [DisplayName("Workout Name")]
        public string Name { get; set; } = "";
        public string? Notes { get; set; }
        public ICollection<TemplateExercise> Exercises { get; set; } = new List<TemplateExercise>();
    }
}
