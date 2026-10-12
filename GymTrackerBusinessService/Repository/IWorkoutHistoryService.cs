using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
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
        public DateTime? WorkoutEndDtTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }


    public class WorkoutHistory : IWorkoutHistoryService
    {
        protected EntityDBContext _context = null;
        protected DbSet<ActualWorkout> _awTable = null;
        protected DbSet<WorkOutStatus> _wsTable = null;
        public WorkoutHistory(EntityDBContext context)
        {
            _context = context;
            _awTable = _context.Set<ActualWorkout>();
            _wsTable= _context.Set<WorkOutStatus>();
        }

        public async Task<List<WorkoputHeaderDisplayVM>> LoadWorkoutHistoryPerUser(string UserId)
        {
            return await
            (
            from actualWorkout in _awTable
            join workoutStatus in _wsTable on actualWorkout.WorkOutStatusId equals workoutStatus.Id
            where actualWorkout.PerformedByUserId.ToString().ToUpper().Equals(UserId.ToUpper())
            orderby actualWorkout.WorkoutStartDate descending, actualWorkout.WorkoutStartTime descending
            select new WorkoputHeaderDisplayVM
            {
                TemplateWorkoutId = 0,
                DayOfWeek = actualWorkout.DayOfWeek,
                WorkoutStrtDtTime = new DateTime(DateOnly.FromDateTime(actualWorkout.WorkoutStartDate), TimeOnly.FromTimeSpan(actualWorkout.WorkoutStartTime)),
                WorkoutEndDtTime = actualWorkout.WorkoutEndDate == null || actualWorkout.WorkoutEndTime == null
                    ? null
                    : new DateTime(DateOnly.FromDateTime(actualWorkout.WorkoutEndDate.Value), TimeOnly.FromTimeSpan(actualWorkout.WorkoutEndTime.Value)),
                Notes = actualWorkout.Notes,
                WorkoutName = actualWorkout.WorkoutName,
                Status = workoutStatus.Description
            }).ToListAsync();
        }
    }
}
