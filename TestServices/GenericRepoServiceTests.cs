using GymTrackerBusinessService.Generic;
using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TestServices;

public class GenericRepoServiceTests
{
    [Fact]
    public async Task Query_FiltersAndIncludesNestedPaths()
    {
        await using EntityDBContext context = CreateContext();
        Catalog catalog = await SeedAsync(context);
        var proposals = new GenericRepoService<ScheduleProposal>(context);
        proposals.StageAdd(Proposal(catalog, "Visible"));
        proposals.StageAdd(Proposal(catalog, "Hidden"));
        await proposals.SaveAsync();

        IReadOnlyList<ScheduleProposal> found = await proposals.QueryAsync(
            item => item.Explanation == "Visible",
            trackChanges: false,
            "FitnessLevel",
            "Goals.TrainingGoal",
            "Slots.WorkoutPeriod",
            "Slots.TemplateWorkout.FitnessLevel");

        ScheduleProposal proposal = Assert.Single(found);
        Assert.Equal("Novice", proposal.FitnessLevel.Description);
        Assert.Equal("Strength", Assert.Single(proposal.Goals).TrainingGoal.Description);
        ScheduleProposalSlot slot = Assert.Single(proposal.Slots);
        Assert.Equal("Morning", slot.WorkoutPeriod.Description);
        Assert.Equal("Beginner Full Body", slot.TemplateWorkout.Name);
        Assert.Equal("Novice", slot.TemplateWorkout.FitnessLevel.Description);
    }

    [Fact]
    public async Task Query_TrackedResultCanBeSaved_UntrackedResultCannot()
    {
        await using EntityDBContext context = CreateContext();
        Catalog catalog = await SeedAsync(context);
        var proposals = new GenericRepoService<ScheduleProposal>(context);
        proposals.StageAdd(Proposal(catalog, "Original"));
        await proposals.SaveAsync();

        ScheduleProposal untracked = Assert.Single(await proposals.QueryAsync(item => item.Explanation == "Original", false));
        untracked.Explanation = "Changed while detached";
        await proposals.SaveAsync();
        Assert.Equal("Original", Assert.Single(await proposals.QueryAsync(item => item.UserId == untracked.UserId, false)).Explanation);

        ScheduleProposal tracked = Assert.Single(await proposals.QueryAsync(item => item.Explanation == "Original", true));
        tracked.Explanation = "Changed while tracked";
        await proposals.SaveAsync();
        Assert.Equal("Changed while tracked", Assert.Single(await proposals.QueryAsync(item => item.Id == tracked.Id, false)).Explanation);
    }

    [Fact]
    public async Task StageAdd_DoesNotSaveUntilSaveAsync_AndAcceptsACompositeKeyChild()
    {
        await using EntityDBContext context = CreateContext();
        Catalog catalog = await SeedAsync(context);
        var proposals = new GenericRepoService<ScheduleProposal>(context);
        var goals = new GenericRepoService<ScheduleProposalGoal>(context);
        proposals.StageAdd(Proposal(catalog, "Staged"));

        Assert.Empty(await proposals.QueryAsync(item => true, false));

        await goals.SaveAsync();
        ScheduleProposal stored = Assert.Single(await proposals.QueryAsync(item => item.Explanation == "Staged", false, "Goals"));
        Assert.Equal(catalog.GoalId, Assert.Single(stored.Goals).TrainingGoalId);
    }

    [Fact]
    public async Task StageRemove_DoesNotDeleteUntilSaveAsync()
    {
        await using EntityDBContext context = CreateContext();
        Catalog catalog = await SeedAsync(context);
        var proposals = new GenericRepoService<ScheduleProposal>(context);
        proposals.StageAdd(Proposal(catalog, "Remove me"));
        await proposals.SaveAsync();
        ScheduleProposal tracked = Assert.Single(await proposals.QueryAsync(item => item.Explanation == "Remove me", true));

        proposals.StageRemove(tracked);
        Assert.Single(await proposals.QueryAsync(item => item.Id == tracked.Id, false));

        await proposals.SaveAsync();
        Assert.Empty(await proposals.QueryAsync(item => item.Id == tracked.Id, false));
    }

    [Fact]
    public async Task TransactionMethods_AreSafeWhenTheProviderHasNoTransaction()
    {
        await using EntityDBContext context = CreateContext();
        var levels = new GenericRepoService<FitnessLevel>(context);
        var proposals = new GenericRepoService<ScheduleProposal>(context);

        await levels.BeginTransactionAsync();
        levels.StageAdd(new FitnessLevel { Description = "Novice", MinSessionsPerWeek = 2, MaxSessionsPerWeek = 3 });
        await proposals.SaveAsync();
        await proposals.CommitTransactionAsync();
        await levels.RollbackTransactionAsync();

        Assert.Single(await levels.QueryAsync(item => item.Description == "Novice", false));
    }

    private static ScheduleProposal Proposal(Catalog catalog, string explanation)
    {
        return new ScheduleProposal
        {
            UserId = Guid.NewGuid(),
            Status = ProposalStatus.Draft,
            FitnessLevelId = catalog.LevelId,
            RecommendedSessionCount = 1,
            Explanation = explanation,
            CreatedAt = new DateTime(2026, 10, 5),
            Goals = [new ScheduleProposalGoal { TrainingGoalId = catalog.GoalId }],
            Slots =
            [
                new ScheduleProposalSlot
                {
                    DayOfWeek = DayOfWeek.Monday,
                    WorkoutPeriodId = catalog.PeriodId,
                    TemplateWorkoutId = catalog.TemplateId,
                    SortOrder = 1
                }
            ]
        };
    }

    private static async Task<Catalog> SeedAsync(EntityDBContext context)
    {
        var level = new FitnessLevel { Description = "Novice", MinSessionsPerWeek = 2, MaxSessionsPerWeek = 3 };
        var goal = new TrainingGoal { Description = "Strength" };
        var period = new WorkoutPeriod { Description = "Morning", StartTime = new TimeSpan(5, 0, 0), EndTime = new TimeSpan(12, 0, 0) };
        context.AddRange(level, goal, period);
        await context.SaveChangesAsync();
        var template = new TemplateWorkout { Name = "Beginner Full Body", FitnessLevelId = level.Id };
        context.Add(template);
        await context.SaveChangesAsync();
        return new Catalog(level.Id, goal.Id, period.Id, template.Id);
    }

    private sealed record Catalog(int LevelId, int GoalId, int PeriodId, int TemplateId);

    private static EntityDBContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EntityDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new EntityDBContext(options);
    }
}
