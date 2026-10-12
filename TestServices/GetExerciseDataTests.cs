using GymTrackerBusinessService.Repository;
using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TestServices;

public class GetExerciseDataTests : IDisposable
{
    private readonly EntityDBContext _context;
    private readonly LogWorkOutService _service;

    public GetExerciseDataTests()
    {
        var options = new DbContextOptionsBuilder<EntityDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new EntityDBContext(options);
        _service = new LogWorkOutService(_context);
    }

    [Fact]
    public async Task UnloggedSquat_ReturnsFourSetsWithPlannedMetrics()
    {
        var squat = await WorkoutLogSeed.SeedSquatAsync(_context);

        var result = await _service.GetExerciseData(Exercise(squat.Id));

        Assert.Equal(new[] { 1, 2, 3, 4 }, result.Select(set => set.SetNumber).ToArray());
        foreach (var plan in WorkoutLogSeed.Sets)
        {
            AssertPlannedSet(result.Single(set => set.SetNumber == plan.Order), plan.Reps, plan.BodyWeightPercent);
        }
    }

    [Fact]
    public async Task PartiallyLoggedSquat_CompletesOnlyTheFirstSet()
    {
        var squat = await WorkoutLogSeed.SeedSquatAsync(_context);
        var actualExercise = await WorkoutLogSeed.LogFirstSetAsync(_context, squat, actualReps: 10m, actualBodyWeightPercent: 45m);

        var result = await _service.GetExerciseData(Exercise(squat.Id, actualExercise.Id));

        var set1 = result.Single(set => set.SetNumber == 1);
        Assert.True(set1.Complete);
        Assert.Equal(WorkoutLogSeed.PlannedSetMetrics(12, 40m), set1.PlannedSetMetrics);

        var reps = Metric(set1, WorkoutLogSeed.RepsName);
        var bodyWeight = Metric(set1, WorkoutLogSeed.BodyWeightName);
        Assert.Equal(12m, reps.PlannedNumericValue);
        Assert.Equal(10m, reps.ActualNumericValue);
        Assert.Equal(WorkoutLogSeed.RepsLabel(12), reps.ActualLabel);
        Assert.Equal(40m, bodyWeight.PlannedNumericValue);
        Assert.Equal(45m, bodyWeight.ActualNumericValue);
        Assert.Equal(WorkoutLogSeed.BodyWeightLabel(40m), bodyWeight.ActualLabel);

        foreach (var plan in WorkoutLogSeed.Sets.Where(set => set.Order > 1))
        {
            AssertPlannedSet(result.Single(set => set.SetNumber == plan.Order), plan.Reps, plan.BodyWeightPercent);
        }
    }

    [Fact]
    public async Task EachSetShowsOnlyItsOwnMetrics()
    {
        var squat = await WorkoutLogSeed.SeedSquatAsync(_context);

        var result = await _service.GetExerciseData(Exercise(squat.Id));

        Assert.Equal(new[] { 1, 2, 3, 4 }, result.Select(set => set.SetNumber).ToArray());

        var set1 = result.Single(set => set.SetNumber == 1);
        var set2 = result.Single(set => set.SetNumber == 2);
        AssertExactMetrics(set1, reps: 12, bodyWeightPercent: 40m);
        AssertExactMetrics(set2, reps: 10, bodyWeightPercent: 50m);

        Assert.DoesNotContain(WorkoutLogSeed.PlannedSetMetrics(10, 50m), set1.PlannedSetMetrics);
        Assert.DoesNotContain("10 reps Reps", set1.PlannedSetMetrics);
        Assert.DoesNotContain("50 % Percentage Body Weight", set1.PlannedSetMetrics);
        Assert.DoesNotContain(WorkoutLogSeed.PlannedSetMetrics(12, 40m), set2.PlannedSetMetrics);
        Assert.DoesNotContain("12 reps Reps", set2.PlannedSetMetrics);
        Assert.DoesNotContain("40 % Percentage Body Weight", set2.PlannedSetMetrics);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private static LogWorkoutLogExerciseVM Exercise(int templateExerciseId, int actualExerciseId = 0)
    {
        return new LogWorkoutLogExerciseVM
        {
            TemplateExerciseId = templateExerciseId,
            ActualExerciseId = actualExerciseId
        };
    }

    private static void AssertPlannedSet(LogWorkoutSetVM set, int reps, decimal bodyWeightPercent)
    {
        Assert.False(set.Complete);
        Assert.Equal(WorkoutLogSeed.PlannedSetMetrics(reps, bodyWeightPercent), set.PlannedSetMetrics);
        AssertExactMetrics(set, reps, bodyWeightPercent);

        var repsMetric = Metric(set, WorkoutLogSeed.RepsName);
        var bodyWeight = Metric(set, WorkoutLogSeed.BodyWeightName);
        Assert.Equal(reps, repsMetric.ActualNumericValue);
        Assert.Equal(bodyWeightPercent, bodyWeight.ActualNumericValue);
    }

    private static void AssertExactMetrics(LogWorkoutSetVM set, int reps, decimal bodyWeightPercent)
    {
        var metrics = set.LogWorkoutSetMetricVMs;
        Assert.NotNull(metrics);
        Assert.Equal(2, metrics.Count);

        var repsMetric = Metric(set, WorkoutLogSeed.RepsName);
        var bodyWeight = Metric(set, WorkoutLogSeed.BodyWeightName);
        Assert.Equal(reps, repsMetric.PlannedNumericValue);
        Assert.Equal("reps", repsMetric.Unit);
        Assert.Equal(WorkoutLogSeed.RepsLabel(reps), repsMetric.ActualLabel);
        Assert.Equal(bodyWeightPercent, bodyWeight.PlannedNumericValue);
        Assert.Equal("%", bodyWeight.Unit);
        Assert.Equal(WorkoutLogSeed.BodyWeightLabel(bodyWeightPercent), bodyWeight.ActualLabel);
    }

    private static LogWorkoutSetMetricVM Metric(LogWorkoutSetVM set, string name)
    {
        return Assert.Single(set.LogWorkoutSetMetricVMs!, metric => metric.MetricName == name);
    }
}
