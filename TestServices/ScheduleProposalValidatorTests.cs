using GymTrackerBusinessService.Scheduling;
using Xunit;

namespace TestServices;

public class ScheduleProposalValidatorTests
{
    private const int BeginnerFullBody = 1;
    private const int PushWorkout = 2;
    private const int ModerateCardio = 3;
    private const int StrengthSquat = 8;
    private const int Morning = 1;
    private const int Afternoon = 2;
    private const int Evening = 3;

    private readonly ScheduleProposalValidator _validator = new();
    private readonly HashSet<int> _templates = [BeginnerFullBody, PushWorkout, ModerateCardio, StrengthSquat];
    private readonly HashSet<int> _periods = [Morning, Afternoon, Evening];

    [Fact]
    public void OneSlot_IsValid()
    {
        ProposalValidationResult result = _validator.ValidateAiProposal(
            1,
            [Slot(DayOfWeek.Monday, Morning, BeginnerFullBody)],
            _templates,
            _periods);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void SeveralSlots_AreValid()
    {
        ProposalValidationResult result = _validator.ValidateAiProposal(
            3,
            [
                Slot(DayOfWeek.Monday, Morning, BeginnerFullBody),
                Slot(DayOfWeek.Wednesday, Morning, BeginnerFullBody),
                Slot(DayOfWeek.Friday, Afternoon, ModerateCardio)
            ],
            _templates,
            _periods);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Sunday_IsAccepted()
    {
        ProposalValidationResult result = _validator.ValidateAiProposal(
            1,
            [Slot(DayOfWeek.Sunday, Morning, BeginnerFullBody)],
            _templates,
            _periods);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void TwoDifferentTemplates_OnTheSameDayAndPeriod_AreAccepted()
    {
        ProposalValidationResult result = _validator.ValidateUserProposal(
            [
                Slot(DayOfWeek.Monday, Morning, PushWorkout),
                Slot(DayOfWeek.Monday, Morning, BeginnerFullBody)
            ],
            _templates,
            _periods);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void TemplateFromAnotherFitnessLevel_IsAccepted()
    {
        ProposalValidationResult result = _validator.ValidateUserProposal(
            [Slot(DayOfWeek.Monday, Morning, StrengthSquat)],
            _templates,
            _periods);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void TemplateOutsideTheGoalPool_IsAccepted()
    {
        ProposalValidationResult result = _validator.ValidateUserProposal(
            [Slot(DayOfWeek.Tuesday, Morning, ModerateCardio)],
            _templates,
            _periods);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void UserEdit_WithFewerSlotsThanTheRecommendation_IsValid()
    {
        ProposalValidationResult result = _validator.ValidateUserProposal(
            [
                Slot(DayOfWeek.Monday, Morning, PushWorkout),
                Slot(DayOfWeek.Wednesday, Morning, PushWorkout),
                Slot(DayOfWeek.Friday, Morning, PushWorkout)
            ],
            _templates,
            _periods);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void FiveSessions_AreValidWhenTheCountMatchesTheSlots()
    {
        ProposalSlotInput[] slots =
        [
            Slot(DayOfWeek.Monday, Morning, BeginnerFullBody),
            Slot(DayOfWeek.Tuesday, Morning, ModerateCardio),
            Slot(DayOfWeek.Wednesday, Morning, BeginnerFullBody),
            Slot(DayOfWeek.Thursday, Evening, ModerateCardio),
            Slot(DayOfWeek.Friday, Afternoon, PushWorkout)
        ];

        ProposalValidationResult result = _validator.ValidateAiProposal(5, slots, _templates, _periods);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void NullSlots_AreRejected()
    {
        ProposalValidationResult result = _validator.ValidateAiProposal(1, null, _templates, _periods);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("no slots", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void EmptySlots_AreRejected()
    {
        ProposalValidationResult result = _validator.ValidateUserProposal([], _templates, _periods);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("no slots", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UnknownTemplate_IsRejected()
    {
        ProposalValidationResult result = _validator.ValidateUserProposal(
            [Slot(DayOfWeek.Monday, Morning, templateId: 99)],
            _templates,
            _periods);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("template 99", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UnknownPeriod_IsRejected()
    {
        ProposalValidationResult result = _validator.ValidateUserProposal(
            [Slot(DayOfWeek.Monday, periodId: 50, BeginnerFullBody)],
            _templates,
            _periods);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("period 50", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DayOutsideTheWeek_IsRejected()
    {
        ProposalValidationResult result = _validator.ValidateUserProposal(
            [Slot(day: 7, Morning, BeginnerFullBody)],
            _templates,
            _periods);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("day 7", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DuplicateSlot_IsRejected()
    {
        ProposalSlotInput slot = Slot(DayOfWeek.Sunday, Morning, BeginnerFullBody);
        ProposalValidationResult result = _validator.ValidateUserProposal([slot, slot], _templates, _periods);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("duplicates", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AiProposal_WhoseCountDiffersFromTheSlotCount_IsRejected()
    {
        ProposalValidationResult result = _validator.ValidateAiProposal(
            4,
            [
                Slot(DayOfWeek.Monday, Morning, PushWorkout),
                Slot(DayOfWeek.Wednesday, Morning, PushWorkout),
                Slot(DayOfWeek.Friday, Morning, PushWorkout)
            ],
            _templates,
            _periods);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("recommended session count is 4", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Errors, error => error.Contains("3 slots", StringComparison.OrdinalIgnoreCase));
    }

    private static ProposalSlotInput Slot(DayOfWeek day, int periodId, int templateId)
    {
        return Slot((int)day, periodId, templateId);
    }

    private static ProposalSlotInput Slot(int day, int periodId, int templateId)
    {
        return new ProposalSlotInput
        {
            ScheduleProposalId = 10,
            DayOfWeek = day,
            WorkoutPeriodId = periodId,
            TemplateWorkoutId = templateId
        };
    }
}
