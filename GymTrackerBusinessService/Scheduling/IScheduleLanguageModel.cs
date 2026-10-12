namespace GymTrackerBusinessService.Scheduling
{
    public interface IScheduleLanguageModel
    {
        Task<string> CompleteAsync(string instructions, string contextJson, CancellationToken cancellationToken = default);
    }

    public sealed class ScheduleAdvisorException : Exception
    {
        public ScheduleAdvisorException(string message) : base(message)
        {
        }
    }
}
