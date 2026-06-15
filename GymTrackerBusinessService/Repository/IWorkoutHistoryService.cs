using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerBusinessService.Repository
{
    public interface IWorkoutHistoryService
    {
        Task<List<WorkoputHeaderDisplayVM>> LoadWorkoutHistoryPerUser(string UserId);
    }

    public class WorkoputHeaderDisplayVM
    {
        [IgnoreInGrid]
        public int TemplateWorkoutId { get; set; }
        [DisplayName("Workout Name")]
        [MaxLength(255)]
        public string? WorkoutName { get; set; }
        [DisplayName("Day Of the Week")]
        public DayOfWeek DayOfWeek { get; set; }
        [DisplayName("Workout Start")]
        public DateTime WorkoutStrtDtTime { get; set; }
        [DisplayName("Workout End")]
        public DateTime WorkoutEndDtTime { get; set; }
        public string? Notes { get; set; }
    }


    public class WorkoutHistory : IWorkoutHistoryService
    {
        protected EntityDBContext _context = null;
        protected DbSet<ActualWorkout> _awTable = null;
        protected DbSet<DOWWorkout> _dwTable = null;
        protected DbSet<TemplateWorkout> _twTable = null;
        public WorkoutHistory(EntityDBContext context)
        {
            _context = context;
            _awTable = _context.Set<ActualWorkout>();
            _dwTable = _context.Set<DOWWorkout>();
            _twTable = _context.Set<TemplateWorkout>(); 
        }

        Task<List<WorkoputHeaderDisplayVM>> IWorkoutHistoryService.LoadWorkoutHistoryPerUser(string UserId)
        {
            throw new NotImplementedException();
        }
        //public async Task<List<WorkoputHeaderDisplayVM>> LoadWorkoutHistoryPerUser(string UserId)
        //{
        //    var WorkoputHeaderDisplayVM = await
        //    (
        //    from actualWorkout in _awTable
        //    join dowWorkout in _dwTable on actualWorkout.DOWWorkoutId equals dowWorkout.Id
        //    join templateworkout in _twTable on dowWorkout.TemplateWorkoutId equals templateworkout.Id
        //    where dowWorkout.UserId.Equals(UserId)
        //    select new
        //    {
        //        TemplateWorkoutId = dowWorkout.TemplateWorkoutId,
        //        DayOfWeek = dowWorkout.DayOfWeek,
        //        WorkoutStrtDtTime = new DateTime(DateOnly.FromDateTime(actualWorkout.WorkoutStartTime), TimeOnly.FromTimeSpan(actualWorkout.WorkoutStartTime)),
        //        WorkoutEndDtTime = new DateTime(DateOnly.FromDateTime(actualWorkout.WorkoutEndDate), TimeOnly.FromTimeSpan(actualWorkout.WorkoutEndTime)),
        //        Notes = actualWorkout.Notes,
        //        WorkoutName = templateworkout.Name
        //    }).ToListAsync();

        //    return WorkoputHeaderDisplayVM.GroupBy(x => new
        //    {
        //        x.TemplateWorkoutId,
        //        x.DayOfWeek,
        //        x.WorkoutStrtDtTime,
        //        x.WorkoutEndDtTime,
        //        x.Notes,
        //        x.WorkoutName
        //    }).Select( y=> new WorkoputHeaderDisplayVM 
        //    {
        //        TemplateWorkoutId = y.Key.TemplateWorkoutId,
        //        DayOfWeek = y.Key.DayOfWeek,
        //        WorkoutStrtDtTime = y.Key.WorkoutStrtDtTime,
        //        WorkoutEndDtTime = y.Key.WorkoutEndDtTime,   
        //        Notes = y.Key.Notes,
        //        WorkoutName = y.Key.WorkoutName
        //    }).ToList(); 


        //}
    }
}
