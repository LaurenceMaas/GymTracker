namespace GymTrackerBusinessService.Scheduling
{
    public sealed class ScheduleAdvisorOptions
    {
        public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
        public string Model { get; set; } = "gpt-4o-mini";
        public string ApiKey { get; set; } = "";
        public int TimeoutSeconds { get; set; } = 30;
    }
}
