using GymTrackerDataModel.Models;
using GymTrackerDbUow.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using GymTrackerDataModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Identity.Client;
using System.Diagnostics;
using GymTrackerBusinessService.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Principal;
using GymTrackerDataModel.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


//I need a service that PREPARES data for the UI
namespace GymTrackerBusinessService.Repository
{
    public interface IWorkoutExerciseSetService
    {
        Task<List<WorkoutGroupVM>> GetHeaderDataAsync(int workOutTemplateId);
        Task<List<WorkoutSetVM>> GetDetailDataAsync(WorkoutGroupVM workoutGroupVM);
        Task<List<WorkoutSetVM>> OnAddNewExerciseDetail(List<WorkoutSetVM> exerciseDetails);
        List<WorkoutGroupVM> OnAddNewExercise(List<WorkoutGroupVM> exercises);
        Task<List<WorkoutGroupVM>> OnUpdateExerciseAsync(WorkoutGroupVM workoutGroupVMmod, List<WorkoutGroupVM> currentWorkoutGroupVMs);
        Task<Dictionary<object, List<WorkoutSetVM>>> OnUpdateDetailEntryAsync(Dictionary<object, List<WorkoutSetVM>> sets, object metricId, int index, WorkoutSetVM workoutSetVM);
        Task SaveWorkoutHeaderData(TemplateWorkout templateWorkout);
        Task<List<WorkoutGroupVM>> SaveWorkoutExerciseData(List<WorkoutGroupVM>? exerciseToSave, int workOutTemplateId);
        Task SaveExerciseSetsData(Dictionary<object, List<WorkoutSetVM>> data);
    }
    public class WorkoutGroupVM 
    {
        [IgnoreInGrid]
        public int TemplateExerciseId { get; set; }
        public int ExerciseNumber { get; set; }
        [DisplayName("Exercise Number")]
        [IgnoreInGrid]
        public int ExerciseId { get; set; }
        [NavigationProperty(typeof(Exercise), TextField = "Name")]
        public Exercise? Exercise { get; set; }
    }

    public class WorkoutSetVM
    {
        [IgnoreInGrid]
        public int TemplateSetId { get; set; }
        public int SetNumber { get; set; }
        [Collection]
        public List<WorkoutSetMetricVM>? WorkoutSetMetricVMs { get; set; }
    }
    public class WorkoutSetMetricVM
    {
        [IgnoreInGrid]
        public int MetricId { get; set; }
        [NavigationProperty(typeof(Metric), TextField = "Name")]
        public Metric? Metric { get; set; }
        [IgnoreInGrid]
        public int TemplateSetId { get; set; }
        [DisplayName("")]
        public decimal? NumericValue { get; set; }
        [DisplayName("")]
        public string? TextValue { get; set; }
    }
    public class WorkoutExerciseSetService : IWorkoutExerciseSetService
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

        public WorkoutExerciseSetService(EntityDBContext context)
        {
            _context = context;
            _wtTable = _context.Set<TemplateWorkout>();
            _tetable = _context.Set<TemplateExercise>();
            _etable = _context.Set<Exercise>();
            _tstable = _context.Set<TemplateSet>();
            _tsmtable = _context.Set<TemplateSetMetric>();
            _mtable= _context.Set<Metric>();
            _ettable = _context.Set<ExerciseType>();
        }
        public async Task<List<WorkoutGroupVM>> GetHeaderDataAsync(int workOutTemplateId)
        {
            int i = 0;
            var workoutExerciseVM = (
            await
            (
            from templateWorkout in _wtTable
            join templateExercise in _tetable on templateWorkout.Id equals templateExercise.WorkoutTemplateId
            join exercise in _etable on templateExercise.ExerciseId equals exercise.Id
            where templateWorkout.Id == workOutTemplateId
            select new
            {
                ExerciseId = exercise.Id,
                TemplateExerciseId = templateExercise.Id,
                ExerciseName = exercise.Name,
            }
            )
            .ToListAsync())
            .Select((x, index) => new
            {
                RowNumber = index + 1,
                x.ExerciseId,
                x.TemplateExerciseId,
                x.ExerciseName
            })
            .ToList();

            List<WorkoutGroupVM> result = workoutExerciseVM
            .GroupBy(x => new
            {
                x.TemplateExerciseId,
                x.ExerciseId,
                x.ExerciseName,
                x.RowNumber
            })
            .Select(g => new WorkoutGroupVM
            {
                ExerciseNumber= g.Key.RowNumber,
                ExerciseId = g.Key.ExerciseId,
                TemplateExerciseId = g.Key.TemplateExerciseId,
                Exercise = new Exercise
                {
                    Id = g.Key.ExerciseId,
                    Name = g.Key.ExerciseName
                }
            }
            ).ToList();

            return result;
        }
        public async Task<List<WorkoutSetVM>> GetDetailDataAsync(WorkoutGroupVM workoutGroupVM)
        {
            var rawData = (await
            (
                from templateSet in _tstable
                join templateSetMetric in _tsmtable  on templateSet.Id equals templateSetMetric.TemplateSetId
                join metric in _mtable on templateSetMetric.MetricId equals metric.Id
                where templateSet.TemplateExerciseId ==  workoutGroupVM.TemplateExerciseId
                select new
                {
                    TemplateSetId = templateSet.Id,
                    MetricId = metric.Id,
                    MetricName = metric.Name,
                    NumericValue = templateSetMetric.NumericValue,
                    TextValue = templateSetMetric.TextValue
                }
            ).ToListAsync());

            var formattedData = rawData
                .GroupBy(x => new
                {
                    x.TemplateSetId,
                })
                .Select(g => new WorkoutSetVM
                {
                    TemplateSetId = g.Key.TemplateSetId,

                    WorkoutSetMetricVMs = g.Select(m => new WorkoutSetMetricVM
                    {
                        MetricId = m.MetricId,
                        Metric = new Metric()
                        {
                            Id = m.MetricId,
                            Name = m.MetricName
                        },
                        NumericValue = m.NumericValue,
                        TextValue = m.TextValue,
                        TemplateSetId = m.TemplateSetId,
                    }).ToList()
                })
                .OrderBy(x => x.SetNumber)
                .ToList();

            for(int i = 0;i< formattedData.Count;i++)
            {
                formattedData.ElementAt(i).SetNumber = (i + 1);
            }

            return formattedData;
        }
        public async Task<List<WorkoutSetVM>> OnAddNewExerciseDetail(List<WorkoutSetVM> exerciseDetails)
        {

            var templateSet = exerciseDetails.Count > 0 ? exerciseDetails.Max(x => x.TemplateSetId) + 1: 0;
            exerciseDetails.Add(new WorkoutSetVM()
            {
                TemplateSetId = templateSet,
                SetNumber = exerciseDetails.Count > 0 ? exerciseDetails.Max(x => x.SetNumber) + 1:1,
                WorkoutSetMetricVMs = new List<WorkoutSetMetricVM>()
                {
                    new WorkoutSetMetricVM() {MetricId =0,TemplateSetId = templateSet,NumericValue = 0,TextValue = " "},
                    new WorkoutSetMetricVM() {MetricId =0,TemplateSetId = templateSet,NumericValue = 0,TextValue = " "}
                }
            });

            return exerciseDetails;
        }
        public List<WorkoutGroupVM>? OnAddNewExercise(List<WorkoutGroupVM> exercises)
        {
            exercises.Add(new WorkoutGroupVM(){ ExerciseNumber  = exercises.Count > 0 ? exercises.Max(x=>x.ExerciseNumber) +1:1});
            return exercises;
        }
        public async Task<List<WorkoutGroupVM>> OnUpdateExerciseAsync(WorkoutGroupVM workoutGroupVMmod, List<WorkoutGroupVM> currentWorkoutGroupVMs)
        {
            return currentWorkoutGroupVMs;
        }
        public async Task SaveWorkoutHeaderData(TemplateWorkout templateWorkout)
        {
            _optionsBuilder = Helpers.BuildOptions();
            IGenericRepoService<TemplateWorkout> genericRepoService = new GenericRepoService<TemplateWorkout>(new EntityDBContext(_optionsBuilder.Options));

            if (templateWorkout.Id ==0)
            {
                await genericRepoService.InsertAsync(templateWorkout);
            }
            else 
            {
                await genericRepoService.UpdateAsync(templateWorkout);
            }
        }
        public async Task<List<WorkoutGroupVM>> SaveWorkoutExerciseData(List<WorkoutGroupVM>? exerciseToSave, int workOutTemplateId)
        {
            IEnumerable<int> OriginalItems;
            IEnumerable<int> IDs;
            var existingIds = await GetHeaderDataAsync(workOutTemplateId);
            OriginalItems = existingIds.Select(x => x.TemplateExerciseId);
            IDs = exerciseToSave.Select(x => x.TemplateExerciseId);
            IEnumerable<int> missingEntries = OriginalItems.Where(x => !IDs.Any(y => y == x));

            _optionsBuilder = Helpers.BuildOptions();
            IGenericRepoService<TemplateExercise> genericRepoService = new GenericRepoService<TemplateExercise>(new EntityDBContext(_optionsBuilder.Options));

            foreach (int item in missingEntries)
            {
                await genericRepoService.DeleteAsync(item);
            }
            foreach (WorkoutGroupVM workoutGroupVM in exerciseToSave)
            {
                if (workoutGroupVM.TemplateExerciseId == 0)
                {
                    TemplateExercise templateExercise = new TemplateExercise()
                    {
                        Id = 0,
                        WorkoutTemplateId = workOutTemplateId,
                        ExerciseId = workoutGroupVM.ExerciseId
                    };
                    workoutGroupVM.TemplateExerciseId = await genericRepoService.InsertAsync(templateExercise);
                }
                else
                {
                    TemplateExercise templateExercise = new TemplateExercise()
                    {
                        Id = workoutGroupVM.TemplateExerciseId,
                        WorkoutTemplateId = workOutTemplateId,
                        ExerciseId = workoutGroupVM.ExerciseId
                    };
                    await genericRepoService.UpdateAsync(templateExercise);
                }
            }

            return exerciseToSave;
        }
        public async Task SaveExerciseSetsData(Dictionary<object, List<WorkoutSetVM>> data)
        {
            IEnumerable<int> OriginalItems;
            IEnumerable<int> IDs;
            List<WorkoutSetVM> workoutSets;
            IEnumerable<int> missingEntries;
            IEnumerable<int> metricIDs;
            TemplateSetMetric? tsmToSave;
            List<TemplateSetMetric>? tsmOrig;
            List<TemplateSetMetric>? tsmUpdate;
            TemplateSet tSet;
            int iter = 0;

            _optionsBuilder = Helpers.BuildOptions();
            IGenericRepoService<TemplateSet> tsRepoService = new GenericRepoService<TemplateSet>(new EntityDBContext(_optionsBuilder.Options));
            IGenericRepoService<TemplateSetMetric> tsmRepoService = new GenericRepoService<TemplateSetMetric>(new EntityDBContext(_optionsBuilder.Options));

            foreach (object key in data.Keys)
            {
                WorkoutGroupVM wg = (WorkoutGroupVM)key;
                workoutSets = data[key];
                IDs = workoutSets.Select(x => x.TemplateSetId);
                OriginalItems =
                (
                from templateSet in _tstable
                where templateSet.TemplateExerciseId == wg.TemplateExerciseId
                select new
                {
                    Id = templateSet.Id,
                }).Select(x => x.Id);

                missingEntries = OriginalItems.Where(x => !IDs.Any(y => y == x));

                foreach (int item in missingEntries)
                {
                    await tsRepoService.DeleteAsync(item);
                }

                foreach(WorkoutSetVM workoutSetVM in workoutSets)
                {
                    tSet = await tsRepoService.GetByIdAsync(workoutSetVM.TemplateSetId);
                    if (tSet == null)
                    {
                        tSet = new TemplateSet();
                    }
                    if (tSet.TemplateExerciseId == wg.TemplateExerciseId)
                    {
                        tSet.TemplateExerciseId = wg.TemplateExerciseId;
                        tSet.Order = workoutSetVM.SetNumber;
                        await tsRepoService.UpdateAsync(tSet);
                    }
                    else
                    {
                        tSet = new TemplateSet()
                        {
                            Id = 0,
                            TemplateExerciseId = wg.TemplateExerciseId,
                            Order = workoutSetVM.SetNumber
                        };
                        await tsRepoService.InsertAsync(tSet);
                    }

                    tsmOrig = (
                    from templateSetMetric in _tsmtable
                    where templateSetMetric.TemplateSetId == workoutSetVM.TemplateSetId
                    select new TemplateSetMetric
                    {

                        Id = templateSetMetric.Id,
                        TemplateSetId = templateSetMetric.TemplateSetId,

                        MetricId = templateSetMetric.MetricId,
                        NumericValue = templateSetMetric.NumericValue,
                        TextValue = templateSetMetric.TextValue

                    }).ToList();

                    iter = 0;
                    foreach (WorkoutSetMetricVM wsm in workoutSetVM.WorkoutSetMetricVMs)
                    {
                        if (wsm.MetricId > 0)
                        {
                            tsmUpdate = tsmOrig.FindAll(x => (x.TemplateSetId == tSet.Id));
                            //only allowing 2 metrics
                            if (tsmUpdate.Count >= 2)
                            {
                                tsmToSave = tsmOrig.FirstOrDefault(x => (x.TemplateSetId == tSet.Id) && x.MetricId == wsm.MetricId) ?? tsmUpdate.ElementAt(iter);
                            }
                            else
                            {
                                tsmToSave = new TemplateSetMetric();
                            }

                            tsmToSave.NumericValue = wsm.NumericValue;
                            tsmToSave.TextValue = wsm.TextValue;
                            tsmToSave.TemplateSetId = tSet.Id;//workoutSetVM.TemplateSetId;
                            tsmToSave.MetricId = wsm.MetricId;
                            if (tsmToSave.Id != 0)
                            {
                                await tsmRepoService.UpdateAsync(tsmToSave);
                            }
                            else
                            {
                                await tsmRepoService.InsertAsync(tsmToSave);
                            }
                        }
                        iter++;
                    }

                }
            }
        }
        public async Task<Dictionary<object, List<WorkoutSetVM>>> OnUpdateDetailEntryAsync(Dictionary<object, List<WorkoutSetVM>> sets,object metricId,int index, WorkoutSetVM workoutSetVM)
        {
            List<WorkoutSetVM> value;
            WorkoutSetVM set;
            WorkoutSetMetricVM met = new WorkoutSetMetricVM();
            foreach (object key in sets.Keys)
            {
               value = sets[key];
               set = value.FirstOrDefault(x => x.SetNumber == workoutSetVM.SetNumber && x.TemplateSetId == workoutSetVM.TemplateSetId);
                
                if (set != null)
                {
                    if (set.WorkoutSetMetricVMs.Count >= index)
                    {
                        met = set == null ? new WorkoutSetMetricVM() : set.WorkoutSetMetricVMs.ElementAt(index);
                    }
                    else
                    {
                        met = new WorkoutSetMetricVM();
                    }

                    var metric = await (from Metric in _mtable
                                        where Metric.Id == (int)metricId
                                        select new
                                        {
                                            Id = Metric.Id,
                                            ValueType = Metric.ValueType
                                        }
                    ).ToListAsync();

                    met.Metric = new Metric();
                    met.TemplateSetId = workoutSetVM.TemplateSetId;
                    met.MetricId = (int)metricId;
                    var metricToChange = metric.GroupBy(x => new
                    {
                        x.Id,
                        x.ValueType
                    }).Select(g => new Metric
                    {
                        Id = g.Key.Id,
                        ValueType = g.Key.ValueType
                    }).ToList();
                    if (metricToChange.Count == 0)
                    {
                        met.TextValue = null;
                        met.NumericValue = null;
                    }
                    else
                    {
                        switch (metricToChange.FirstOrDefault().ValueType)
                        {
                            case MetricValueType.Numeric:
                                met.TextValue = null;
                                met.NumericValue = 0;
                                break;

                            case MetricValueType.Text:
                                met.TextValue = " ";
                                met.NumericValue = null;
                                break;
                        }
                    }
                }

            }

            return sets;
        }

    }
}
