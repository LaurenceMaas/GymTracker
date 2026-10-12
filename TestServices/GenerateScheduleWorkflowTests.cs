using System.ComponentModel;
using System.Reflection;
using GymTrackerBusinessService.Generic;
using GymTrackerBusinessService.Scheduling;
using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TestServices;

public class GenerateScheduleWorkflowTests
{
    [Fact]
    public void SlotViewModel_RepresentsTheEditableSessionFields()
    {
        Assert.Equal(typeof(DayOfWeek), Property(typeof(ScheduleProposalSlotVM), nameof(ScheduleProposalSlotVM.DayOfWeek)).PropertyType);
        Assert.Equal(0, (int)DayOfWeek.Sunday);
        Assert.NotNull(Property(typeof(ScheduleProposalSlotVM), nameof(ScheduleProposalSlotVM.WorkoutPeriod)).GetCustomAttribute<NavigationPropertyAttribute>());
        Assert.NotNull(Property(typeof(ScheduleProposalSlotVM), nameof(ScheduleProposalSlotVM.WorkoutPeriodId)).GetCustomAttribute<IgnoreInGridAttribute>());
        Assert.NotNull(Property(typeof(ScheduleProposalSlotVM), nameof(ScheduleProposalSlotVM.TemplateWorkout)).GetCustomAttribute<NavigationPropertyAttribute>());
        Assert.NotNull(Property(typeof(ScheduleProposalSlotVM), nameof(ScheduleProposalSlotVM.TemplateWorkoutId)).GetCustomAttribute<IgnoreInGridAttribute>());
        Assert.Equal(typeof(int), Property(typeof(ScheduleProposalSlotVM), nameof(ScheduleProposalSlotVM.SortOrder)).PropertyType);
        Assert.Null(typeof(ScheduleProposalSlotVM).GetProperty("UserId"));
        Assert.Null(typeof(ScheduleProposalSlotVM).GetProperty("ScheduleProposalId"));
        Assert.True(Property(typeof(ScheduleProposalVM), nameof(ScheduleProposalVM.Explanation)).GetCustomAttribute<ReadOnlyAttribute>()!.IsReadOnly);
        Assert.True(Property(typeof(ScheduleProposalVM), nameof(ScheduleProposalVM.RecommendedSessionCount)).GetCustomAttribute<ReadOnlyAttribute>()!.IsReadOnly);
        Assert.True(Property(typeof(SchedulePreferenceVM), nameof(SchedulePreferenceVM.WeeklyTraining)).GetCustomAttribute<ReadOnlyAttribute>()!.IsReadOnly);
    }

    [Fact]
    public async Task Load_WithNoDraft_DoesNotGenerate()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Failure = new InvalidOperationException("Load must not generate.");

        GenerateScheduleState state = await harness.Workflow.LoadAsync(harness.UserId);

        Assert.Null(state.Proposal);
        Assert.False(state.HasDraft);
        Assert.Empty(state.Warnings);
        Assert.Empty(harness.Context.TRN_ScheduleProposal);
        Assert.Null(harness.Advisor.LastRequest);
    }

    [Fact]
    public async Task Load_WithExistingDraft_DoesNotGenerateAnother()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("Keep this draft.");
        await harness.Workflow.GenerateAsync(harness.UserId, harness.Preference(), replaceExistingDraft: false);
        harness.Advisor.Failure = new InvalidOperationException("Load must not generate.");

        GenerateScheduleState state = await harness.Workflow.LoadAsync(harness.UserId);

        Assert.NotNull(state.Proposal);
        Assert.Equal("Keep this draft.", state.Proposal.Explanation);
        Assert.Equal(1, harness.Context.TRN_ScheduleProposal.Count());
        Assert.Equal(ProposalStatus.Draft, harness.Context.TRN_ScheduleProposal.Single().Status);
    }

    [Fact]
    public async Task Generate_SavesPreferencesAndCreatesADraft()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("One full-body session.");

        GenerateScheduleCommand result = await harness.Workflow.GenerateAsync(harness.UserId, harness.Preference(), replaceExistingDraft: false);

        Assert.True(result.Succeeded);
        Assert.Equal("Novice", result.State.Preference.FitnessLevel?.Description);
        Assert.Equal("2-3 days, full body.", result.State.Preference.WeeklyTraining);
        Assert.Equal("Strength", result.State.Preference.Goals.Single().TrainingGoal?.Description);
        Assert.NotNull(result.State.Proposal);
        Assert.Equal("One full-body session.", result.State.Proposal.Explanation);
        Assert.Equal(DayOfWeek.Monday, result.State.Proposal.Slots.Single().DayOfWeek);
        Assert.Equal("Morning", result.State.Proposal.Slots.Single().WorkoutPeriod?.Description);
        Assert.Equal("Beginner Full Body", result.State.Proposal.Slots.Single().TemplateWorkout?.Name);
    }

    [Fact]
    public async Task Generate_DoesNotModifyTheLiveSchedule()
    {
        await using Harness harness = await Harness.CreateAsync();
        DOWWorkout existing = harness.ScheduleRow(DayOfWeek.Friday, harness.Catalog.ModerateCardio);
        harness.Context.TRN_DOWWorkout.Add(existing);
        await harness.Context.SaveChangesAsync();
        int scheduleId = existing.Id;
        harness.Advisor.Response = harness.OneSlot("Draft only.");

        await harness.Workflow.GenerateAsync(harness.UserId, harness.Preference(), replaceExistingDraft: false);

        DOWWorkout row = Assert.Single(harness.Context.TRN_DOWWorkout);
        Assert.Equal(scheduleId, row.Id);
        Assert.Equal(DayOfWeek.Friday, row.DayOfWeek);
        Assert.Equal(harness.Catalog.ModerateCardio, row.TemplateWorkoutId);
    }

    [Fact]
    public async Task Generate_WhenADraftExists_RequiresConfirmationBeforeReplacingIt()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("Keep this draft.");
        await harness.Workflow.GenerateAsync(harness.UserId, harness.Preference(), replaceExistingDraft: false);
        harness.Advisor.Response = harness.OneSlot("Replacement.", DayOfWeek.Wednesday);

        GenerateScheduleCommand blocked = await harness.Workflow.GenerateAsync(harness.UserId, harness.Preference(), replaceExistingDraft: false);

        Assert.False(blocked.Succeeded);
        Assert.True(blocked.RequiresConfirmation);
        Assert.Contains(GenerateScheduleWorkflow.ReplaceDraftConfirmation, blocked.Errors);
        Assert.Equal("Keep this draft.", blocked.State.Proposal!.Explanation);
        Assert.Equal(DayOfWeek.Monday, harness.Context.TRN_ScheduleProposalSlot.Single().DayOfWeek);

        GenerateScheduleCommand replaced = await harness.Workflow.GenerateAsync(harness.UserId, harness.Preference(), replaceExistingDraft: true);

        Assert.True(replaced.Succeeded);
        Assert.Equal("Replacement.", replaced.State.Proposal!.Explanation);
        Assert.Equal(DayOfWeek.Wednesday, replaced.State.Proposal.Slots.Single().DayOfWeek);
    }

    [Fact]
    public async Task SlotEdits_RemainInMemoryUntilSaveChanges()
    {
        await using Harness harness = await Harness.CreateAsync();
        ScheduleProposalVM draft = await harness.GenerateDraftAsync();
        ScheduleProposalSlotVM edited = harness.Slot(draft.Slots[0], DayOfWeek.Wednesday, harness.Catalog.Morning, harness.Catalog.BeginnerFullBody, 1);

        GenerateScheduleState beforeSave = await harness.Workflow.LoadAsync(harness.UserId);

        Assert.Equal(DayOfWeek.Monday, beforeSave.Proposal!.Slots.Single().DayOfWeek);

        GenerateScheduleCommand saved = await harness.Workflow.SaveSlotsAsync(harness.UserId, draft.Id, [edited]);

        Assert.True(saved.Succeeded);
        Assert.Equal(DayOfWeek.Wednesday, saved.State.Proposal!.Slots.Single().DayOfWeek);
        Assert.Equal(1, saved.State.Proposal.Slots.Single().SortOrder);
    }

    [Fact]
    public async Task InvalidSlot_IsRejectedAndThePersistedDraftStays()
    {
        await using Harness harness = await Harness.CreateAsync();
        ScheduleProposalVM draft = await harness.GenerateDraftAsync();
        ScheduleProposalSlotVM invalid = harness.Slot(draft.Slots[0], DayOfWeek.Monday, harness.Catalog.Morning, 999, 1);

        GenerateScheduleCommand result = await harness.Workflow.SaveSlotsAsync(harness.UserId, draft.Id, [invalid]);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Contains("template 999", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(harness.Catalog.BeginnerFullBody, result.State.Proposal!.Slots.Single().TemplateWorkoutId);
        Assert.Equal(harness.Catalog.BeginnerFullBody, harness.Context.TRN_ScheduleProposalSlot.Single().TemplateWorkoutId);
    }

    [Fact]
    public async Task Save_AllowsMoreSessionsThanTheRecommendation()
    {
        await using Harness harness = await Harness.CreateAsync();
        ScheduleProposalVM draft = await harness.GenerateDraftAsync();

        GenerateScheduleCommand result = await harness.Workflow.SaveSlotsAsync(harness.UserId, draft.Id,
        [
            harness.Slot(draft.Slots[0], DayOfWeek.Monday, harness.Catalog.Morning, harness.Catalog.BeginnerFullBody, 1),
            harness.Slot(draft.Slots[0], DayOfWeek.Wednesday, harness.Catalog.Morning, harness.Catalog.BeginnerFullBody, 2),
            harness.Slot(draft.Slots[0], DayOfWeek.Friday, harness.Catalog.Evening, harness.Catalog.BeginnerFullBody, 3)
        ]);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.State.Proposal!.RecommendedSessionCount);
        Assert.Equal(3, result.State.Proposal.Slots.Count);
    }

    [Fact]
    public async Task Save_AllowsATemplateOutsideTheGoalPool()
    {
        await using Harness harness = await Harness.CreateAsync();
        ScheduleProposalVM draft = await harness.GenerateDraftAsync();
        Assert.Equal(["Beginner Full Body"], harness.Advisor.LastRequest!.Templates.Select(item => item.Name).ToArray());

        GenerateScheduleCommand result = await harness.Workflow.SaveSlotsAsync(harness.UserId, draft.Id,
        [
            harness.Slot(draft.Slots[0], DayOfWeek.Monday, harness.Catalog.Morning, harness.Catalog.ModerateCardio, 1)
        ]);

        Assert.True(result.Succeeded);
        Assert.Equal(harness.Catalog.ModerateCardio, result.State.Proposal!.Slots.Single().TemplateWorkoutId);
        Assert.Equal("Moderate Cardio", result.State.Proposal.Slots.Single().TemplateWorkout?.Name);
    }

    [Fact]
    public async Task Save_AllowsDifferentTemplatesOnTheSameDayAndPeriod()
    {
        await using Harness harness = await Harness.CreateAsync();
        ScheduleProposalVM draft = await harness.GenerateDraftAsync();

        GenerateScheduleCommand result = await harness.Workflow.SaveSlotsAsync(harness.UserId, draft.Id,
        [
            harness.Slot(draft.Slots[0], DayOfWeek.Monday, harness.Catalog.Morning, harness.Catalog.BeginnerFullBody, 1),
            harness.Slot(draft.Slots[0], DayOfWeek.Monday, harness.Catalog.Morning, harness.Catalog.ModerateCardio, 2)
        ]);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.State.Proposal!.Slots.Count);
        Assert.All(result.State.Proposal.Slots, item =>
        {
            Assert.Equal(DayOfWeek.Monday, item.DayOfWeek);
            Assert.Equal(harness.Catalog.Morning, item.WorkoutPeriodId);
        });
    }

    [Fact]
    public async Task Save_KeepsSundayAsSunday()
    {
        await using Harness harness = await Harness.CreateAsync();
        ScheduleProposalVM draft = await harness.GenerateDraftAsync();
        Assert.Equal(0, (int)DayOfWeek.Sunday);

        GenerateScheduleCommand result = await harness.Workflow.SaveSlotsAsync(harness.UserId, draft.Id,
        [
            harness.Slot(draft.Slots[0], DayOfWeek.Sunday, harness.Catalog.Morning, harness.Catalog.BeginnerFullBody, 1)
        ]);

        Assert.True(result.Succeeded);
        Assert.Equal(DayOfWeek.Sunday, result.State.Proposal!.Slots.Single().DayOfWeek);
        Assert.Equal(0, (int)harness.Context.TRN_ScheduleProposalSlot.Single().DayOfWeek);
    }

    [Fact]
    public async Task Accept_ReplacesTheLiveScheduleAndLeavesHistoryUntouched()
    {
        await using Harness harness = await Harness.CreateAsync();
        ScheduleProposalVM draft = await harness.GenerateDraftAsync();
        harness.Context.TRN_DOWWorkout.Add(harness.ScheduleRow(DayOfWeek.Friday, harness.Catalog.ModerateCardio));
        harness.Context.TRN_ActualWorkout.Add(harness.HistoryRow());
        await harness.Context.SaveChangesAsync();
        int historyId = harness.Context.TRN_ActualWorkout.Single().Id;

        GenerateScheduleCommand result = await harness.Workflow.AcceptAsync(harness.UserId, draft.Id, draft.Slots);

        Assert.True(result.Succeeded);
        Assert.Null(result.State.Proposal);
        DOWWorkout row = Assert.Single(harness.Context.TRN_DOWWorkout);
        Assert.Equal(harness.UserId, row.UserId);
        Assert.Equal(DayOfWeek.Monday, row.DayOfWeek);
        Assert.Equal(harness.Catalog.BeginnerFullBody, row.TemplateWorkoutId);
        ActualWorkout history = Assert.Single(harness.Context.TRN_ActualWorkout);
        Assert.Equal(historyId, history.Id);
        Assert.Equal("Beginner Full Body", history.WorkoutName);
    }

    [Fact]
    public async Task Accept_SavesUnsavedSlotEditsBeforeReplacingTheLiveSchedule()
    {
        await using Harness harness = await Harness.CreateAsync();
        ScheduleProposalVM draft = await harness.GenerateDraftAsync();
        ScheduleProposalSlotVM monday = harness.Slot(draft.Slots[0], DayOfWeek.Monday, harness.Catalog.Morning, harness.Catalog.BeginnerFullBody, 1);
        ScheduleProposalSlotVM wednesday = harness.Slot(draft.Slots[0], DayOfWeek.Wednesday, harness.Catalog.Morning, harness.Catalog.BeginnerFullBody, 2);
        GenerateScheduleCommand stored = await harness.Workflow.SaveSlotsAsync(harness.UserId, draft.Id, [monday, wednesday]);
        Assert.True(stored.Succeeded);
        harness.Context.TRN_ActualWorkout.Add(harness.HistoryRow());
        await harness.Context.SaveChangesAsync();

        GenerateScheduleCommand result = await harness.Workflow.AcceptAsync(harness.UserId, draft.Id, [monday]);

        Assert.True(result.Succeeded);
        DOWWorkout row = Assert.Single(harness.Context.TRN_DOWWorkout);
        Assert.Equal(DayOfWeek.Monday, row.DayOfWeek);
        Assert.Equal(harness.Catalog.BeginnerFullBody, row.TemplateWorkoutId);
        Assert.DoesNotContain(harness.Context.TRN_DOWWorkout, item => item.DayOfWeek == DayOfWeek.Wednesday);
        Assert.Equal("Beginner Full Body", Assert.Single(harness.Context.TRN_ActualWorkout).WorkoutName);
    }

    [Fact]
    public async Task Abandon_LeavesTheLiveScheduleAndHistoryUntouched()
    {
        await using Harness harness = await Harness.CreateAsync();
        await harness.GenerateDraftAsync();
        harness.Context.TRN_DOWWorkout.Add(harness.ScheduleRow(DayOfWeek.Friday, harness.Catalog.ModerateCardio));
        harness.Context.TRN_ActualWorkout.Add(harness.HistoryRow());
        await harness.Context.SaveChangesAsync();

        GenerateScheduleCommand result = await harness.Workflow.AbandonAsync(harness.UserId);

        Assert.True(result.Succeeded);
        Assert.Null(result.State.Proposal);
        Assert.Equal(ProposalStatus.Abandoned, harness.Context.TRN_ScheduleProposal.Single().Status);
        Assert.Equal(DayOfWeek.Friday, Assert.Single(harness.Context.TRN_DOWWorkout).DayOfWeek);
        Assert.Equal("Beginner Full Body", Assert.Single(harness.Context.TRN_ActualWorkout).WorkoutName);
    }

    private static PropertyInfo Property(Type type, string name)
    {
        return type.GetProperty(name)!;
    }

    private sealed class FakeAdvisor : IScheduleAdvisor
    {
        public ScheduleAdvisorResponse? Response { get; set; }
        public Exception? Failure { get; set; }
        public ScheduleAdvisorRequest? LastRequest { get; set; }

        public Task<ScheduleAdvisorResponse> RecommendAsync(ScheduleAdvisorRequest request)
        {
            LastRequest = request;
            if (Failure != null)
            {
                throw Failure;
            }

            return Task.FromResult(Response!);
        }
    }

    private sealed class Catalog
    {
        public int Novice { get; init; }
        public int Strength { get; init; }
        public int Morning { get; init; }
        public int Evening { get; init; }
        public int BeginnerFullBody { get; init; }
        public int ModerateCardio { get; init; }
    }

    private sealed class Harness : IAsyncDisposable
    {
        public required EntityDBContext Context { get; init; }
        public required IGenerateScheduleWorkflow Workflow { get; init; }
        public required FakeAdvisor Advisor { get; init; }
        public required Catalog Catalog { get; init; }
        public Guid UserId { get; } = Guid.NewGuid();

        public static async Task<Harness> CreateAsync()
        {
            var options = new DbContextOptionsBuilder<EntityDBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var context = new EntityDBContext(options);
            Catalog catalog = await SeedAsync(context);
            var advisor = new FakeAdvisor();
            var preferences = new TrainingPreferenceService(
                new GenericRepoService<UserTrainingPreference>(context),
                new GenericRepoService<UserTrainingPreferenceGoal>(context),
                new GenericRepoService<FitnessLevel>(context),
                new GenericRepoService<TrainingGoal>(context));
            var proposals = new ScheduleProposalService(
                preferences,
                new GenericRepoService<ScheduleProposal>(context),
                new GenericRepoService<ScheduleProposalGoal>(context),
                new GenericRepoService<ScheduleProposalSlot>(context),
                new GenericRepoService<FitnessLevel>(context),
                new GenericRepoService<TemplateWorkout>(context),
                new GenericRepoService<TemplateTrainingGoal>(context),
                new GenericRepoService<WorkoutPeriod>(context),
                new GenericRepoService<DOWWorkout>(context),
                advisor,
                new ScheduleProposalValidator());
            var workflow = new GenerateScheduleWorkflow(
                preferences,
                proposals,
                new GenericRepoService<FitnessLevel>(context),
                new GenericRepoService<TrainingGoal>(context),
                new GenericRepoService<WorkoutPeriod>(context),
                new GenericRepoService<TemplateWorkout>(context));
            return new Harness
            {
                Context = context,
                Workflow = workflow,
                Advisor = advisor,
                Catalog = catalog
            };
        }

        public SchedulePreferenceVM Preference()
        {
            return new SchedulePreferenceVM
            {
                FitnessLevelId = Catalog.Novice,
                Goals = [new SchedulePreferenceGoalVM { TrainingGoalId = Catalog.Strength }]
            };
        }

        public async Task<ScheduleProposalVM> GenerateDraftAsync()
        {
            Advisor.Response = OneSlot("One full-body session.");
            GenerateScheduleCommand result = await Workflow.GenerateAsync(UserId, Preference(), replaceExistingDraft: false);
            Assert.True(result.Succeeded);
            Assert.NotNull(result.State.Proposal);
            return result.State.Proposal;
        }

        public ScheduleProposalSlotVM Slot(ScheduleProposalSlotVM template, DayOfWeek day, int periodId, int templateId, int sortOrder)
        {
            return new ScheduleProposalSlotVM
            {
                DayOfWeek = day,
                WorkoutPeriodId = periodId,
                WorkoutPeriod = template.WorkoutPeriod,
                TemplateWorkoutId = templateId,
                TemplateWorkout = template.TemplateWorkout,
                SortOrder = sortOrder
            };
        }

        public ScheduleAdvisorResponse OneSlot(string explanation, DayOfWeek day = DayOfWeek.Monday)
        {
            return new ScheduleAdvisorResponse
            {
                RecommendedSessionCount = 1,
                Explanation = explanation,
                Slots =
                [
                    new ScheduleAdvisorSlot
                    {
                        DayOfWeek = (int)day,
                        WorkoutPeriodId = Catalog.Morning,
                        TemplateWorkoutId = Catalog.BeginnerFullBody,
                        SortOrder = 1
                    }
                ]
            };
        }

        public DOWWorkout ScheduleRow(DayOfWeek day, int templateId)
        {
            return new DOWWorkout
            {
                UserId = UserId,
                DayOfWeek = day,
                WorkoutPeriodId = Catalog.Morning,
                TemplateWorkoutId = templateId,
                SortOrder = 1
            };
        }

        public ActualWorkout HistoryRow()
        {
            return new ActualWorkout
            {
                PerformedByUserId = UserId,
                DayOfWeek = DayOfWeek.Monday,
                WorkoutName = "Beginner Full Body",
                TemplateWorkoutId = Catalog.BeginnerFullBody,
                WorkoutStartDate = new DateTime(2026, 10, 2),
                WorkoutStartTime = new TimeSpan(6, 0, 0),
                WorkOutStatusId = 1
            };
        }

        public ValueTask DisposeAsync()
        {
            return Context.DisposeAsync();
        }

        private static async Task<Catalog> SeedAsync(EntityDBContext context)
        {
            context.LKP_FitnessLevel.Add(new FitnessLevel
            {
                Description = "Novice",
                MinSessionsPerWeek = 2,
                MaxSessionsPerWeek = 3,
                WeeklyTraining = "2-3 days, full body."
            });
            context.LKP_TrainingGoal.AddRange(
                new TrainingGoal { Description = "Fat loss" },
                new TrainingGoal { Description = "Strength" });
            context.LKP_WorkoutPeriod.AddRange(
                new WorkoutPeriod { Description = "Morning", StartTime = new TimeSpan(5, 0, 0), EndTime = new TimeSpan(12, 0, 0) },
                new WorkoutPeriod { Description = "Evening", StartTime = new TimeSpan(18, 1, 0), EndTime = new TimeSpan(23, 59, 0) });
            await context.SaveChangesAsync();
            int novice = context.LKP_FitnessLevel.Single().Id;
            int fatLoss = context.LKP_TrainingGoal.Single(item => item.Description == "Fat loss").Id;
            int strength = context.LKP_TrainingGoal.Single(item => item.Description == "Strength").Id;
            var beginner = new TemplateWorkout { Name = "Beginner Full Body", FitnessLevelId = novice };
            var cardio = new TemplateWorkout { Name = "Moderate Cardio", FitnessLevelId = novice };
            context.LKP_WorkoutTemplate.AddRange(beginner, cardio);
            await context.SaveChangesAsync();
            context.LKP_TemplateTrainingGoal.AddRange(
                new TemplateTrainingGoal { TemplateWorkoutId = beginner.Id, TrainingGoalId = strength },
                new TemplateTrainingGoal { TemplateWorkoutId = cardio.Id, TrainingGoalId = fatLoss });
            await context.SaveChangesAsync();
            return new Catalog
            {
                Novice = novice,
                Strength = strength,
                Morning = context.LKP_WorkoutPeriod.Single(item => item.Description == "Morning").Id,
                Evening = context.LKP_WorkoutPeriod.Single(item => item.Description == "Evening").Id,
                BeginnerFullBody = beginner.Id,
                ModerateCardio = cardio.Id
            };
        }
    }
}
