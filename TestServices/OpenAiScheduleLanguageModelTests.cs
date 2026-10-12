using System.Net;
using System.Text;
using GymTrackerBusinessService.Scheduling;
using Microsoft.Extensions.Options;
using Xunit;

namespace TestServices;

public class OpenAiScheduleLanguageModelTests
{
    [Fact]
    public async Task Complete_SendsThePromptAndReadsTheStructuredContent()
    {
        StubHandler handler = new()
        {
            Response = Json("""{"choices":[{"message":{"content":"{\"recommendedSessionCount\":3}"}}]}""")
        };
        OpenAiScheduleLanguageModel model = Client(handler, "test-key");

        string content = await model.CompleteAsync("Recommend a week.", """{"fitnessLevel":"Intermediate"}""");

        Assert.Contains("recommendedSessionCount", content);
        Assert.NotNull(handler.Request);
        Assert.Equal("Bearer", handler.Request.Headers.Authorization?.Scheme);
        Assert.Equal("test-key", handler.Request.Headers.Authorization?.Parameter);
        Assert.Contains("Recommend a week.", handler.Body);
        Assert.Contains("Intermediate", handler.Body);
        Assert.Contains("json_schema", handler.Body);
        Assert.Contains("schedule_recommendation", handler.Body);
        Assert.DoesNotContain("test-key", handler.Body);
    }

    [Fact]
    public async Task MissingApiKey_DoesNotCallTheService()
    {
        StubHandler handler = new() { Response = new HttpResponseMessage(HttpStatusCode.OK) };
        OpenAiScheduleLanguageModel model = Client(handler, "");

        ScheduleAdvisorException exception = await Assert.ThrowsAsync<ScheduleAdvisorException>(() => model.CompleteAsync("Recommend a week.", "{}"));

        Assert.Equal("The schedule advisor is not configured.", exception.Message);
        Assert.Null(handler.Request);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "The schedule advisor rejected the credentials.")]
    [InlineData(HttpStatusCode.TooManyRequests, "The schedule advisor is busy. Try again later.")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "The schedule advisor request failed.")]
    public async Task UnsuccessfulResponse_FailsWithoutASchedule(HttpStatusCode status, string expected)
    {
        StubHandler handler = new() { Response = new HttpResponseMessage(status) };
        OpenAiScheduleLanguageModel model = Client(handler, "test-key");

        ScheduleAdvisorException exception = await Assert.ThrowsAsync<ScheduleAdvisorException>(() => model.CompleteAsync("Recommend a week.", "{}"));

        Assert.Equal(expected, exception.Message);
    }

    [Fact]
    public async Task EmptyResponse_Fails()
    {
        StubHandler handler = new() { Response = Json("""{"choices":[]}""") };
        OpenAiScheduleLanguageModel model = Client(handler, "test-key");

        ScheduleAdvisorException exception = await Assert.ThrowsAsync<ScheduleAdvisorException>(() => model.CompleteAsync("Recommend a week.", "{}"));

        Assert.Equal("The schedule advisor returned an empty response.", exception.Message);
    }

    [Fact]
    public async Task Timeout_Fails()
    {
        StubHandler handler = new() { Error = new TaskCanceledException("timed out") };
        OpenAiScheduleLanguageModel model = Client(handler, "test-key");

        ScheduleAdvisorException exception = await Assert.ThrowsAsync<ScheduleAdvisorException>(() => model.CompleteAsync("Recommend a week.", "{}"));

        Assert.Equal("The schedule advisor did not respond in time.", exception.Message);
    }

    [Fact]
    public async Task NetworkFailure_Fails()
    {
        StubHandler handler = new() { Error = new HttpRequestException("no route") };
        OpenAiScheduleLanguageModel model = Client(handler, "test-key");

        ScheduleAdvisorException exception = await Assert.ThrowsAsync<ScheduleAdvisorException>(() => model.CompleteAsync("Recommend a week.", "{}"));

        Assert.Equal("The schedule advisor could not be reached.", exception.Message);
    }

    private static OpenAiScheduleLanguageModel Client(StubHandler handler, string apiKey)
    {
        return new OpenAiScheduleLanguageModel(new HttpClient(handler), Options.Create(new ScheduleAdvisorOptions
        {
            ApiKey = apiKey,
            Model = "gpt-4o-mini",
            Endpoint = "https://example.test/v1/chat/completions",
            TimeoutSeconds = 5
        }));
    }

    private static HttpResponseMessage Json(string body)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string? Body { get; private set; }
        public HttpResponseMessage? Response { get; init; }
        public Exception? Error { get; init; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            Body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            if (Error != null)
            {
                throw Error;
            }

            return Response ?? new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
