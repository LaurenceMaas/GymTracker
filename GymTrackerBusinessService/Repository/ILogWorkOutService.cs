using GymTrackerBusinessService.Generic;
using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GymTrackerBusinessService.Repository
{
    public interface ILogWorkOutService
    {
        Task<List<DOWWorkout>> LoadDOWDataPerUserCurrentDateTime(string UserId);
        Task<List<LogWorkoutLogExerciseVM>> GetExercises(int workOutTemplateId, Guid user);
        Task<List<LogWorkoutSetVM>> GetExerciseData(LogWorkoutLogExerciseVM workoutLogExerciseVM);
        Task<int> SaveNewActualWorkOut(Guid user,int DOWWorkoutId,DateTime startDateTime,string? notes);
        Task ResetActualWorkOut(Guid user);
        ActualWorkout GetCurrentWorkout(Guid user);
        Task CompleteSet(LogWorkoutSetVM set, int actualWOId);
        Task CompleteWorkout(int actualWOId);
    }
    public interface IDynamicDisplayName
    {
        string GetDisplayName(PropertyInfo prop);
    }
    public class LogWorkoutLogExerciseVM
    {
        [IgnoreInGrid]
        public int ActualExerciseId { get; set; }
        [IgnoreInGrid]
        public int TemplateExerciseId { get; set; }
        [ReadOnly(true)]
        public int ExerciseNumber { get; set; }
        [ReadOnly(true)]
        public string? ExerciseName { get; set; }
    }
    public class LogWorkoutSetVM
    {
        [IgnoreInGrid]
        public int TemplateExerciseId { get; set; }
        [IgnoreInGrid]
        public int TemplateSetId { get; set; }
        public int SetNumber { get; set; }
        [ReadOnly(true)]
        public string? PlannedSetMetrics { get; set; }
        [Collection]
        public List<LogWorkoutSetMetricVM>? LogWorkoutSetMetricVMs { get; set; }
        public bool Complete { get; set; }
        [IgnoreInGrid]
        public int Order { get; set; }
    }
    public class LogWorkoutSetMetricVM : IDynamicDisplayName
    {        
        [IgnoreInGrid]
        public int TemplateSetMetricId { get; set; }
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
        protected DbSet<ActualWorkout> _awtable = null;
        protected DbSet<DOWWorkout> _dwtable = null;
        protected DbSet<WorkoutPeriod> _wptable = null;
        protected DbSet<ActualExercise> _aetable = null;
        protected DbSet<ActualSet> _astable = null;
        protected DbSet<ActualSetMetric> _amtable = null;

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
            _awtable= _context.Set<ActualWorkout>();
            _aetable = _context.Set<ActualExercise>();
            _dwtable = _context.Set<DOWWorkout>();
            _wptable = _context.Set<WorkoutPeriod>();
            _astable = _context.Set<ActualSet>();
            _amtable = _context.Set<ActualSetMetric>();
        }
        public async Task<List<LogWorkoutLogExerciseVM>> GetExercises(int workOutTemplateId, Guid user)
        {
            ActualWorkout aw;
            aw = GetCurrentWorkout(user);
            if (aw.WorkOutStatusId == (int)workoutStatus.Ended)
            {
                return new List<LogWorkoutLogExerciseVM>();
            }

            var logExerciseVM =
            (
            await
            (
            from templateExercise in _tetable
            join exercise in _etable on templateExercise.ExerciseId equals exercise.Id
            join actualExerciseGroup in _aetable.Where(x => x.ActualWorkoutId == aw.Id) on templateExercise.Id equals actualExerciseGroup.TemplateExerciseId
            into actualExerciseJoin
            from actualExercise in actualExerciseJoin.DefaultIfEmpty()
            where templateExercise.WorkoutTemplateId == workOutTemplateId
            select new
            {
                TemplateExerciseId = templateExercise.Id,
                ExerciseName = exercise.Name,
                ActualExerciseId = actualExercise == null
            ? 0
            : actualExercise.Id
            }
            )
            .ToListAsync()
            )
            .Select((x, index) => new
            {
                RowNumber = index + 1,
                x.ExerciseName,
                x.TemplateExerciseId,
                x.ActualExerciseId
            })
            .ToList();        

            List<LogWorkoutLogExerciseVM> result = logExerciseVM
            .GroupBy(x => new
            {
                x.ExerciseName,
                x.RowNumber,
                x.TemplateExerciseId,
                x.ActualExerciseId
            })
            .Select(g => new LogWorkoutLogExerciseVM
            {
                ExerciseNumber = g.Key.RowNumber,
                ExerciseName = g.Key.ExerciseName,
                TemplateExerciseId = g.Key.TemplateExerciseId,
                ActualExerciseId = g.Key.ActualExerciseId
            }
            ).ToList();


            return result ?? new List<LogWorkoutLogExerciseVM>();
        }
        public async Task<List<LogWorkoutSetVM>> GetExerciseData(LogWorkoutLogExerciseVM workoutLogExerciseVM)
        {
            StringBuilder plannedSB = new StringBuilder();
            int cnt;
            string at = string.Empty;
            string planned = string.Empty;
            LogWorkoutSetVM logWorkoutSetVM;

            var rdbData = (await
            (
            from templateSet in _tstable
            join templateSetMetric in _tsmtable on templateSet.Id equals templateSetMetric.TemplateSetId
            join metric in _mtable on templateSetMetric.MetricId equals metric.Id
            where templateSet.TemplateExerciseId == workoutLogExerciseVM.TemplateExerciseId
            select new
            {
                TemplateExerciseId = templateSet.TemplateExerciseId,
                TemplateSetId = templateSet.Id,
                MetricId = metric.Id,
                MetricName = metric.Name,
                NumericValue = templateSetMetric.NumericValue,
                TextValue = templateSetMetric.TextValue,
                Unit = metric.Unit,
                Order = templateSet.Order,
                TemplateSetMetricId = templateSetMetric.Id
            }
            ).ToListAsync());

            var formattedData = rdbData
            .GroupBy(x => new
            {
                x.TemplateSetId,
                x.TemplateExerciseId,
                x.Order               
            })
            .Select(g => new LogWorkoutSetVM
            {
                TemplateSetId = g.Key.TemplateSetId,
                TemplateExerciseId = g.Key.TemplateExerciseId,
                Order = g.Key.Order,
                LogWorkoutSetMetricVMs = g.Select(m => new LogWorkoutSetMetricVM
                {
                    MetricId = m.MetricId,
                    Unit = m.Unit,
                    MetricName = m.MetricName,
                    PlannedNumericValue = m.NumericValue,
                    PlannedTextValue = m.TextValue,
                    TemplateSetMetricId = m.TemplateSetMetricId,
                    ActualNumericValue = m.NumericValue,
                    ActualTextValue = m.TextValue,
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
                logWorkoutSetVM = formattedData.ElementAt(i);
                foreach (var setMetric in logWorkoutSetVM.LogWorkoutSetMetricVMs)
                {
                    at = cnt == formattedData.ElementAt(i).LogWorkoutSetMetricVMs.Count ? "" : " @\r\n";
                    plannedSB.Append((setMetric.PlannedNumericValue != null ? setMetric.PlannedNumericValue : "" + setMetric.PlannedTextValue) + " " + setMetric.Unit + " " + setMetric.MetricName + at);

                    var actualSetMetric = (await
                    (from actualMet in _amtable
                     where (actualMet.TemplateSetMetricId == setMetric.TemplateSetMetricId)
                     select new
                     {
                         ActualNumericValue = actualMet.ActualNumericValue,
                         ActualTextValue = actualMet.ActualTextValue
                     }
                    ).ToListAsync()).FirstOrDefault() ?? null;

                    if (actualSetMetric != null)
                    {
                        setMetric.ActualNumericValue = actualSetMetric.ActualNumericValue;
                        setMetric.ActualTextValue = actualSetMetric.ActualTextValue;
                    }
                    cnt++;
                }

                var readComplete = (await (from actualSet in _astable
                                           where (actualSet.ActualExerciseId == workoutLogExerciseVM.ActualExerciseId
                                           && actualSet.TemplateSetId == logWorkoutSetVM.TemplateSetId)
                                           select new
                                           {
                                               Completed = actualSet.Completed
                                           }
                ).ToListAsync()).FirstOrDefault();

                logWorkoutSetVM.Complete = readComplete == null ? false : readComplete.Completed;
                logWorkoutSetVM.PlannedSetMetrics = plannedSB.ToString();
            }

            return formattedData;
        }
        public async Task<int> SaveNewActualWorkOut(Guid user, int DOWWorkoutId, DateTime startDateTime, string? notes)
        {
            _optionsBuilder = Helpers.BuildOptions();
            IGenericRepoService<ActualWorkout> genericRepoService = new GenericRepoService<ActualWorkout>(new EntityDBContext(_optionsBuilder.Options));
            int woId = GetCurrentWorkout(user).Id;
            if (woId == 0)
            {
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

                return await genericRepoService.InsertAsync(actualWorkoutSave);
            }
            else
            {
                return woId;
            }
        }
        public async Task ResetActualWorkOut(Guid user)
        {
            ActualWorkout actualWorkout;
            _optionsBuilder = Helpers.BuildOptions();
            IGenericRepoService<ActualWorkout> genericRepoService = new GenericRepoService<ActualWorkout>(new EntityDBContext(_optionsBuilder.Options));
            int workoputId = GetCurrentWorkout(user).Id;
            List<int> exerciseIds;
            List<int> setIds;

            if (workoputId > 0)
            {
                actualWorkout = await genericRepoService.GetByIdAsync(workoputId);
                actualWorkout.WorkOutStatusId = (int)workoutStatus.Abandoned;
                await genericRepoService.UpdateAsync(actualWorkout);

                var exercises = _context.TRN_ActualExercise.Where(x => x.ActualWorkoutId == actualWorkout.Id);
                exerciseIds = await exercises.Select(x => x.Id).ToListAsync();
                var sets = _context.TRN_ActualSet.Where(x => exerciseIds.Any(y => y == x.ActualExerciseId));
                setIds = await sets.Select(z => z.Id).ToListAsync();

                await exercises.ExecuteDeleteAsync();               
                await sets.ExecuteDeleteAsync();
                await _context.TRN_ActualSetMetric.Where(x => setIds.Any(y => y == x.ActualSetId)).ExecuteDeleteAsync();
            }
        }
        public ActualWorkout GetCurrentWorkout(Guid user)
        {
            int workoutPeriod = (from woPeriod in _wptable
                                 where DateTime.Now.TimeOfDay >= woPeriod.StartTime
                                 && DateTime.Now.TimeOfDay < woPeriod.EndTime
                                 select new
                                 {
                                     Id = woPeriod.Id
                                 }).ToList().FirstOrDefault().Id;

            var queryRes =
            (
            from actualWorkout in _awtable
            join dowworkout in _dwtable on actualWorkout.DOWWorkoutId equals dowworkout.Id
            where actualWorkout.PerformedByUserId == user
            && DateOnly.FromDateTime(actualWorkout.WorkoutStartDate) == DateOnly.FromDateTime(DateTime.Now)
            && dowworkout.WorkoutPeriodId == workoutPeriod
            select new ActualWorkout
            {
                Id = actualWorkout.Id,
                PerformedByUserId = actualWorkout.PerformedByUserId,
                DOWWorkoutId = actualWorkout.DOWWorkoutId,
                WorkoutStartDate = actualWorkout.WorkoutStartDate,
                WorkoutStartTime = actualWorkout.WorkoutStartTime,
                WorkoutEndDate = actualWorkout.WorkoutEndDate,
                WorkoutEndTime = actualWorkout.WorkoutEndTime,
                Notes = actualWorkout.Notes,
                WorkOutStatusId = actualWorkout.WorkOutStatusId
            }
            ).ToList();

            return queryRes.Count == 0 ? new ActualWorkout() : queryRes.FirstOrDefault();
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
        public async Task CompleteSet(LogWorkoutSetVM set,int actualWOId)
        {
            _optionsBuilder = Helpers.BuildOptions();
            IGenericRepoService<ActualExercise> actExRepoServ = new GenericRepoService<ActualExercise>(new EntityDBContext(_optionsBuilder.Options));
            IGenericRepoService<ActualSet> actSetRepoServ = new GenericRepoService<ActualSet>(new EntityDBContext(_optionsBuilder.Options));
            IGenericRepoService<ActualSetMetric> actSetMetRepoServ = new GenericRepoService<ActualSetMetric>(new EntityDBContext(_optionsBuilder.Options));

            var existingActExercise = (
            await
            (
            from actExercises in _aetable
            where actExercises.TemplateExerciseId == set.TemplateExerciseId && actExercises.ActualWorkoutId == actualWOId
            select new
            {
                Id = actExercises.Id,
            }
            ).ToListAsync()).FirstOrDefault()
            ?? new
            {
                Id = 0
            };

            ActualExercise actExercise = new ActualExercise()
            {
                Id = existingActExercise.Id,
                TemplateExerciseId = set.TemplateExerciseId,
                ActualTemplateExerciseId = set.TemplateExerciseId,
                ActualWorkoutId = actualWOId,
                Createdatetime = DateTime.Now
            };

            if (actExercise.Id == 0)
            {
                await actExRepoServ.InsertAsync(actExercise);
            }
            else
            {
                await actExRepoServ.UpdateAsync(actExercise);
            }

            var existingActSet = (
            await
            (
            from actSet in _astable
            where actSet.ActualExerciseId == actExercise.Id && actSet.ExecutionOrder == set.Order
            select new
            {
                Id = actSet.Id,
            }
            ).ToListAsync()).FirstOrDefault()
            ?? new
            {
                Id = 0
            };

            ActualSet actualSet = new ActualSet()
            {
                Id = existingActSet.Id,
                ActualExerciseId = actExercise.Id,
                TemplateSetId = set.TemplateSetId,
                ExecutionOrder = set.Order,
                Completed = set.Complete,
                Createdatetime = DateTime.Now
            };

            if (actualSet.Id == 0)
            {
                actualSet.Id =  await actSetRepoServ.InsertAsync(actualSet);
            }
            else
            {
                await actSetRepoServ.UpdateAsync(actualSet);
            }

            if (set.LogWorkoutSetMetricVMs != null && set.Complete)
            {

                foreach (var metric in set.LogWorkoutSetMetricVMs)
                {
                    var existingMetric = (await
                    (
                    from actMetric in _amtable
                    where (metric.TemplateSetMetricId == actMetric.TemplateSetMetricId)
                    select new
                    {
                        Id = actMetric.Id
                    }
                    ).ToListAsync()).FirstOrDefault() ?? new
                    {
                        Id = 0
                    };

                    ActualSetMetric metricSave = new ActualSetMetric()
                    {
                        Id = existingMetric.Id,
                        ActualSetId = actualSet.Id,
                        TemplateSetMetricId = metric.TemplateSetMetricId,
                        PlannedTextValue = metric.PlannedTextValue,
                        PlannedNumericValue = metric.PlannedNumericValue ?? 0,
                        ActualTextValue = metric.ActualTextValue,
                        ActualNumericValue = metric.ActualNumericValue ?? 0,
                        Createdatetime = DateTime.Now
                    };

                    if (metricSave.Id == 0)
                    {
                        metricSave.Id = await actSetMetRepoServ.InsertAsync(metricSave);
                    }
                    else
                    {
                        await actSetMetRepoServ.UpdateAsync(metricSave);
                    }
                }
            }
        }
        public async Task CompleteWorkout(int actualWOId)
        {
            _optionsBuilder = Helpers.BuildOptions();
            IGenericRepoService<ActualWorkout> genericRepoService = new GenericRepoService<ActualWorkout>(new EntityDBContext(_optionsBuilder.Options));
            ActualWorkout aw = await genericRepoService.GetByIdAsync(actualWOId);
            aw.WorkOutStatusId = (int)workoutStatus.Ended;
            aw.WorkoutEndDate = DateTime.Now;
            aw.WorkoutEndTime = DateTime.Now.TimeOfDay;
            await genericRepoService.UpdateAsync(aw);
        }
    }
    public enum workoutStatus
    {
        Started = 1,
        Abandoned=2,
        Ended=3
    }

}
