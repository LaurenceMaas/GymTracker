using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using GymTrackerDataModel.Interfaces;

namespace GymTrackerDataModel.Models
{
    public class Exercise : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayName("Nr")]
        public int Id { get; set; }
        [Required]
        [MaxLength(250)]
        [DisplayName("Exercise Name")]
        public string Name { get; set; } = "";
        [IgnoreInGrid]
        public int ExerciseTypeId { get; set; }
        [DisplayName("Exercise Type")]
        [NavigationProperty(typeof(ExerciseType), TextField = "Name")]
        public ExerciseType ExerciseType { get; set; }
        [DisplayName("Steps To Perform")]
        public string? StepsToPerform { get; set; } = "";
        [MaxLength(500)]
        [RenderAs(RenderType.Video)]
        [DisplayName("Video Link")]
        public string? VideoLink { get; set; }
        public ICollection<ExerciseMetric> ExerciseMetrics { get; set; }

    }
}
