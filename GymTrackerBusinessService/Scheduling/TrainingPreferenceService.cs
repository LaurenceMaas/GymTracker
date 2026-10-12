using GymTrackerBusinessService.Generic;
using GymTrackerDataModel.Models;

namespace GymTrackerBusinessService.Scheduling
{
    public interface ITrainingPreferenceService
    {
        Task<TrainingPreferenceDto?> GetAsync(Guid userId);
        Task<SaveTrainingPreferenceResult> SaveAsync(Guid userId, int fitnessLevelId, IReadOnlyCollection<int>? goalIds);
    }
    public sealed class TrainingPreferenceDto
    {
        public Guid UserId { get; init; }
        public int FitnessLevelId { get; init; }
        public string FitnessLevel { get; init; } = "";
        public int MinSessionsPerWeek { get; init; }
        public int MaxSessionsPerWeek { get; init; }
        public IReadOnlyList<TrainingGoalDto> Goals { get; init; } = [];
    }
    public sealed class TrainingGoalDto
    {
        public int Id { get; init; }
        public string Description { get; init; } = "";
    }
    public sealed class SaveTrainingPreferenceResult
    {
        public bool Succeeded { get; init; }
        public TrainingPreferenceDto? Preference { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = [];

        public static SaveTrainingPreferenceResult Success(TrainingPreferenceDto preference)
        {
            return new SaveTrainingPreferenceResult { Succeeded = true, Preference = preference };
        }

        public static SaveTrainingPreferenceResult Failure(IReadOnlyList<string> errors)
        {
            return new SaveTrainingPreferenceResult { Succeeded = false, Errors = errors };
        }
    }
    public sealed class TrainingPreferenceService : ITrainingPreferenceService
    {
        private readonly IGenericRepoService<UserTrainingPreference> _preferences;
        private readonly IGenericRepoService<UserTrainingPreferenceGoal> _preferenceGoals;
        private readonly IGenericRepoService<FitnessLevel> _fitnessLevels;
        private readonly IGenericRepoService<TrainingGoal> _trainingGoals;

        public TrainingPreferenceService(
            IGenericRepoService<UserTrainingPreference> preferences,
            IGenericRepoService<UserTrainingPreferenceGoal> preferenceGoals,
            IGenericRepoService<FitnessLevel> fitnessLevels,
            IGenericRepoService<TrainingGoal> trainingGoals)
        {
            _preferences = preferences;
            _preferenceGoals = preferenceGoals;
            _fitnessLevels = fitnessLevels;
            _trainingGoals = trainingGoals;
        }

        public async Task<TrainingPreferenceDto?> GetAsync(Guid userId)
        {
            UserTrainingPreference? preference = (await _preferences.QueryAsync(
                item => item.UserId == userId,
                trackChanges: false,
                "FitnessLevel",
                "Goals.TrainingGoal")).SingleOrDefault();

            return preference == null ? null : ToDto(preference);
        }

        public async Task<SaveTrainingPreferenceResult> SaveAsync(Guid userId, int fitnessLevelId, IReadOnlyCollection<int>? goalIds)
        {
            if (goalIds == null || goalIds.Count == 0)
            {
                return SaveTrainingPreferenceResult.Failure(["Select at least one training goal."]);
            }

            int[] distinctGoalIds = goalIds.Distinct().ToArray();
            FitnessLevel? fitnessLevel = (await _fitnessLevels.QueryAsync(
                item => item.Id == fitnessLevelId,
                trackChanges: true)).SingleOrDefault();
            IReadOnlyList<TrainingGoal> goals = await _trainingGoals.QueryAsync(
                item => distinctGoalIds.Contains(item.Id),
                trackChanges: true);

            var errors = new List<string>();
            if (fitnessLevel == null)
            {
                errors.Add($"Fitness level {fitnessLevelId} does not exist.");
            }

            foreach (int goalId in distinctGoalIds)
            {
                if (goals.All(item => item.Id != goalId))
                {
                    errors.Add($"Training goal {goalId} does not exist.");
                }
            }

            if (errors.Count > 0)
            {
                return SaveTrainingPreferenceResult.Failure(errors);
            }

            UserTrainingPreference? preference = (await _preferences.QueryAsync(
                item => item.UserId == userId,
                trackChanges: true,
                "Goals")).SingleOrDefault();

            if (preference == null)
            {
                preference = new UserTrainingPreference { UserId = userId };
                _preferences.StageAdd(preference);
            }

            preference.UserId = userId;
            preference.FitnessLevelId = fitnessLevel!.Id;
            preference.FitnessLevel = fitnessLevel;

            foreach (UserTrainingPreferenceGoal link in preference.Goals.ToList())
            {
                if (!distinctGoalIds.Contains(link.TrainingGoalId))
                {
                    preference.Goals.Remove(link);
                    _preferenceGoals.StageRemove(link);
                }
            }

            HashSet<int> currentGoalIds = preference.Goals.Select(item => item.TrainingGoalId).ToHashSet();
            foreach (int goalId in distinctGoalIds)
            {
                if (currentGoalIds.Contains(goalId))
                {
                    continue;
                }

                _preferenceGoals.StageAdd(new UserTrainingPreferenceGoal
                {
                    UserTrainingPreference = preference,
                    TrainingGoalId = goalId
                });
            }

            await SaveChangesAtomicallyAsync();
            return SaveTrainingPreferenceResult.Success(ToDto(userId, fitnessLevel, goals));
        }

        private async Task SaveChangesAtomicallyAsync()
        {
            await _preferences.BeginTransactionAsync();
            try
            {
                await _preferences.SaveAsync();
                await _preferences.CommitTransactionAsync();
            }
            catch
            {
                await _preferences.RollbackTransactionAsync();
                throw;
            }
        }

        private static TrainingPreferenceDto ToDto(UserTrainingPreference preference)
        {
            return ToDto(
                preference.UserId,
                preference.FitnessLevel,
                preference.Goals.Select(item => item.TrainingGoal).ToList());
        }

        private static TrainingPreferenceDto ToDto(Guid userId, FitnessLevel fitnessLevel, IEnumerable<TrainingGoal> goals)
        {
            return new TrainingPreferenceDto
            {
                UserId = userId,
                FitnessLevelId = fitnessLevel.Id,
                FitnessLevel = fitnessLevel.Description ?? "",
                MinSessionsPerWeek = fitnessLevel.MinSessionsPerWeek,
                MaxSessionsPerWeek = fitnessLevel.MaxSessionsPerWeek,
                Goals = goals
                    .OrderBy(item => item.Id)
                    .Select(item => new TrainingGoalDto
                    {
                        Id = item.Id,
                        Description = item.Description ?? ""
                    })
                    .ToList()
            };
        }
    }
}
