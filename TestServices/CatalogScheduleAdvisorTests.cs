using GymTrackerBusinessService.Scheduling;
using Xunit;

namespace TestServices;

public class CatalogScheduleAdvisorTests
{
    private const int Intermediate = 2;
    private const int Advanced = 3;
    private const int Strength = 2;
    private const int Size = 3;
    private const int Evening = 1;
    private const int Morning = 2;

    [Fact]
    public async Task IntermediateStrengthAndSize_ReturnsTheStructuredRecommendation()
    {
        FakeModel model = new()
        {
            Response = """
                {
                  "recommendedSessionCount": 3,
                  "explanation": "Three sessions spread across the week cover strength and size with Push, Pull, and Legs.",
                  "slots": [
                    { "dayOfWeek": 1, "workoutPeriodId": 2, "templateWorkoutId": 30, "sortOrder": 1 },
                    { "dayOfWeek": 3, "workoutPeriodId": 2, "templateWorkoutId": 31, "sortOrder": 2 },
                    { "dayOfWeek": 5, "workoutPeriodId": 2, "templateWorkoutId": 32, "sortOrder": 3 }
                  ]
                }
                """
        };
        CatalogScheduleAdvisor advisor = new(model);

        ScheduleAdvisorResponse response = await advisor.RecommendAsync(IntermediateRequest());

        Assert.Equal(1, model.Calls);
        Assert.Equal(CatalogScheduleAdvisor.Instructions, model.Instructions);
        Assert.Contains("Intermediate", model.Context);
        Assert.Contains("About 6 months of consistent resistance training.", model.Context);
        Assert.Contains("3-4 days.", model.Context);
        Assert.Contains("Strength", model.Context);
        Assert.Contains("Size", model.Context);
        Assert.Contains("Push", model.Context);
        Assert.Contains("Pull", model.Context);
        Assert.Contains("Legs", model.Context);
        Assert.Contains("Advanced Push", model.Context);
        Assert.Contains("Morning", model.Context);
        Assert.Contains("Evening", model.Context);
        Assert.DoesNotContain("UserId", model.Context);
        Assert.Equal(3, response.RecommendedSessionCount);
        Assert.Equal("Three sessions spread across the week cover strength and size with Push, Pull, and Legs.", response.Explanation);
        Assert.Equal([30, 31, 32], response.Slots.Select(item => item.TemplateWorkoutId).ToArray());
        Assert.Equal([1, 3, 5], response.Slots.Select(item => item.DayOfWeek).ToArray());
        Assert.Equal([2, 2, 2], response.Slots.Select(item => item.WorkoutPeriodId).ToArray());
        Assert.Equal([1, 2, 3], response.Slots.Select(item => item.SortOrder).ToArray());
    }

    [Fact]
    public async Task TheSameRequest_SendsTheSameContext()
    {
        FakeModel model = new()
        {
            Response = """
                {"recommendedSessionCount":1,"explanation":"One Push session.","slots":[{"dayOfWeek":1,"workoutPeriodId":2,"templateWorkoutId":30,"sortOrder":1}]}
                """
        };
        CatalogScheduleAdvisor advisor = new(model);
        ScheduleAdvisorRequest request = IntermediateRequest();

        ScheduleAdvisorResponse first = await advisor.RecommendAsync(request);
        ScheduleAdvisorResponse second = await advisor.RecommendAsync(request);

        Assert.Equal(model.Contexts[0], model.Contexts[1]);
        Assert.Equal(first.Explanation, second.Explanation);
        Assert.Equal(first.Slots[0].TemplateWorkoutId, second.Slots[0].TemplateWorkoutId);
    }

    [Fact]
    public async Task UnknownTemplateId_IsReturnedForTheValidator()
    {
        FakeModel model = new()
        {
            Response = """
                {"recommendedSessionCount":1,"explanation":"An unknown workout.","slots":[{"dayOfWeek":1,"workoutPeriodId":2,"templateWorkoutId":999,"sortOrder":1}]}
                """
        };

        ScheduleAdvisorResponse response = await new CatalogScheduleAdvisor(model).RecommendAsync(IntermediateRequest());

        Assert.Equal(999, Assert.Single(response.Slots).TemplateWorkoutId);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("""{"recommendedSessionCount":3,"slots":[]}""")]
    [InlineData("""{"recommendedSessionCount":1,"explanation":"Missing a day.","slots":[{"workoutPeriodId":2,"templateWorkoutId":30,"sortOrder":1}]}""")]
    public async Task UnreadableResponse_Fails(string response)
    {
        FakeModel model = new() { Response = response };

        ScheduleAdvisorException exception = await Assert.ThrowsAsync<ScheduleAdvisorException>(
            () => new CatalogScheduleAdvisor(model).RecommendAsync(IntermediateRequest()));

        Assert.Equal("The schedule advisor returned a response that could not be read.", exception.Message);
    }

    [Fact]
    public async Task EmptyCatalogue_DoesNotCallTheModel()
    {
        FakeModel model = new();
        ScheduleAdvisorRequest request = IntermediateRequest();
        request = new ScheduleAdvisorRequest
        {
            FitnessLevelId = request.FitnessLevelId,
            FitnessLevelName = request.FitnessLevelName,
            MinSessionsPerWeek = request.MinSessionsPerWeek,
            MaxSessionsPerWeek = request.MaxSessionsPerWeek,
            Goals = request.Goals,
            Periods = request.Periods,
            Templates = []
        };

        ScheduleAdvisorResponse response = await new CatalogScheduleAdvisor(model).RecommendAsync(request);

        Assert.Equal(0, model.Calls);
        Assert.Equal(0, response.RecommendedSessionCount);
        Assert.Empty(response.Slots);
    }

    [Fact]
    public async Task ModelFailure_Propagates()
    {
        FakeModel model = new() { Error = new ScheduleAdvisorException("The schedule advisor could not be reached.") };

        ScheduleAdvisorException exception = await Assert.ThrowsAsync<ScheduleAdvisorException>(
            () => new CatalogScheduleAdvisor(model).RecommendAsync(IntermediateRequest()));

        Assert.Equal("The schedule advisor could not be reached.", exception.Message);
    }

    private static ScheduleAdvisorRequest IntermediateRequest()
    {
        return new ScheduleAdvisorRequest
        {
            FitnessLevelId = Intermediate,
            FitnessLevelName = "Intermediate",
            MinSessionsPerWeek = 3,
            MaxSessionsPerWeek = 4,
            WeeklyTraining = "3-4 days. Three for full body, or four on a split so each muscle is trained twice.",
            Qualification = "About 6 months of consistent resistance training.",
            StrengthLoading = "A wider 1-12 rep range, varied over time.",
            Goals =
            [
                new ScheduleAdvisorGoal { Id = Strength, Name = "Strength" },
                new ScheduleAdvisorGoal { Id = Size, Name = "Size" }
            ],
            Periods =
            [
                new ScheduleAdvisorPeriod { Id = Evening, Name = "Evening" },
                new ScheduleAdvisorPeriod { Id = Morning, Name = "Morning" }
            ],
            Templates =
            [
                Template(30, "Push", Intermediate, "Intermediate", [Strength, Size]),
                Template(31, "Pull", Intermediate, "Intermediate", [Strength]),
                Template(32, "Legs", Intermediate, "Intermediate", [Size]),
                Template(40, "Advanced Push", Advanced, "Advanced", [Strength, Size])
            ]
        };
    }

    private static ScheduleAdvisorTemplate Template(int id, string name, int levelId, string levelName, int[] goalIds)
    {
        return new ScheduleAdvisorTemplate
        {
            Id = id,
            Name = name,
            FitnessLevelId = levelId,
            FitnessLevelName = levelName,
            GoalIds = goalIds
        };
    }

    private sealed class FakeModel : IScheduleLanguageModel
    {
        public int Calls { get; private set; }
        public string? Instructions { get; private set; }
        public string? Context { get; private set; }
        public List<string> Contexts { get; } = [];
        public string Response { get; init; } = "";
        public Exception? Error { get; init; }

        public Task<string> CompleteAsync(string instructions, string contextJson, CancellationToken cancellationToken = default)
        {
            Calls++;
            Instructions = instructions;
            Context = contextJson;
            Contexts.Add(contextJson);
            if (Error != null)
            {
                throw Error;
            }

            return Task.FromResult(Response);
        }
    }
}
