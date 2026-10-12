using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace GymTrackerBusinessService.Scheduling
{
    public sealed class OpenAiScheduleLanguageModel : IScheduleLanguageModel
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly JsonElement Schema = JsonDocument.Parse("""
            {
              "type": "object",
              "additionalProperties": false,
              "required": ["recommendedSessionCount", "explanation", "slots"],
              "properties": {
                "recommendedSessionCount": { "type": "integer" },
                "explanation": { "type": "string" },
                "slots": {
                  "type": "array",
                  "items": {
                    "type": "object",
                    "additionalProperties": false,
                    "required": ["dayOfWeek", "workoutPeriodId", "templateWorkoutId", "sortOrder"],
                    "properties": {
                      "dayOfWeek": { "type": "integer" },
                      "workoutPeriodId": { "type": "integer" },
                      "templateWorkoutId": { "type": "integer" },
                      "sortOrder": { "type": "integer" }
                    }
                  }
                }
              }
            }
            """).RootElement.Clone();

        private readonly HttpClient _http;
        private readonly ScheduleAdvisorOptions _options;

        public OpenAiScheduleLanguageModel(HttpClient http, IOptions<ScheduleAdvisorOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options);
            _http = http;
            _options = options.Value;
            int timeoutSeconds = _options.TimeoutSeconds > 0 ? _options.TimeoutSeconds : 30;
            _http.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        }

        public async Task<string> CompleteAsync(string instructions, string contextJson, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(instructions);
            ArgumentException.ThrowIfNullOrWhiteSpace(contextJson);

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new ScheduleAdvisorException("The schedule advisor is not configured.");
            }

            if (string.IsNullOrWhiteSpace(_options.Model) || !Uri.TryCreate(_options.Endpoint, UriKind.Absolute, out Uri? endpoint))
            {
                throw new ScheduleAdvisorException("The schedule advisor endpoint is not configured.");
            }

            var body = new ChatRequest
            {
                Model = _options.Model,
                Messages =
                [
                    new ChatMessage { Role = "system", Content = instructions },
                    new ChatMessage { Role = "user", Content = contextJson }
                ],
                ResponseFormat = new ResponseFormat
                {
                    Type = "json_schema",
                    JsonSchema = new JsonSchemaSpec
                    {
                        Name = "schedule_recommendation",
                        Strict = true,
                        Schema = Schema
                    }
                }
            };

            using var message = new HttpRequestMessage(HttpMethod.Post, endpoint);
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            message.Content = JsonContent.Create(body, options: JsonOptions);

            HttpResponseMessage response;
            try
            {
                response = await _http.SendAsync(message, cancellationToken);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new ScheduleAdvisorException("The schedule advisor did not respond in time.");
            }
            catch (HttpRequestException)
            {
                throw new ScheduleAdvisorException("The schedule advisor could not be reached.");
            }

            using (response)
            {
                if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
                {
                    throw new ScheduleAdvisorException("The schedule advisor rejected the credentials.");
                }

                if ((int)response.StatusCode == 429)
                {
                    throw new ScheduleAdvisorException("The schedule advisor is busy. Try again later.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new ScheduleAdvisorException("The schedule advisor request failed.");
                }

                await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                if (!document.RootElement.TryGetProperty("choices", out JsonElement choices)
                    || choices.ValueKind != JsonValueKind.Array
                    || choices.GetArrayLength() == 0
                    || !choices[0].TryGetProperty("message", out JsonElement messageElement)
                    || !messageElement.TryGetProperty("content", out JsonElement content)
                    || content.ValueKind != JsonValueKind.String)
                {
                    throw new ScheduleAdvisorException("The schedule advisor returned an empty response.");
                }

                string? text = content.GetString();
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new ScheduleAdvisorException("The schedule advisor returned an empty response.");
                }

                return text;
            }
        }

        private sealed class ChatRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; init; } = "";

            [JsonPropertyName("messages")]
            public ChatMessage[] Messages { get; init; } = [];

            [JsonPropertyName("response_format")]
            public ResponseFormat ResponseFormat { get; init; } = new();
        }

        private sealed class ChatMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; init; } = "";

            [JsonPropertyName("content")]
            public string Content { get; init; } = "";
        }

        private sealed class ResponseFormat
        {
            [JsonPropertyName("type")]
            public string Type { get; init; } = "";

            [JsonPropertyName("json_schema")]
            public JsonSchemaSpec JsonSchema { get; init; } = new();
        }

        private sealed class JsonSchemaSpec
        {
            [JsonPropertyName("name")]
            public string Name { get; init; } = "";

            [JsonPropertyName("strict")]
            public bool Strict { get; init; }

            [JsonPropertyName("schema")]
            public JsonElement Schema { get; init; }
        }
    }
}
