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
    public class DOWWorkout
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayName("Nr")]
        public int Id { get; set; }
        [IgnoreInGrid]
        public Guid UserId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        [IgnoreInGrid]
        public int WorkoutPeriodId { get; set; }
        [DisplayName("WorkoutPeriod")]
        [NavigationProperty(typeof(WorkoutPeriod), TextField = "Name")]
        public WorkoutPeriod WorkoutPeriod { get; set; }
        [IgnoreInGrid]
        public int TemplateWorkoutId { get; set; }
        [DisplayName("Workout")]
        [NavigationProperty(typeof(TemplateWorkout), TextField = "Name")]
        public TemplateWorkout TemplateWorkout { get; set; }
        [IgnoreInGrid]
        public int SortOrder { get; set; }
    }

}
