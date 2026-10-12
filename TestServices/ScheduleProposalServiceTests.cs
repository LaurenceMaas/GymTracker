using GymTrackerBusinessService.Generic;
using GymTrackerBusinessService.Scheduling;
using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace TestServices;

public class ScheduleProposalServiceTests
{
    [Fact]
    public async Task ValidPreferences_GenerateADraft()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("One full-body session.");

        ProposalCommandResult result = await harness.Service.GenerateAsync(harness.UserId);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Proposal);
        ScheduleProposal draft = result.Proposal;
        Assert.Equal(ProposalStatus.Draft, draft.Status);
        Assert.Equal(harness.Catalog.Novice, draft.FitnessLevelId);
        Assert.Equal("Strength", Assert.Single(draft.Goals).TrainingGoal.Description);
        ScheduleProposalSlot slot = Assert.Single(draft.Slots);
        Assert.Equal(harness.Catalog.BeginnerFullBody, slot.TemplateWorkoutId);
        Assert.Equal(harness.Catalog.Morning, slot.WorkoutPeriodId);
        Assert.Equal("Beginner Full Body", slot.TemplateWorkout.Name);
        Assert.Equal("Novice", draft.FitnessLevel.Description);
        Assert.Equal("Morning", slot.WorkoutPeriod.Description);
        Assert.Equal("Novice", slot.TemplateWorkout.FitnessLevel.Description);
        Assert.Equal("One full-body session.", draft.Explanation);
        Assert.Equal("No resistance-training experience, or none for several years.", harness.Advisor.LastRequest!.Qualification);
        Assert.Equal("8-12 reps at about 60-70% of 1RM. Start with 1-3 sets.", harness.Advisor.LastRequest.StrengthLoading);
        Assert.Equal(["Beginner Full Body"], harness.Advisor.LastRequest.Templates.Select(item => item.Name).ToArray());
        Assert.Equal(["Morning", "Evening"], harness.Advisor.LastRequest.Periods.Select(item => item.Name).ToArray());
    }

    [Fact]
    public async Task GeneratingAgain_ReplacesOnlyTheDraft()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Context.TRN_ScheduleProposal.Add(Proposal(harness.UserId, ProposalStatus.Accepted, "Accepted plan", harness.Catalog));
        harness.Context.TRN_ScheduleProposal.Add(Proposal(harness.UserId, ProposalStatus.Abandoned, "Abandoned plan", harness.Catalog));
        harness.Context.TRN_DOWWorkout.Add(ScheduleRow(harness));
        await harness.Context.SaveChangesAsync();
        harness.Advisor.Response = harness.OneSlot("First draft.");
        await harness.Service.GenerateAsync(harness.UserId);
        harness.Advisor.Response = harness.OneSlot("Second draft.", DayOfWeek.Wednesday);

        ProposalCommandResult result = await harness.Service.GenerateAsync(harness.UserId);

        Assert.True(result.Succeeded);
        Assert.Equal(DayOfWeek.Wednesday, Assert.Single(result.Proposal!.Slots).DayOfWeek);
        Assert.Equal("Second draft.", result.Proposal.Explanation);
        ScheduleProposal[] stored = harness.Context.TRN_ScheduleProposal.OrderBy(item => item.Id).ToArray();
        Assert.Equal(3, stored.Length);
        Assert.Contains(stored, item => item.Status == ProposalStatus.Accepted && item.Explanation == "Accepted plan");
        Assert.Contains(stored, item => item.Status == ProposalStatus.Abandoned && item.Explanation == "Abandoned plan");
        Assert.Equal(1, stored.Count(item => item.Status == ProposalStatus.Draft));
        Assert.Equal(DayOfWeek.Monday, Assert.Single(harness.Context.TRN_DOWWorkout).DayOfWeek);
    }

    [Fact]
    public async Task AdvisorFailure_LeavesTheExistingDraft()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("Keep this draft.");
        await harness.Service.GenerateAsync(harness.UserId);
        harness.Advisor.Failure = new InvalidOperationException("Advisor unavailable.");

        await Assert.ThrowsAsync<InvalidOperationException>(() => harness.Service.GenerateAsync(harness.UserId));

        ScheduleProposal draft = Assert.Single(harness.Context.TRN_ScheduleProposal);
        Assert.Equal(ProposalStatus.Draft, draft.Status);
        Assert.Equal("Keep this draft.", draft.Explanation);
    }

    [Fact]
    public async Task AdvisorConfigurationFailure_DoesNotReplaceTheDraft()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("Keep this draft.");
        await harness.Service.GenerateAsync(harness.UserId);
        harness.Advisor.Failure = new ScheduleAdvisorException("The schedule advisor is not configured.");

        ProposalCommandResult result = await harness.Service.GenerateAsync(harness.UserId);

        Assert.False(result.Succeeded);
        Assert.Equal("The schedule advisor is not configured.", Assert.Single(result.Errors));
        ScheduleProposal draft = Assert.Single(harness.Context.TRN_ScheduleProposal);
        Assert.Equal(ProposalStatus.Draft, draft.Status);
        Assert.Equal("Keep this draft.", draft.Explanation);
        Assert.Empty(harness.Context.TRN_DOWWorkout);
    }

    [Fact]
    public async Task InvalidAdvisorResponse_DoesNotCreateOrReplaceADraft()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = new ScheduleAdvisorResponse
        {
            RecommendedSessionCount = 1,
            Explanation = "Bad template.",
            Slots = [new ScheduleAdvisorSlot { DayOfWeek = (int)DayOfWeek.Monday, WorkoutPeriodId = harness.Catalog.Morning, TemplateWorkoutId = 999, SortOrder = 1 }]
        };

        ProposalCommandResult first = await harness.Service.GenerateAsync(harness.UserId);

        Assert.False(first.Succeeded);
        Assert.Contains(first.Errors, error => error.Contains("template 999", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(harness.Context.TRN_ScheduleProposal);

        harness.Advisor.Response = harness.OneSlot("Keep this draft.");
        await harness.Service.GenerateAsync(harness.UserId);
        harness.Advisor.Response = new ScheduleAdvisorResponse
        {
            RecommendedSessionCount = 1,
            Explanation = "Bad period.",
            Slots = [new ScheduleAdvisorSlot { DayOfWeek = (int)DayOfWeek.Monday, WorkoutPeriodId = 50, TemplateWorkoutId = harness.Catalog.BeginnerFullBody, SortOrder = 1 }]
        };

        ProposalCommandResult second = await harness.Service.GenerateAsync(harness.UserId);

        Assert.False(second.Succeeded);
        Assert.Contains(second.Errors, error => error.Contains("period 50", StringComparison.OrdinalIgnoreCase));
        ScheduleProposal draft = Assert.Single(harness.Context.TRN_ScheduleProposal);
        Assert.Equal("Keep this draft.", draft.Explanation);
        Assert.Equal(harness.Catalog.BeginnerFullBody, Assert.Single(harness.Context.TRN_ScheduleProposalSlot).TemplateWorkoutId);
    }

    [Fact]
    public async Task FailedSave_LeavesTheExistingDraftAndNoPartialRows()
    {
        string databaseName = Guid.NewGuid().ToString();
        var gate = new SaveGate();
        await using Harness harness = await Harness.CreateAsync(databaseName, gate);
        harness.Advisor.Response = harness.OneSlot("Keep this draft.");
        await harness.Service.GenerateAsync(harness.UserId);
        gate.Throw = true;
        harness.Advisor.Response = harness.OneSlot("Replacement.", DayOfWeek.Friday);

        await Assert.ThrowsAsync<InvalidOperationException>(() => harness.Service.GenerateAsync(harness.UserId));

        await using EntityDBContext check = Harness.CreateContext(databaseName);
        ScheduleProposal draft = Assert.Single(check.TRN_ScheduleProposal.Include(item => item.Slots));
        Assert.Equal("Keep this draft.", draft.Explanation);
        Assert.Equal(DayOfWeek.Monday, Assert.Single(draft.Slots).DayOfWeek);
    }

    [Fact]
    public async Task UserCanChangeSessionsAndChooseAnyExistingTemplate()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("One session.");
        await harness.Service.GenerateAsync(harness.UserId);

        ProposalCommandResult result = await harness.Service.UpdateSlotsAsync(harness.UserId,
        [
            Slot(DayOfWeek.Monday, harness.Catalog.Morning, harness.Catalog.StrengthSquat, 1),
            Slot(DayOfWeek.Monday, harness.Catalog.Morning, harness.Catalog.ModerateCardio, 2),
            Slot(DayOfWeek.Tuesday, harness.Catalog.Evening, harness.Catalog.BeginnerFullBody, 3)
        ]);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Proposal!.Slots.Count);
        Assert.Equal(1, result.Proposal.RecommendedSessionCount);
        Assert.Contains(result.Proposal.Slots, item => item.TemplateWorkoutId == harness.Catalog.StrengthSquat);
        Assert.Contains(result.Proposal.Slots, item => item.TemplateWorkoutId == harness.Catalog.ModerateCardio);
        Assert.Equal(2, result.Proposal.Slots.Count(item => item.DayOfWeek == DayOfWeek.Monday && item.WorkoutPeriodId == harness.Catalog.Morning));
    }

    [Fact]
    public async Task InvalidSlot_IsRejectedAndTheDraftStays()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("One session.");
        await harness.Service.GenerateAsync(harness.UserId);

        ProposalCommandResult result = await harness.Service.UpdateSlotsAsync(harness.UserId,
        [
            Slot(day: 7, harness.Catalog.Morning, harness.Catalog.BeginnerFullBody, 1)
        ]);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Contains("day 7", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(DayOfWeek.Monday, Assert.Single(harness.Context.TRN_ScheduleProposalSlot).DayOfWeek);
    }

    [Fact]
    public async Task EditedDraft_ProducesRecommendationWarnings()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("One session.");
        await harness.Service.GenerateAsync(harness.UserId);
        await harness.Service.UpdateSlotsAsync(harness.UserId,
        [
            Slot(DayOfWeek.Monday, harness.Catalog.Morning, harness.Catalog.StrengthSquat, 1),
            Slot(DayOfWeek.Monday, harness.Catalog.Morning, harness.Catalog.ModerateCardio, 2),
            Slot(DayOfWeek.Tuesday, harness.Catalog.Morning, harness.Catalog.BeginnerFullBody, 3),
            Slot(DayOfWeek.Wednesday, harness.Catalog.Morning, harness.Catalog.BeginnerFullBody, 4)
        ]);

        IReadOnlyList<string> warnings = await harness.Service.GetWarningsAsync(harness.UserId);

        Assert.Contains(warnings, item => item.Contains("outside the Novice range", StringComparison.Ordinal));
        Assert.Contains(warnings, item => item.Contains("Strength Squat", StringComparison.Ordinal) && item.Contains("Advanced", StringComparison.Ordinal));
        Assert.Contains(warnings, item => item.Contains("Moderate Cardio", StringComparison.Ordinal) && item.Contains("does not share a goal", StringComparison.Ordinal));
        Assert.Contains(warnings, item => item.Contains("More than one workout", StringComparison.Ordinal));
    }

    [Fact]
    public async Task EveningSlot_ProducesTheKnownLimitationWarning()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("One session.");
        await harness.Service.GenerateAsync(harness.UserId);
        await harness.Service.UpdateSlotsAsync(harness.UserId,
        [
            Slot(DayOfWeek.Monday, harness.Catalog.Evening, harness.Catalog.BeginnerFullBody, 1),
            Slot(DayOfWeek.Wednesday, harness.Catalog.Morning, harness.Catalog.BeginnerFullBody, 2)
        ]);

        IReadOnlyList<string> warnings = await harness.Service.GetWarningsAsync(harness.UserId);

        Assert.Contains(warnings, item => item.Contains("Evening", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Abandon_MarksTheDraftAndLeavesScheduleAndHistory()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("One session.");
        await harness.Service.GenerateAsync(harness.UserId);
        harness.Context.TRN_DOWWorkout.Add(ScheduleRow(harness));
        harness.Context.TRN_ActualWorkout.Add(new ActualWorkout
        {
            PerformedByUserId = harness.UserId,
            DayOfWeek = DayOfWeek.Monday,
            WorkoutName = "Beginner Full Body",
            TemplateWorkoutId = harness.Catalog.BeginnerFullBody,
            WorkoutStartDate = new DateTime(2026, 10, 2),
            WorkoutStartTime = new TimeSpan(6, 0, 0),
            WorkOutStatusId = 1
        });
        await harness.Context.SaveChangesAsync();

        ProposalCommandResult result = await harness.Service.AbandonAsync(harness.UserId);

        Assert.True(result.Succeeded);
        Assert.Null(await harness.Service.GetDraftAsync(harness.UserId));
        Assert.Equal(ProposalStatus.Abandoned, Assert.Single(harness.Context.TRN_ScheduleProposal).Status);
        Assert.Equal(DayOfWeek.Monday, Assert.Single(harness.Context.TRN_DOWWorkout).DayOfWeek);
        Assert.Equal("Beginner Full Body", Assert.Single(harness.Context.TRN_ActualWorkout).WorkoutName);
    }

    [Fact]
    public async Task AnotherUser_CannotReadOrReplaceTheDraft()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("One session.");
        await harness.Service.GenerateAsync(harness.UserId);
        Guid otherUser = Guid.NewGuid();

        Assert.Null(await harness.Service.GetDraftAsync(otherUser));
        ProposalCommandResult update = await harness.Service.UpdateSlotsAsync(otherUser,
        [
            Slot(DayOfWeek.Friday, harness.Catalog.Evening, harness.Catalog.StrengthSquat, 1)
        ]);

        Assert.False(update.Succeeded);
        ScheduleProposal? draft = await harness.Service.GetDraftAsync(harness.UserId);
        Assert.NotNull(draft);
        Assert.Equal(DayOfWeek.Monday, Assert.Single(draft.Slots).DayOfWeek);
        Assert.Equal(harness.Catalog.BeginnerFullBody, draft.Slots.Single().TemplateWorkoutId);
    }

    [Fact]
    public async Task Abandon_DoesNotAffectAnotherUserOrAClosedProposal()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("One session.");
        await harness.Service.GenerateAsync(harness.UserId);
        Guid otherUser = Guid.NewGuid();
        harness.Context.TRN_ScheduleProposal.Add(Proposal(otherUser, ProposalStatus.Accepted, "Accepted plan", harness.Catalog));
        harness.Context.TRN_ScheduleProposal.Add(Proposal(harness.UserId, ProposalStatus.Abandoned, "Old abandoned plan", harness.Catalog));
        await harness.Context.SaveChangesAsync();

        ProposalCommandResult other = await harness.Service.AbandonAsync(otherUser);

        Assert.False(other.Succeeded);
        Assert.Equal(ProposalStatus.Draft, (await harness.Service.GetDraftAsync(harness.UserId))!.Status);
        Assert.Equal(ProposalStatus.Accepted, harness.Context.TRN_ScheduleProposal.Single(item => item.UserId == otherUser).Status);

        ProposalCommandResult abandoned = await harness.Service.AbandonAsync(harness.UserId);
        ProposalCommandResult again = await harness.Service.AbandonAsync(harness.UserId);

        Assert.True(abandoned.Succeeded);
        Assert.False(again.Succeeded);
        Assert.Null(await harness.Service.GetDraftAsync(harness.UserId));
        Assert.Equal(ProposalStatus.Accepted, harness.Context.TRN_ScheduleProposal.Single(item => item.Explanation == "Accepted plan").Status);
        Assert.Equal(ProposalStatus.Abandoned, harness.Context.TRN_ScheduleProposal.Single(item => item.Explanation == "Old abandoned plan").Status);
    }

    [Fact]
    public async Task Accept_ReplacesTheLiveScheduleAndMarksTheProposalAccepted()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Context.TRN_DOWWorkout.Add(new DOWWorkout
        {
            UserId = harness.UserId,
            DayOfWeek = DayOfWeek.Friday,
            WorkoutPeriodId = harness.Catalog.Evening,
            TemplateWorkoutId = harness.Catalog.ModerateCardio,
            SortOrder = 9
        });
        await harness.Context.SaveChangesAsync();
        harness.Advisor.Response = harness.OneSlot("Accept this.");
        ProposalCommandResult generated = await harness.Service.GenerateAsync(harness.UserId);

        ProposalCommandResult result = await harness.Service.AcceptAsync(harness.UserId, generated.Proposal!.Id);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Proposal);
        Assert.Equal(ProposalStatus.Accepted, result.Proposal.Status);
        Assert.NotNull(result.Proposal.AcceptedAt);
        Assert.InRange(result.Proposal.AcceptedAt.Value, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        Assert.Equal("Novice", result.Proposal.FitnessLevel.Description);
        Assert.Equal("Strength", Assert.Single(result.Proposal.Goals).TrainingGoal.Description);
        ScheduleProposalSlot slot = Assert.Single(result.Proposal.Slots);
        Assert.Equal("Morning", slot.WorkoutPeriod.Description);
        Assert.Equal("Novice", slot.TemplateWorkout.FitnessLevel.Description);
        DOWWorkout row = Assert.Single(harness.Context.TRN_DOWWorkout);
        Assert.Equal(harness.UserId, row.UserId);
        Assert.Equal(DayOfWeek.Monday, row.DayOfWeek);
        Assert.Equal(harness.Catalog.Morning, row.WorkoutPeriodId);
        Assert.Equal(harness.Catalog.BeginnerFullBody, row.TemplateWorkoutId);
        Assert.Equal(1, row.SortOrder);
        Assert.Null(await harness.Service.GetDraftAsync(harness.UserId));
    }

    [Fact]
    public async Task Accept_ReplacesEveryExistingScheduleRow()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Context.TRN_DOWWorkout.AddRange(
            ScheduleRow(harness),
            new DOWWorkout
            {
                UserId = harness.UserId,
                DayOfWeek = DayOfWeek.Thursday,
                WorkoutPeriodId = harness.Catalog.Evening,
                TemplateWorkoutId = harness.Catalog.ModerateCardio,
                SortOrder = 2
            });
        await harness.Context.SaveChangesAsync();
        harness.Advisor.Response = harness.OneSlot("Wednesday only.", DayOfWeek.Wednesday);
        ProposalCommandResult generated = await harness.Service.GenerateAsync(harness.UserId);

        ProposalCommandResult result = await harness.Service.AcceptAsync(harness.UserId, generated.Proposal!.Id);

        Assert.True(result.Succeeded);
        DOWWorkout row = Assert.Single(harness.Context.TRN_DOWWorkout);
        Assert.Equal(DayOfWeek.Wednesday, row.DayOfWeek);
        Assert.Equal(harness.Catalog.BeginnerFullBody, row.TemplateWorkoutId);
        Assert.Equal(1, row.SortOrder);
    }

    [Fact]
    public async Task Accept_LeavesAnotherUsersScheduleUntouched()
    {
        await using Harness harness = await Harness.CreateAsync();
        Guid otherUser = Guid.NewGuid();
        harness.Context.TRN_DOWWorkout.Add(ScheduleRow(harness));
        harness.Context.TRN_DOWWorkout.Add(new DOWWorkout
        {
            UserId = otherUser,
            DayOfWeek = DayOfWeek.Tuesday,
            WorkoutPeriodId = harness.Catalog.Evening,
            TemplateWorkoutId = harness.Catalog.StrengthSquat,
            SortOrder = 4
        });
        await harness.Context.SaveChangesAsync();
        harness.Advisor.Response = harness.OneSlot("Replace mine.", DayOfWeek.Wednesday);
        ProposalCommandResult generated = await harness.Service.GenerateAsync(harness.UserId);

        ProposalCommandResult result = await harness.Service.AcceptAsync(harness.UserId, generated.Proposal!.Id);

        Assert.True(result.Succeeded);
        DOWWorkout other = Assert.Single(harness.Context.TRN_DOWWorkout, item => item.UserId == otherUser);
        Assert.Equal(DayOfWeek.Tuesday, other.DayOfWeek);
        Assert.Equal(harness.Catalog.StrengthSquat, other.TemplateWorkoutId);
        Assert.Equal(4, other.SortOrder);
        DOWWorkout mine = Assert.Single(harness.Context.TRN_DOWWorkout, item => item.UserId == harness.UserId);
        Assert.Equal(DayOfWeek.Wednesday, mine.DayOfWeek);
    }

    [Fact]
    public async Task Accept_RejectsAnotherUsersProposal()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("Mine.");
        ProposalCommandResult generated = await harness.Service.GenerateAsync(harness.UserId);
        Guid otherUser = Guid.NewGuid();
        harness.Context.TRN_DOWWorkout.Add(new DOWWorkout
        {
            UserId = otherUser,
            DayOfWeek = DayOfWeek.Tuesday,
            WorkoutPeriodId = harness.Catalog.Morning,
            TemplateWorkoutId = harness.Catalog.ModerateCardio,
            SortOrder = 1
        });
        await harness.Context.SaveChangesAsync();

        ProposalCommandResult result = await harness.Service.AcceptAsync(otherUser, generated.Proposal!.Id);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Contains("no draft proposal", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(ProposalStatus.Draft, (await harness.Service.GetDraftAsync(harness.UserId))!.Status);
        Assert.Null(harness.Context.TRN_ScheduleProposal.Single().AcceptedAt);
        Assert.Empty(harness.Context.TRN_DOWWorkout.Where(item => item.UserId == harness.UserId));
        Assert.Equal(DayOfWeek.Tuesday, Assert.Single(harness.Context.TRN_DOWWorkout).DayOfWeek);
    }

    [Fact]
    public async Task Accept_RejectsAnAlreadyAcceptedProposal()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("Once.");
        ProposalCommandResult generated = await harness.Service.GenerateAsync(harness.UserId);
        ProposalCommandResult first = await harness.Service.AcceptAsync(harness.UserId, generated.Proposal!.Id);
        DateTime acceptedAt = first.Proposal!.AcceptedAt!.Value;
        int scheduleId = Assert.Single(harness.Context.TRN_DOWWorkout).Id;

        ProposalCommandResult second = await harness.Service.AcceptAsync(harness.UserId, generated.Proposal.Id);

        Assert.False(second.Succeeded);
        Assert.Contains(second.Errors, error => error.Contains("Only a draft", StringComparison.Ordinal));
        Assert.Equal(ProposalStatus.Accepted, harness.Context.TRN_ScheduleProposal.Single().Status);
        Assert.Equal(acceptedAt, harness.Context.TRN_ScheduleProposal.Single().AcceptedAt);
        Assert.Equal(scheduleId, Assert.Single(harness.Context.TRN_DOWWorkout).Id);
    }

    [Fact]
    public async Task Accept_RejectsAnAbandonedProposal()
    {
        await using Harness harness = await Harness.CreateAsync();
        ScheduleProposal abandoned = Proposal(harness.UserId, ProposalStatus.Abandoned, "Closed.", harness.Catalog);
        harness.Context.TRN_ScheduleProposal.Add(abandoned);
        harness.Context.TRN_DOWWorkout.Add(ScheduleRow(harness));
        await harness.Context.SaveChangesAsync();

        ProposalCommandResult result = await harness.Service.AcceptAsync(harness.UserId, abandoned.Id);

        Assert.False(result.Succeeded);
        Assert.Equal(ProposalStatus.Abandoned, harness.Context.TRN_ScheduleProposal.Single().Status);
        Assert.Null(harness.Context.TRN_ScheduleProposal.Single().AcceptedAt);
        Assert.Equal(DayOfWeek.Monday, Assert.Single(harness.Context.TRN_DOWWorkout).DayOfWeek);
    }

    [Fact]
    public async Task Accept_RejectsInvalidSlotsAndLeavesTheSchedule()
    {
        await using Harness harness = await Harness.CreateAsync();
        ScheduleProposal draft = Proposal(harness.UserId, ProposalStatus.Draft, "Bad day.", harness.Catalog);
        draft.Slots = [new ScheduleProposalSlot { DayOfWeek = (DayOfWeek)7, WorkoutPeriodId = harness.Catalog.Morning, TemplateWorkoutId = harness.Catalog.BeginnerFullBody, SortOrder = 1 }];
        harness.Context.TRN_ScheduleProposal.Add(draft);
        harness.Context.TRN_DOWWorkout.Add(ScheduleRow(harness));
        await harness.Context.SaveChangesAsync();

        ProposalCommandResult result = await harness.Service.AcceptAsync(harness.UserId, draft.Id);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Contains("day 7", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(ProposalStatus.Draft, harness.Context.TRN_ScheduleProposal.Single().Status);
        Assert.Equal(DayOfWeek.Monday, Assert.Single(harness.Context.TRN_DOWWorkout).DayOfWeek);
    }

    [Fact]
    public async Task Accept_RollsBackWhenTheSaveFails()
    {
        string databaseName = Guid.NewGuid().ToString();
        var gate = new SaveGate();
        await using Harness harness = await Harness.CreateAsync(databaseName, gate);
        harness.Advisor.Response = harness.OneSlot("Keep this draft.");
        ProposalCommandResult generated = await harness.Service.GenerateAsync(harness.UserId);
        DOWWorkout original = ScheduleRow(harness);
        harness.Context.TRN_DOWWorkout.Add(original);
        await harness.Context.SaveChangesAsync();
        int originalId = original.Id;
        gate.Throw = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => harness.Service.AcceptAsync(harness.UserId, generated.Proposal!.Id));

        await using EntityDBContext check = Harness.CreateContext(databaseName);
        ScheduleProposal draft = Assert.Single(check.TRN_ScheduleProposal);
        Assert.Equal(ProposalStatus.Draft, draft.Status);
        Assert.Null(draft.AcceptedAt);
        Assert.Equal("Keep this draft.", draft.Explanation);
        Assert.Equal(originalId, Assert.Single(check.TRN_DOWWorkout).Id);
    }

    [Fact]
    public async Task Accept_LeavesActualWorkoutUntouched()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("Accept this.");
        ProposalCommandResult generated = await harness.Service.GenerateAsync(harness.UserId);
        harness.Context.TRN_ActualWorkout.Add(new ActualWorkout
        {
            PerformedByUserId = harness.UserId,
            DayOfWeek = DayOfWeek.Monday,
            WorkoutName = "Beginner Full Body",
            TemplateWorkoutId = harness.Catalog.BeginnerFullBody,
            WorkoutStartDate = new DateTime(2026, 10, 2),
            WorkoutStartTime = new TimeSpan(6, 0, 0),
            WorkOutStatusId = 1
        });
        await harness.Context.SaveChangesAsync();
        int historyId = Assert.Single(harness.Context.TRN_ActualWorkout).Id;

        ProposalCommandResult result = await harness.Service.AcceptAsync(harness.UserId, generated.Proposal!.Id);

        Assert.True(result.Succeeded);
        ActualWorkout history = Assert.Single(harness.Context.TRN_ActualWorkout);
        Assert.Equal(historyId, history.Id);
        Assert.Equal("Beginner Full Body", history.WorkoutName);
        Assert.Equal(harness.UserId, history.PerformedByUserId);
    }

    [Fact]
    public async Task Accept_WritesTheAuthenticatedUserIdOntoTheSchedule()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("Accept this.");
        ProposalCommandResult generated = await harness.Service.GenerateAsync(harness.UserId);

        ProposalCommandResult result = await harness.Service.AcceptAsync(harness.UserId, generated.Proposal!.Id);

        Assert.True(result.Succeeded);
        Assert.Equal(harness.UserId, Assert.Single(harness.Context.TRN_DOWWorkout).UserId);
        Assert.Equal(harness.UserId, harness.Context.TRN_ScheduleProposal.Single().UserId);
    }

    [Fact]
    public async Task Accept_KeepsDifferentTemplatesOnTheSameDayAndPeriod()
    {
        await using Harness harness = await Harness.CreateAsync();
        harness.Advisor.Response = harness.OneSlot("One session.");
        await harness.Service.GenerateAsync(harness.UserId);
        await harness.Service.UpdateSlotsAsync(harness.UserId,
        [
            Slot(DayOfWeek.Monday, harness.Catalog.Morning, harness.Catalog.StrengthSquat, 1),
            Slot(DayOfWeek.Monday, harness.Catalog.Morning, harness.Catalog.ModerateCardio, 2)
        ]);
        int proposalId = (await harness.Service.GetDraftAsync(harness.UserId))!.Id;

        ProposalCommandResult result = await harness.Service.AcceptAsync(harness.UserId, proposalId);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Proposal!.RecommendedSessionCount);
        DOWWorkout[] rows = harness.Context.TRN_DOWWorkout.OrderBy(item => item.SortOrder).ToArray();
        Assert.Equal(2, rows.Length);
        Assert.All(rows, item =>
        {
            Assert.Equal(harness.UserId, item.UserId);
            Assert.Equal(DayOfWeek.Monday, item.DayOfWeek);
            Assert.Equal(harness.Catalog.Morning, item.WorkoutPeriodId);
        });
        Assert.Equal([harness.Catalog.StrengthSquat, harness.Catalog.ModerateCardio], rows.Select(item => item.TemplateWorkoutId).ToArray());
        Assert.Equal([1, 2], rows.Select(item => item.SortOrder).ToArray());
    }

    [Fact]
    public async Task Accept_RejectsAProposalWithNoSlots()
    {
        await using Harness harness = await Harness.CreateAsync();
        ScheduleProposal draft = Proposal(harness.UserId, ProposalStatus.Draft, "Empty.", harness.Catalog);
        draft.Slots = [];
        harness.Context.TRN_ScheduleProposal.Add(draft);
        harness.Context.TRN_DOWWorkout.Add(ScheduleRow(harness));
        await harness.Context.SaveChangesAsync();

        ProposalCommandResult result = await harness.Service.AcceptAsync(harness.UserId, draft.Id);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Contains("no slots", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(ProposalStatus.Draft, harness.Context.TRN_ScheduleProposal.Single().Status);
        Assert.Equal(DayOfWeek.Monday, Assert.Single(harness.Context.TRN_DOWWorkout).DayOfWeek);
    }

    private static ScheduleProposal Proposal(Guid userId, ProposalStatus status, string explanation, Catalog catalog)
    {
        return new ScheduleProposal
        {
            UserId = userId,
            Status = status,
            FitnessLevelId = catalog.Novice,
            RecommendedSessionCount = 1,
            Explanation = explanation,
            CreatedAt = new DateTime(2026, 10, 1),
            Goals = [new ScheduleProposalGoal { TrainingGoalId = catalog.Strength }],
            Slots = [new ScheduleProposalSlot { DayOfWeek = DayOfWeek.Sunday, WorkoutPeriodId = catalog.Morning, TemplateWorkoutId = catalog.BeginnerFullBody, SortOrder = 1 }]
        };
    }

    private static DOWWorkout ScheduleRow(Harness harness)
    {
        return new DOWWorkout
        {
            UserId = harness.UserId,
            DayOfWeek = DayOfWeek.Monday,
            WorkoutPeriodId = harness.Catalog.Morning,
            TemplateWorkoutId = harness.Catalog.BeginnerFullBody,
            SortOrder = 1
        };
    }

    private static ProposalSlotInput Slot(int day, int periodId, int templateId, int sortOrder)
    {
        return new ProposalSlotInput
        {
            DayOfWeek = day,
            WorkoutPeriodId = periodId,
            TemplateWorkoutId = templateId,
            SortOrder = sortOrder
        };
    }

    private static ProposalSlotInput Slot(DayOfWeek day, int periodId, int templateId, int sortOrder)
    {
        return Slot((int)day, periodId, templateId, sortOrder);
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

    private sealed class SaveGate : SaveChangesInterceptor
    {
        public bool Throw { get; set; }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (Throw)
            {
                throw new InvalidOperationException("Simulated proposal save failure.");
            }

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (Throw)
            {
                throw new InvalidOperationException("Simulated proposal save failure.");
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
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
        public int StrengthSquat { get; init; }
    }

    private sealed class Harness : IAsyncDisposable
    {
        public required EntityDBContext Context { get; init; }
        public required IScheduleProposalService Service { get; init; }
        public required FakeAdvisor Advisor { get; init; }
        public required Catalog Catalog { get; init; }
        public Guid UserId { get; init; }

        public static async Task<Harness> CreateAsync(string? databaseName = null, SaveGate? gate = null)
        {
            EntityDBContext context = CreateContext(databaseName, gate);
            Catalog catalog = await SeedAsync(context);
            Guid userId = Guid.NewGuid();
            var preferences = new TrainingPreferenceService(
                new GenericRepoService<UserTrainingPreference>(context),
                new GenericRepoService<UserTrainingPreferenceGoal>(context),
                new GenericRepoService<FitnessLevel>(context),
                new GenericRepoService<TrainingGoal>(context));
            await preferences.SaveAsync(userId, catalog.Novice, [catalog.Strength]);
            var advisor = new FakeAdvisor();
            var service = new ScheduleProposalService(
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
            return new Harness
            {
                Context = context,
                Service = service,
                Advisor = advisor,
                Catalog = catalog,
                UserId = userId
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

        public static EntityDBContext CreateContext(string? databaseName, SaveGate? gate = null)
        {
            var builder = new DbContextOptionsBuilder<EntityDBContext>()
                .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString());
            if (gate != null)
            {
                builder.AddInterceptors(gate);
            }

            return new EntityDBContext(builder.Options);
        }

        public ValueTask DisposeAsync()
        {
            return Context.DisposeAsync();
        }

        private static async Task<Catalog> SeedAsync(EntityDBContext context)
        {
            context.LKP_FitnessLevel.AddRange(
                new FitnessLevel
                {
                    Description = "Novice",
                    Qualification = "No resistance-training experience, or none for several years.",
                    StrengthLoading = "8-12 reps at about 60-70% of 1RM. Start with 1-3 sets.",
                    MinSessionsPerWeek = 2,
                    MaxSessionsPerWeek = 3,
                    WeeklyTraining = "2-3 days, full body."
                },
                new FitnessLevel { Description = "Advanced", MinSessionsPerWeek = 4, MaxSessionsPerWeek = 6, WeeklyTraining = "4-6 days." });
            context.LKP_TrainingGoal.AddRange(
                new TrainingGoal { Description = "Fat loss" },
                new TrainingGoal { Description = "Strength" });
            context.LKP_WorkoutPeriod.AddRange(
                new WorkoutPeriod { Description = "Morning", StartTime = new TimeSpan(5, 0, 0), EndTime = new TimeSpan(12, 0, 0) },
                new WorkoutPeriod { Description = "Evening", StartTime = new TimeSpan(18, 1, 0), EndTime = new TimeSpan(23, 59, 0) });
            await context.SaveChangesAsync();

            int novice = context.LKP_FitnessLevel.Single(item => item.Description == "Novice").Id;
            int advanced = context.LKP_FitnessLevel.Single(item => item.Description == "Advanced").Id;
            int fatLoss = context.LKP_TrainingGoal.Single(item => item.Description == "Fat loss").Id;
            int strength = context.LKP_TrainingGoal.Single(item => item.Description == "Strength").Id;
            var beginner = new TemplateWorkout { Name = "Beginner Full Body", FitnessLevelId = novice };
            var cardio = new TemplateWorkout { Name = "Moderate Cardio", FitnessLevelId = novice };
            var squat = new TemplateWorkout { Name = "Strength Squat", FitnessLevelId = advanced };
            context.LKP_WorkoutTemplate.AddRange(beginner, cardio, squat);
            await context.SaveChangesAsync();
            context.LKP_TemplateTrainingGoal.AddRange(
                new TemplateTrainingGoal { TemplateWorkoutId = beginner.Id, TrainingGoalId = strength },
                new TemplateTrainingGoal { TemplateWorkoutId = cardio.Id, TrainingGoalId = fatLoss },
                new TemplateTrainingGoal { TemplateWorkoutId = squat.Id, TrainingGoalId = strength });
            await context.SaveChangesAsync();

            return new Catalog
            {
                Novice = novice,
                Strength = strength,
                Morning = context.LKP_WorkoutPeriod.Single(item => item.Description == "Morning").Id,
                Evening = context.LKP_WorkoutPeriod.Single(item => item.Description == "Evening").Id,
                BeginnerFullBody = beginner.Id,
                ModerateCardio = cardio.Id,
                StrengthSquat = squat.Id
            };
        }
    }
}
