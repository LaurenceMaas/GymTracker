using GymTrackerBusinessService.Repository;
using GymTrackerDataModel;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TestServices;

public class BeginnerFullBodyWorkoutLoadTests : IDisposable
{
    private readonly EntityDBContext _context;
    private readonly LogWorkOutService _service;

    public BeginnerFullBodyWorkoutLoadTests()
    {
        var options = new DbContextOptionsBuilder<EntityDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new EntityDBContext(options);
        _service = new LogWorkOutService(_context);
    }

    [Fact]
    public async Task LoadWorkout_ReturnsFourExercisesWithFourPlannedSets()
    {
        var workout = await WorkoutLogSeed.SeedBeginnerFullBodyAsync(_context);

        var exercises = await _service.GetExercises(workout.Id, Guid.NewGuid());

        Assert.Equal(4, exercises.Count);
        Assert.Equal(
            new[] { "Barbell Squat", "Barbell Bench Press", "Bent Over Row", "Deadlift" },
            exercises.Select(exercise => exercise.ExerciseName).ToArray());

        foreach (var exercise in exercises)
        {
            var sets = await _service.GetExerciseData(exercise);

            Assert.Equal(new[] { 1, 2, 3, 4 }, sets.Select(set => set.SetNumber).ToArray());
            foreach (var plan in WorkoutLogSeed.Sets)
            {
                var set = sets.Single(item => item.SetNumber == plan.Order);
                Assert.False(set.Complete);
                Assert.Equal(WorkoutLogSeed.PlannedSetMetrics(plan.Reps, plan.BodyWeightPercent), set.PlannedSetMetrics);

                var metrics = set.LogWorkoutSetMetricVMs;
                Assert.NotNull(metrics);
                Assert.Equal(2, metrics.Count);
                Assert.Equal(plan.Reps, Assert.Single(metrics, metric => metric.MetricName == WorkoutLogSeed.RepsName).PlannedNumericValue);
                Assert.Equal(plan.BodyWeightPercent, Assert.Single(metrics, metric => metric.MetricName == WorkoutLogSeed.BodyWeightName).PlannedNumericValue);
            }
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
