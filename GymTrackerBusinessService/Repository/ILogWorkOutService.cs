using GymTrackerBusinessService.Generic;
using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerBusinessService.Repository
{
    public interface ILogWorkOutService
    {
        Task<List<DOWWorkout>> LoadDOWDataPerUserCurrentDateTime(string UserId);
        Task<List<LogWorkoutLogExerciseVM>> GetExercises(int workOutTemplateId);
        Task<List<LogWorkoutSetVM>> LogExerciseData(LogWorkoutLogExerciseVM workoutLogExerciseVM);
        Task SaveNewActualWorkOut(Guid user,int DOWWorkoutId,DateTime startDateTime,string? notes);
    }
    public interface IDynamicDisplayName
    {
        string GetDisplayName(PropertyInfo prop);
    }
    public class LogWorkoutLogExerciseVM
    {
        [IgnoreInGrid]
        public int TemplateExerciseId { get; set; }
        [ReadOnly(true)]
        public int ExerciseNumber { get; set; }
        [ReadOnly(true)]
        public string? ExerciseName { get; set; }
        public string? Notes { get; set; }
    }
    public class LogWorkoutSetVM
    {
        [IgnoreInGrid]
        public int TemplateSetId { get; set; }
        public int SetNumber { get; set; }
        [ReadOnly(true)]
        public string? PlannedSetMetrics { get; set; }
        [Collection]
        public List<LogWorkoutSetMetricVM>? LogWorkoutSetMetricVMs { get; set; }
        public bool Complete { get; set; }
    }
    public class LogWorkoutSetMetricVM : IDynamicDisplayName
    {        
        [IgnoreInGrid]
        public int TemplateSetId { get; set; }
        [IgnoreInGrid]
        public int MetricId { get; set; }
        [IgnoreInGrid]
        public string MetricName { get; set; }
        [IgnoreInGrid]
        public decimal? PlannedNumericValue { get; set; }
        [DisplayName("Actual")]
        public decimal? ActualNumericValue { get; set; }
        [IgnoreInGrid]
        public string? PlannedTextValue { get; set; }
        [DisplayName("Actual")]
        public string? ActualTextValue { get; set; }
        [IgnoreInGrid]
        public string? Unit { get; set; }
        [IgnoreInGrid]
        public string ActualLabel { get; set; }
        public string GetDisplayName(PropertyInfo prop)
        {
            if (prop.Name == nameof(ActualNumericValue))
            {
                if (PlannedNumericValue != null)
                    return $"{MetricName} ({PlannedNumericValue}{Unit})";

                return MetricName;
            }

            if (prop.Name == nameof(ActualTextValue))
            {
                if (!string.IsNullOrEmpty(PlannedTextValue))
                    return $"{MetricName} ({PlannedTextValue})";

                return MetricName;
            }

            return prop.Name;
        }
    }
    public class LogWorkOutService : ILogWorkOutService
    {
        protected EntityDBContext _context = null;
        protected DbSet<TemplateWorkout> _wtTable = null;
        protected DbSet<TemplateExercise> _tetable = null;
        protected DbSet<Exercise> _etable = null;
        protected DbSet<TemplateSet> _tstable = null;
        protected DbSet<TemplateSetMetric> _tsmtable = null;
        protected DbSet<Metric> _mtable = null;
        protected DbSet<ExerciseType> _ettable = null;
        protected DbContextOptionsBuilder<EntityDBContext>? _optionsBuilder;
        public LogWorkOutService(EntityDBContext context)
        {
            _context = context;
            _wtTable = _context.Set<TemplateWorkout>();
            _tetable = _context.Set<TemplateExercise>();
            _etable = _context.Set<Exercise>();
            _tstable = _context.Set<TemplateSet>();
            _tsmtable = _context.Set<TemplateSetMetric>();
            _mtable = _context.Set<Metric>();
            _ettable = _context.Set<ExerciseType>();
        }
        public async Task<List<LogWorkoutLogExerciseVM>> GetExercises(int workOutTemplateId)
        {
            int i = 0;
            var logExerciseVM = (
            await
            (
            from templateWorkout in _wtTable
            join templateExercise in _tetable on templateWorkout.Id equals templateExercise.WorkoutTemplateId
            join exercise in _etable on templateExercise.ExerciseId equals exercise.Id
            where templateWorkout.Id == workOutTemplateId
            select new
            {
                TemplateExerciseId = templateExercise.Id,
                ExerciseName = exercise.Name,
            }
            )
            .ToListAsync())
            .Select((x, index) => new
            {
                RowNumber = index + 1,
                x.ExerciseName,
                x.TemplateExerciseId
            })
            .ToList();

            List<LogWorkoutLogExerciseVM> result = logExerciseVM
            .GroupBy(x => new
            {
                x.ExerciseName,
                x.RowNumber,
                x.TemplateExerciseId
            })
            .Select(g => new LogWorkoutLogExerciseVM
            {
                ExerciseNumber = g.Key.RowNumber,
                ExerciseName = g.Key.ExerciseName,
                TemplateExerciseId = g.Key.TemplateExerciseId

            }
            ).ToList();

            return result ?? new List<LogWorkoutLogExerciseVM>();
        }
        public async Task<List<LogWorkoutSetVM>> LogExerciseData(LogWorkoutLogExerciseVM workoutLogExerciseVM)
        {
            StringBuilder plannedSB = new StringBuilder();
            int cnt;
            string at = string.Empty; 
            string planned = string.Empty;

            var rawData = (await
            (
            from templateSet in _tstable
            join templateSetMetric in _tsmtable on templateSet.Id equals templateSetMetric.TemplateSetId
            join metric in _mtable on templateSetMetric.MetricId equals metric.Id
            where templateSet.TemplateExerciseId == workoutLogExerciseVM.TemplateExerciseId
            select new
            {
                TemplateSetId = templateSet.Id,
                MetricId = metric.Id,
                MetricName = metric.Name,
                NumericValue = templateSetMetric.NumericValue,
                TextValue = templateSetMetric.TextValue,
                Unit = metric.Unit
            }
            ).ToListAsync());

            var formattedData = rawData
            .GroupBy(x => new
            {
                x.TemplateSetId,
            })
            .Select(g => new LogWorkoutSetVM
            {
                TemplateSetId = g.Key.TemplateSetId,

                LogWorkoutSetMetricVMs = g.Select(m => new LogWorkoutSetMetricVM
                {
                    MetricId = m.MetricId,
                    Unit = m.Unit,
                    MetricName = m.MetricName,
                    PlannedNumericValue = m.NumericValue,
                    PlannedTextValue = m.TextValue,
                    TemplateSetId = m.TemplateSetId,
                    ActualNumericValue = 0,
                    ActualTextValue = "",
                    ActualLabel =
        m.NumericValue != null
        ? $"{m.MetricName} ({m.NumericValue}{m.Unit})"
        : $"{m.MetricName} ({m.TextValue})"
                }).ToList()
            })
            .OrderBy(x => x.SetNumber)
            .ToList();

            for (int i = 0; i < formattedData.Count; i++)
            {
                plannedSB.Clear();
                formattedData.ElementAt(i).SetNumber = (i + 1);
                cnt = 1;
                foreach(var setMetric in formattedData.ElementAt(i).LogWorkoutSetMetricVMs)
                {
                    at = cnt == formattedData.ElementAt(i).LogWorkoutSetMetricVMs.Count ? "" : " @\r\n";
                    plannedSB.Append((setMetric.PlannedNumericValue != null ? setMetric.PlannedNumericValue : "" + setMetric.PlannedTextValue) + " " + setMetric.Unit + " " + setMetric.MetricName + at );
                    cnt++;
                }

                formattedData.ElementAt(i).PlannedSetMetrics = plannedSB.ToString();
            }

            return formattedData;
        }
        public async Task SaveNewActualWorkOut(Guid user, int DOWWorkoutId, DateTime startDateTime, string? notes, bool completed)
        {
            if (completed)
            {
                _optionsBuilder = Helpers.BuildOptions();
                IGenericRepoService<ActualWorkout> genericRepoService = new GenericRepoService<ActualWorkout>(new EntityDBContext(_optionsBuilder.Options));
                ActualWorkout actualWorkoutSave = new ActualWorkout()
                {
                    Id = 0,
                    PerformedByUserId = user,
                    DOWWorkoutId = DOWWorkoutId,
                    WorkoutStartDate = startDateTime.Date,
                    WorkoutStartTime = startDateTime.TimeOfDay,
                    WorkoutEndDate = null,
                    WorkoutEndTime = null,
                    Notes = notes,
                    WorkOutStatusId = (int)workoutStatus.Started

                };

                await genericRepoService.InsertAsync(actualWorkoutSave);
            }
        }
        public async Task<List<DOWWorkout>> LoadDOWDataPerUserCurrentDateTime(string UserId)
        {
            _optionsBuilder = Helpers.BuildOptions();
            DayOfWeek currentDOW = DateTime.Now.DayOfWeek;
            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            IGenericRepoService<DOWWorkout> genericRepoService = new GenericRepoService<DOWWorkout>(new EntityDBContext(_optionsBuilder.Options));
            return (await genericRepoService.GetAllAsync()).ToList().FindAll(x => x.UserId.ToString().Equals(UserId)
            && x.WorkoutPeriod.StartTime.CompareTo(currentTime) < 0
            && x.WorkoutPeriod.EndTime.CompareTo(currentTime) >= 0
            && x.DayOfWeek == currentDOW).ToList() ?? new List<DOWWorkout>();
        }
    }

    public enum workoutStatus
    {
        Started =1,
        Abandoned=2,
        Ended=3
    }

}
