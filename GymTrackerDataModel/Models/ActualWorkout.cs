using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace GymTrackerDataModel.Models
{
    public class ActualWorkout
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [IgnoreInGrid]
        public int Id { get; set; }
        [IgnoreInGrid]
        public Guid PerformedByUserId { get; set; }
        [IgnoreInGrid]
        public int DOWWorkoutId { get; set; }
        [DisplayName("Workout")]
        [NavigationProperty(typeof(DOWWorkout), TextField = "Name")]
        public DOWWorkout? DOWWorkout { get; set; }
        public DateTime WorkoutStartDate { get; set; }
        public TimeSpan WorkoutStartTime { get; set; }
        public DateTime WorkoutEndDate { get; set; }
        public TimeSpan WorkoutEndTime { get; set; }
        [MaxLength(1024)]
        public string? Notes { get; set; }
    }
}
