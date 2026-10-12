using GymTrackerBusinessService.Generic;
using GymTrackerBusinessService.Scheduling;
using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace TestServices;

public class TrainingPreferenceServiceTests
{
    [Fact]
    public async Task UserWithNoPreference_ReturnsNull()
    {
        await using EntityDBContext context = await SeededContextAsync();
        var service = CreateService(context);

        TrainingPreferenceDto? preference = await service.GetAsync(Guid.NewGuid());

        Assert.Null(preference);
        Assert.Empty(context.USR_TrainingPreference);
    }

    [Fact]
    public async Task ExistingPreference_ReturnsTheStoredLevelAndGoals()
    {
        await using EntityDBContext context = await SeededContextAsync();
        var service = CreateService(context);
        Guid userId = Guid.NewGuid();
        await service.SaveAsync(userId, LevelId(context, "Intermediate"), [GoalId(context, "Strength"), GoalId(context, "Size")]);

        TrainingPreferenceDto? preference = await service.GetAsync(userId);

        Assert.NotNull(preference);
        Assert.Equal(userId, preference.UserId);
        Assert.Equal("Intermediate", preference.FitnessLevel);
        Assert.Equal(3, preference.MinSessionsPerWeek);
        Assert.Equal(4, preference.MaxSessionsPerWeek);
        Assert.Equal(["Strength", "Size"], preference.Goals.Select(item => item.Description).ToArray());
    }

    [Fact]
    public async Task NewPreference_IsCreated()
    {
        await using EntityDBContext context = await SeededContextAsync();
        var service = CreateService(context);
        Guid userId = Guid.NewGuid();

        SaveTrainingPreferenceResult result = await service.SaveAsync(userId, LevelId(context, "Novice"), [GoalId(context, "Fat loss")]);

        Assert.True(result.Succeeded);
        Assert.Equal(userId, Assert.Single(context.USR_TrainingPreference).UserId);
        Assert.Equal("Novice", result.Preference!.FitnessLevel);
    }

    [Fact]
    public async Task MultipleGoals_AreStored()
    {
        await using EntityDBContext context = await SeededContextAsync();
        var service = CreateService(context);
        Guid userId = Guid.NewGuid();

        SaveTrainingPreferenceResult result = await service.SaveAsync(
            userId,
            LevelId(context, "Intermediate"),
            [GoalId(context, "Fat loss"), GoalId(context, "Strength"), GoalId(context, "Size")]);

        Assert.True(result.Succeeded);
        Assert.Equal(["Fat loss", "Strength", "Size"], result.Preference!.Goals.Select(item => item.Description).ToArray());
        Assert.Equal(3, context.USR_TrainingPreferenceGoal.Count());
    }

    [Fact]
    public async Task ExistingFitnessLevel_CanBeChanged()
    {
        await using EntityDBContext context = await SeededContextAsync();
        var service = CreateService(context);
        Guid userId = Guid.NewGuid();
        int strength = GoalId(context, "Strength");
        await service.SaveAsync(userId, LevelId(context, "Intermediate"), [strength]);

        SaveTrainingPreferenceResult result = await service.SaveAsync(userId, LevelId(context, "Advanced"), [strength]);

        Assert.True(result.Succeeded);
        Assert.Equal("Advanced", result.Preference!.FitnessLevel);
        Assert.Equal(["Strength"], result.Preference.Goals.Select(item => item.Description).ToArray());
        Assert.Equal(LevelId(context, "Advanced"), Assert.Single(context.USR_TrainingPreference).FitnessLevelId);
    }

    [Fact]
    public async Task ExistingGoals_AreReplaced()
    {
        await using EntityDBContext context = await SeededContextAsync();
        var service = CreateService(context);
        Guid userId = Guid.NewGuid();
        await service.SaveAsync(userId, LevelId(context, "Intermediate"), [GoalId(context, "Strength"), GoalId(context, "Size")]);

        SaveTrainingPreferenceResult result = await service.SaveAsync(
            userId,
            LevelId(context, "Intermediate"),
            [GoalId(context, "Fat loss"), GoalId(context, "Strength")]);

        Assert.True(result.Succeeded);
        Assert.Equal(["Fat loss", "Strength"], result.Preference!.Goals.Select(item => item.Description).ToArray());
        Assert.Equal(2, context.USR_TrainingPreferenceGoal.Count());
    }

    [Fact]
    public async Task RemovingAGoal_LeavesOnlyTheSavedGoal()
    {
        await using EntityDBContext context = await SeededContextAsync();
        var service = CreateService(context);
        Guid userId = Guid.NewGuid();
        await service.SaveAsync(userId, LevelId(context, "Intermediate"), [GoalId(context, "Strength"), GoalId(context, "Size")]);

        SaveTrainingPreferenceResult result = await service.SaveAsync(userId, LevelId(context, "Intermediate"), [GoalId(context, "Fat loss")]);

        Assert.True(result.Succeeded);
        Assert.Equal(["Fat loss"], result.Preference!.Goals.Select(item => item.Description).ToArray());
        TrainingPreferenceDto? stored = await service.GetAsync(userId);
        Assert.Equal(["Fat loss"], stored!.Goals.Select(item => item.Description).ToArray());
    }

    [Fact]
    public async Task UnknownFitnessLevel_IsRejected()
    {
        await using EntityDBContext context = await SeededContextAsync();
        var service = CreateService(context);

        SaveTrainingPreferenceResult result = await service.SaveAsync(Guid.NewGuid(), 99, [GoalId(context, "Strength")]);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Contains("Fitness level 99", StringComparison.Ordinal));
        Assert.Empty(context.USR_TrainingPreference);
    }

    [Fact]
    public async Task UnknownGoal_IsRejected()
    {
        await using EntityDBContext context = await SeededContextAsync();
        var service = CreateService(context);

        SaveTrainingPreferenceResult result = await service.SaveAsync(Guid.NewGuid(), LevelId(context, "Novice"), [99]);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Contains("Training goal 99", StringComparison.Ordinal));
        Assert.Empty(context.USR_TrainingPreference);
    }

    [Fact]
    public async Task EmptyGoalSelection_IsRejected()
    {
        await using EntityDBContext context = await SeededContextAsync();
        var service = CreateService(context);

        SaveTrainingPreferenceResult result = await service.SaveAsync(Guid.NewGuid(), LevelId(context, "Novice"), []);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Contains("at least one", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(context.USR_TrainingPreference);
    }

    [Fact]
    public async Task FailedValidation_LeavesTheExistingPreferenceUnchanged()
    {
        await using EntityDBContext context = await SeededContextAsync();
        var service = CreateService(context);
        Guid userId = Guid.NewGuid();
        await service.SaveAsync(userId, LevelId(context, "Intermediate"), [GoalId(context, "Strength"), GoalId(context, "Size")]);

        SaveTrainingPreferenceResult result = await service.SaveAsync(userId, 99, [GoalId(context, "Fat loss")]);

        Assert.False(result.Succeeded);
        TrainingPreferenceDto? stored = await service.GetAsync(userId);
        Assert.Equal("Intermediate", stored!.FitnessLevel);
        Assert.Equal(["Strength", "Size"], stored.Goals.Select(item => item.Description).ToArray());
    }

    [Fact]
    public async Task FailedSave_LeavesTheExistingPreferenceUnchanged()
    {
        string databaseName = Guid.NewGuid().ToString();
        Guid userId = Guid.NewGuid();
        await using (EntityDBContext setup = await SeededContextAsync(databaseName))
        {
            var service = CreateService(setup);
            await service.SaveAsync(userId, LevelId(setup, "Intermediate"), [GoalId(setup, "Strength"), GoalId(setup, "Size")]);
        }

        await using (EntityDBContext failing = await SeededContextAsync(databaseName, new FailSaveInterceptor()))
        {
            var service = CreateService(failing);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.SaveAsync(userId, LevelId(failing, "Novice"), [GoalId(failing, "Fat loss")]));
        }

        await using EntityDBContext check = CreateContext(databaseName);
        var stored = await CreateService(check).GetAsync(userId);
        Assert.Equal("Intermediate", stored!.FitnessLevel);
        Assert.Equal(["Strength", "Size"], stored.Goals.Select(item => item.Description).ToArray());
    }

    [Fact]
    public async Task SavingAPreference_DoesNotChangeScheduleHistoryOrTemplates()
    {
        await using EntityDBContext context = await SeededContextAsync();
        var template = new TemplateWorkout { Name = "Beginner Full Body", FitnessLevelId = LevelId(context, "Novice") };
        context.LKP_WorkoutTemplate.Add(template);
        await context.SaveChangesAsync();
        Guid userId = Guid.NewGuid();
        context.TRN_DOWWorkout.Add(new DOWWorkout
        {
            UserId = userId,
            DayOfWeek = DayOfWeek.Monday,
            WorkoutPeriodId = 1,
            TemplateWorkoutId = template.Id,
            SortOrder = 1
        });
        context.TRN_ActualWorkout.Add(new ActualWorkout
        {
            PerformedByUserId = userId,
            DayOfWeek = DayOfWeek.Monday,
            WorkoutName = "Beginner Full Body",
            TemplateWorkoutId = template.Id,
            WorkoutStartDate = new DateTime(2026, 10, 2),
            WorkoutStartTime = new TimeSpan(6, 0, 0),
            WorkOutStatusId = 1
        });
        await context.SaveChangesAsync();

        await CreateService(context).SaveAsync(userId, LevelId(context, "Novice"), [GoalId(context, "Strength")]);

        DOWWorkout schedule = Assert.Single(context.TRN_DOWWorkout);
        Assert.Equal(DayOfWeek.Monday, schedule.DayOfWeek);
        Assert.Equal(template.Id, schedule.TemplateWorkoutId);
        ActualWorkout history = Assert.Single(context.TRN_ActualWorkout);
        Assert.Equal("Beginner Full Body", history.WorkoutName);
        Assert.Equal("Beginner Full Body", Assert.Single(context.LKP_WorkoutTemplate).Name);
    }

    [Fact]
    public async Task AddingAGoal_KeepsTheGoalsAlreadySelected()
    {
        await using EntityDBContext context = await SeededContextAsync();
        var service = CreateService(context);
        Guid userId = Guid.NewGuid();
        await service.SaveAsync(userId, LevelId(context, "Intermediate"), [GoalId(context, "Strength")]);

        SaveTrainingPreferenceResult result = await service.SaveAsync(
            userId,
            LevelId(context, "Intermediate"),
            [GoalId(context, "Strength"), GoalId(context, "Size")]);

        Assert.True(result.Succeeded);
        Assert.Equal(["Strength", "Size"], result.Preference!.Goals.Select(item => item.Description).ToArray());
        Assert.Equal(2, context.USR_TrainingPreferenceGoal.Count());
    }

    [Fact]
    public async Task SavingOneUser_DoesNotChangeAnotherUsersPreference()
    {
        await using EntityDBContext context = await SeededContextAsync();
        var service = CreateService(context);
        Guid firstUser = Guid.NewGuid();
        Guid secondUser = Guid.NewGuid();
        await service.SaveAsync(firstUser, LevelId(context, "Novice"), [GoalId(context, "Fat loss")]);
        await service.SaveAsync(secondUser, LevelId(context, "Advanced"), [GoalId(context, "Strength")]);

        TrainingPreferenceDto? first = await service.GetAsync(firstUser);
        TrainingPreferenceDto? second = await service.GetAsync(secondUser);

        Assert.Equal("Novice", first!.FitnessLevel);
        Assert.Equal(["Fat loss"], first.Goals.Select(item => item.Description).ToArray());
        Assert.Equal("Advanced", second!.FitnessLevel);
        Assert.Equal(["Strength"], second.Goals.Select(item => item.Description).ToArray());
        Assert.Equal(2, context.USR_TrainingPreference.Count());
    }

    private static TrainingPreferenceService CreateService(EntityDBContext context)
    {
        return new TrainingPreferenceService(
            new GenericRepoService<UserTrainingPreference>(context),
            new GenericRepoService<UserTrainingPreferenceGoal>(context),
            new GenericRepoService<FitnessLevel>(context),
            new GenericRepoService<TrainingGoal>(context));
    }

    private static async Task<EntityDBContext> SeededContextAsync(string? databaseName = null, SaveChangesInterceptor? interceptor = null)
    {
        EntityDBContext context = CreateContext(databaseName, interceptor);
        if (await context.LKP_FitnessLevel.AnyAsync())
        {
            return context;
        }

        context.LKP_FitnessLevel.AddRange(
            new FitnessLevel { Description = "Novice", MinSessionsPerWeek = 2, MaxSessionsPerWeek = 3 },
            new FitnessLevel { Description = "Intermediate", MinSessionsPerWeek = 3, MaxSessionsPerWeek = 4 },
            new FitnessLevel { Description = "Advanced", MinSessionsPerWeek = 4, MaxSessionsPerWeek = 6 });
        context.LKP_TrainingGoal.AddRange(
            new TrainingGoal { Description = "Fat loss" },
            new TrainingGoal { Description = "Strength" },
            new TrainingGoal { Description = "Size" });
        await context.SaveChangesAsync();
        return context;
    }

    private static EntityDBContext CreateContext(string? databaseName, SaveChangesInterceptor? interceptor = null)
    {
        var builder = new DbContextOptionsBuilder<EntityDBContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString());
        if (interceptor != null)
        {
            builder.AddInterceptors(interceptor);
        }

        return new EntityDBContext(builder.Options);
    }

    private static int LevelId(EntityDBContext context, string description)
    {
        return context.LKP_FitnessLevel.Single(item => item.Description == description).Id;
    }

    private static int GoalId(EntityDBContext context, string description)
    {
        return context.LKP_TrainingGoal.Single(item => item.Description == description).Id;
    }

    private sealed class FailSaveInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            throw new InvalidOperationException("Simulated preference save failure.");
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Simulated preference save failure.");
        }
    }
}
