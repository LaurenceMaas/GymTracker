using GymTrackerBusinessService.Repository;
using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TestServices;

public class DevelopmentWorkoutFlowTests
{
    [Fact]
    public async Task StartedWorkout_LoadsPlans_SavesActuals_AndHistoryKeepsSeparateRows()
    {
        var connection = "Data Source=localhost;User ID=sa_test;Password=Password@1;Initial Catalog=GymTrackerDb;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<EntityDBContext>()
            .UseSqlServer(connection)
            .Options;

        await using var context = new EntityDBContext(options);
        var user = Guid.NewGuid();
        var now = DateTime.Now;
        var period = await context.LKP_WorkoutPeriod
            .FirstOrDefaultAsync(item => now.TimeOfDay >= item.StartTime && now.TimeOfDay < item.EndTime);
        Assert.NotNull(period);

        var template = await context.LKP_WorkoutTemplate.SingleAsync(item => item.Name == "Beginner Full Body");
        context.TRN_DOWWorkout.Add(new DOWWorkout
        {
            UserId = user,
            DayOfWeek = now.DayOfWeek,
            WorkoutPeriodId = period.Id,
            TemplateWorkoutId = template.Id,
            SortOrder = 1
        });
        await context.SaveChangesAsync();
        var scheduleId = context.TRN_DOWWorkout.Single(item => item.UserId == user).Id;

        try
        {
            var service = new LogWorkOutService(context);
            var workoutId = await service.SaveNewActualWorkOut(user, scheduleId, now, "flow notes");
            var exercises = await service.GetExercises(template.Id, user);
            Assert.NotEmpty(exercises);

            var sets = await service.GetExerciseData(exercises[0]);
            Assert.NotEmpty(sets);
            Assert.False(string.IsNullOrWhiteSpace(sets[0].PlannedSetMetrics));
            Assert.False(sets[0].Complete);

            var completedSet = sets[0];
            completedSet.Complete = true;
            foreach (var metric in completedSet.LogWorkoutSetMetricVMs!)
            {
                if (metric.PlannedNumericValue != null)
                {
                    metric.ActualNumericValue = metric.PlannedNumericValue + 1;
                }
            }

            await service.CompleteSet(completedSet, workoutId);
            var reloadedExercises = await service.GetExercises(template.Id, user);
            var reloaded = await service.GetExerciseData(reloadedExercises[0]);
            Assert.True(reloadedExercises[0].ActualExerciseId > 0);
            Assert.True(reloaded[0].Complete);
            Assert.Equal(
                completedSet.LogWorkoutSetMetricVMs[0].ActualNumericValue,
                reloaded[0].LogWorkoutSetMetricVMs![0].ActualNumericValue);

            await service.CompleteWorkout(workoutId);

            context.TRN_ActualWorkout.Add(new ActualWorkout
            {
                PerformedByUserId = user,
                DayOfWeek = now.DayOfWeek,
                WorkoutName = "Beginner Full Body",
                WorkoutStartDate = now.Date.AddDays(-1),
                WorkoutStartTime = now.TimeOfDay,
                Notes = "abandoned",
                WorkOutStatusId = 2
            });
            await context.SaveChangesAsync();

            var history = await new WorkoutHistory(context).LoadWorkoutHistoryPerUser(user.ToString());
            Assert.Equal(2, history.Count);
            Assert.True(history[0].WorkoutStrtDtTime >= history[1].WorkoutStrtDtTime);
            Assert.Equal("Beginner Full Body", history[0].WorkoutName);
            Assert.Equal(now.DayOfWeek, history[0].DayOfWeek);
            Assert.Equal("flow notes", history[0].Notes);
            Assert.Equal("Ended", history[0].Status);
            Assert.NotNull(history[0].WorkoutEndDtTime);
            Assert.Null(history.Single(item => item.Notes == "abandoned").WorkoutEndDtTime);
        }
        finally
        {
            await context.TRN_ActualWorkout.Where(item => item.PerformedByUserId == user).ExecuteDeleteAsync();
            await context.TRN_DOWWorkout.Where(item => item.UserId == user).ExecuteDeleteAsync();
        }
    }
}
