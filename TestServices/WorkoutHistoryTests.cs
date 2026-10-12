using GymTrackerBusinessService.Repository;
using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TestServices;

public class WorkoutHistoryTests : IDisposable
{
    private readonly EntityDBContext _context;
    private readonly WorkoutHistory _history;

    public WorkoutHistoryTests()
    {
        var options = new DbContextOptionsBuilder<EntityDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new EntityDBContext(options);
        _history = new WorkoutHistory(_context);
    }

    [Fact]
    public async Task History_ReturnsOneRowPerWorkout_NewestFirst_AndAllowsMissingEnd()
    {
        var user = Guid.NewGuid();
        _context.LKP_WorkOutStatus.AddRange(
            new WorkOutStatus { Id = 1, Description = "Started" },
            new WorkOutStatus { Id = 2, Description = "Abandoned" },
            new WorkOutStatus { Id = 3, Description = "Ended" });

        var olderStart = new DateTime(2026, 9, 29, 8, 0, 0);
        var newerStart = new DateTime(2026, 9, 30, 9, 0, 0);
        _context.TRN_ActualWorkout.AddRange(
            new ActualWorkout
            {
                PerformedByUserId = user,
                DayOfWeek = DayOfWeek.Tuesday,
                WorkoutName = "Beginner Full Body",
                WorkoutStartDate = olderStart.Date,
                WorkoutStartTime = olderStart.TimeOfDay,
                WorkoutEndDate = olderStart.Date,
                WorkoutEndTime = olderStart.TimeOfDay.Add(TimeSpan.FromHours(1)),
                Notes = "older",
                WorkOutStatusId = 3
            },
            new ActualWorkout
            {
                PerformedByUserId = user,
                DayOfWeek = DayOfWeek.Tuesday,
                WorkoutName = "Beginner Full Body",
                WorkoutStartDate = newerStart.Date,
                WorkoutStartTime = newerStart.TimeOfDay,
                Notes = "abandoned",
                WorkOutStatusId = 2
            });
        await _context.SaveChangesAsync();

        var history = await _history.LoadWorkoutHistoryPerUser(user.ToString());

        Assert.Equal(2, history.Count);
        Assert.Equal(newerStart, history[0].WorkoutStrtDtTime);
        Assert.Null(history[0].WorkoutEndDtTime);
        Assert.Equal("Beginner Full Body", history[0].WorkoutName);
        Assert.Equal(DayOfWeek.Tuesday, history[0].DayOfWeek);
        Assert.Equal("Abandoned", history[0].Status);
        Assert.Equal("abandoned", history[0].Notes);
        Assert.Equal(olderStart, history[1].WorkoutStrtDtTime);
        Assert.Equal(olderStart.AddHours(1), history[1].WorkoutEndDtTime);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
