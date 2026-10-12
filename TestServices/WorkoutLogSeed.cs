using GymTrackerDataModel;
using GymTrackerDataModel.Models;

namespace TestServices;

internal static class WorkoutLogSeed
{
    public const string RepsName = "Reps";
    public const string BodyWeightName = "Percentage Body Weight";

    public static readonly (int Order, int Reps, decimal BodyWeightPercent)[] Sets =
    [
        (1, 12, 40m),
        (2, 10, 50m),
        (3, 8, 60m),
        (4, 6, 70m)
    ];

    public static string PlannedSetMetrics(int reps, decimal bodyWeightPercent)
        => $"{reps} reps {RepsName} @\r\n{bodyWeightPercent} % {BodyWeightName}";

    public static string RepsLabel(int reps) => $"{RepsName} ({reps}reps)";

    public static string BodyWeightLabel(decimal bodyWeightPercent) => $"{BodyWeightName} ({bodyWeightPercent}%)";

    public static async Task<TemplateExercise> SeedSquatAsync(EntityDBContext context)
    {
        var workout = new TemplateWorkout { Name = "Beginner Full Body" };
        var exerciseType = new ExerciseType { Description = "Resistance" };
        var squat = AddExercise(workout, exerciseType, "Barbell Squat");

        context.LKP_WorkoutTemplate.Add(workout);
        await context.SaveChangesAsync();
        return squat;
    }

    public static async Task<TemplateWorkout> SeedBeginnerFullBodyAsync(EntityDBContext context)
    {
        var workout = new TemplateWorkout { Name = "Beginner Full Body" };
        var exerciseType = new ExerciseType { Description = "Resistance" };

        foreach (var exerciseName in new[]
        {
            "Barbell Squat",
            "Barbell Bench Press",
            "Bent Over Row",
            "Deadlift"
        })
        {
            AddExercise(workout, exerciseType, exerciseName);
        }

        context.LKP_WorkoutPeriod.Add(new WorkoutPeriod
        {
            Description = "All day",
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.FromHours(24)
        });
        context.LKP_WorkoutTemplate.Add(workout);
        await context.SaveChangesAsync();
        return workout;
    }

    public static async Task<ActualExercise> LogFirstSetAsync(
        EntityDBContext context,
        TemplateExercise templateExercise,
        decimal actualReps,
        decimal actualBodyWeightPercent)
    {
        var templateSet = templateExercise.TemplateSets.Single(set => set.Order == 1);
        var actualExercise = new ActualExercise
        {
            ExerciseId = templateExercise.ExerciseId,
            ExerciseName = templateExercise.Exercise.Name,
            ExerciseType = templateExercise.Exercise.ExerciseType.Description,
            ActualWorkoutId = 1,
            Createdatetime = DateTime.UtcNow
        };
        var actualSet = new ActualSet
        {
            ActualExercise = actualExercise,
            TemplateSetId = templateSet.Id,
            ExecutionOrder = templateSet.Order,
            Completed = true,
            Createdatetime = DateTime.UtcNow
        };

        foreach (var metric in templateSet.TemplateSetMetrics)
        {
            context.TRN_ActualSetMetric.Add(new ActualSetMetric
            {
                ActualSet = actualSet,
                MetricId = metric.MetricId,
                PlannedNumericValue = metric.NumericValue ?? 0,
                PlannedTextValue = metric.TextValue,
                ActualNumericValue = metric.Metric.Name == RepsName ? actualReps : actualBodyWeightPercent,
                Createdatetime = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        return actualExercise;
    }

    private static TemplateExercise AddExercise(TemplateWorkout workout, ExerciseType exerciseType, string exerciseName)
    {
        var reps = new Metric { Name = RepsName, Unit = "reps", ValueType = MetricValueType.Numeric };
        var bodyWeight = new Metric { Name = BodyWeightName, Unit = "%", ValueType = MetricValueType.Numeric };
        var templateExercise = new TemplateExercise
        {
            WorkoutTemplate = workout,
            Exercise = new Exercise
            {
                Name = exerciseName,
                ExerciseType = exerciseType
            }
        };

        foreach (var plan in Sets)
        {
            var set = new TemplateSet
            {
                Order = plan.Order,
                TemplateExercise = templateExercise
            };
            set.TemplateSetMetrics.Add(new TemplateSetMetric
            {
                Metric = reps,
                NumericValue = plan.Reps
            });
            set.TemplateSetMetrics.Add(new TemplateSetMetric
            {
                Metric = bodyWeight,
                NumericValue = plan.BodyWeightPercent
            });
            templateExercise.TemplateSets.Add(set);
        }

        workout.Exercises.Add(templateExercise);
        return templateExercise;
    }
}
