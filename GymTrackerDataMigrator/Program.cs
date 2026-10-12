using GymTrackerDataModel;
using GymTrackerDataModel.Models;
using GymTrackerHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System.Reflection.Emit;

IConfigurationRoot _configuration;
DbContextOptionsBuilder<EntityDBContext> _optionsBuilder;

BuildOptions();
ApplyMigrations();
CreateSeedData(new EntityDBContext(_optionsBuilder.Options));
void CreateSeedData(EntityDBContext context)
{

    if (!context.LKP_ExerciseType.Any(d => d.Id == 1))
    {
        DbSet<ExerciseType> exerciseType = context.LKP_ExerciseType;
        exerciseType.AddRange(
        new ExerciseType { Description = "Aerobic" },
        new ExerciseType { Description = "Anaerobic" }
        );
        context.SaveChanges();
    }
    if (!context.LKP_Exercise.Any())
    {
        var now = DateTime.Now;
        var user = "laurencemaasdorp@gmail.com";

        DbSet<Exercise> exercises = context.LKP_Exercise;

        exercises.AddRange(

        // CHEST
        new Exercise { Name = "Barbell Bench Press", ExerciseTypeId = 2, StepsToPerform = "Lower bar to chest and press upward.", VideoLink = "https://www.youtube.com/watch?v=rT7DgCr-3pg" },
        new Exercise { Name = "Incline Bench Press", ExerciseTypeId = 2, StepsToPerform = "Press bar upward on incline bench.", VideoLink = "https://www.youtube.com/watch?v=DbFgADa2PL8" },
        new Exercise { Name = "Decline Bench Press", ExerciseTypeId = 2, StepsToPerform = "Press bar on decline bench.", VideoLink = "https://www.youtube.com/watch?v=LfyQBUKR8SE" },
        new Exercise { Name = "Dumbbell Press", ExerciseTypeId = 2, StepsToPerform = "Press dumbbells upward from chest.", VideoLink = "https://www.youtube.com/watch?v=VmB1G1K7v94" },
        new Exercise { Name = "Dumbbell Fly", ExerciseTypeId = 2, StepsToPerform = "Open arms wide and bring together.", VideoLink = "https://www.youtube.com/watch?v=eozdVDA78K0" },
        new Exercise { Name = "Push Up", ExerciseTypeId = 2, StepsToPerform = "Lower body to floor and push up.", VideoLink = "https://www.youtube.com/watch?v=IODxDxX7oi4" },

        // BACK
        new Exercise { Name = "Deadlift", ExerciseTypeId = 2, StepsToPerform = "Lift bar from floor keeping back straight.", VideoLink = "https://www.youtube.com/watch?v=ytGaGIn3SjE" },
        new Exercise { Name = "Pull Up", ExerciseTypeId = 2, StepsToPerform = "Pull body until chin passes bar.", VideoLink = "https://www.youtube.com/watch?v=eGo4IYlbE5g" },
        new Exercise { Name = "Lat Pulldown", ExerciseTypeId = 2, StepsToPerform = "Pull bar to chest.", VideoLink = "https://www.youtube.com/watch?v=CAwf7n6Luuc" },
        new Exercise { Name = "Seated Row", ExerciseTypeId = 2, StepsToPerform = "Pull handle toward torso.", VideoLink = "https://www.youtube.com/watch?v=GZbfZ033f74" },
        new Exercise { Name = "Bent Over Row", ExerciseTypeId = 2, StepsToPerform = "Row bar toward stomach.", VideoLink = "https://www.youtube.com/watch?v=vT2GjY_Umpw" },
        new Exercise { Name = "T-Bar Row", ExerciseTypeId = 2, StepsToPerform = "Pull bar toward chest.", VideoLink = "https://www.youtube.com/watch?v=j3Igk5nyZE4" },

        // SHOULDERS
        new Exercise { Name = "Shoulder Press", ExerciseTypeId = 2, StepsToPerform = "Press weight overhead.", VideoLink = "https://www.youtube.com/watch?v=qEwKCR5JCog" },
        new Exercise { Name = "Dumbbell Shoulder Press", ExerciseTypeId = 2, StepsToPerform = "Press dumbbells overhead.", VideoLink = "https://www.youtube.com/watch?v=B-aVuyhvLHU" },
        new Exercise { Name = "Lateral Raise", ExerciseTypeId = 2, StepsToPerform = "Raise arms to the side.", VideoLink = "https://www.youtube.com/watch?v=kDqklk1ZESo" },
        new Exercise { Name = "Front Raise", ExerciseTypeId = 2, StepsToPerform = "Raise arms forward.", VideoLink = "https://www.youtube.com/watch?v=-t7fuZ0KhDA" },
        new Exercise { Name = "Rear Delt Fly", ExerciseTypeId = 2, StepsToPerform = "Raise arms backward.", VideoLink = "https://www.youtube.com/watch?v=EA7u4Q_8HQ0" },
        new Exercise { Name = "Shrugs", ExerciseTypeId = 2, StepsToPerform = "Lift shoulders upward.", VideoLink = "https://www.youtube.com/watch?v=cJRVVxmytaM" },

        // ARMS
        new Exercise { Name = "Barbell Curl", ExerciseTypeId = 2, StepsToPerform = "Curl bar toward shoulders.", VideoLink = "https://www.youtube.com/watch?v=kwG2ipFRgfo" },
        new Exercise { Name = "Dumbbell Curl", ExerciseTypeId = 2, StepsToPerform = "Curl dumbbells upward.", VideoLink = "https://www.youtube.com/watch?v=ykJmrZ5v0Oo" },
        new Exercise { Name = "Hammer Curl", ExerciseTypeId = 2, StepsToPerform = "Curl with neutral grip.", VideoLink = "https://www.youtube.com/watch?v=zC3nLlEvin4" },
        new Exercise { Name = "Preacher Curl", ExerciseTypeId = 2, StepsToPerform = "Curl on preacher bench.", VideoLink = "https://www.youtube.com/watch?v=fIWP-FRFNU0" },
        new Exercise { Name = "Tricep Pushdown", ExerciseTypeId = 2, StepsToPerform = "Push cable downward.", VideoLink = "https://www.youtube.com/watch?v=2-LAMcpzODU" },
        new Exercise { Name = "Skull Crusher", ExerciseTypeId = 2, StepsToPerform = "Lower bar to forehead.", VideoLink = "https://www.youtube.com/watch?v=d_KZxkY_0cM" },

        // LEGS
        new Exercise { Name = "Barbell Squat", ExerciseTypeId = 2, StepsToPerform = "Squat down and stand up.", VideoLink = "https://www.youtube.com/watch?v=Dy28eq2PjcM" },
        new Exercise { Name = "Front Squat", ExerciseTypeId = 2, StepsToPerform = "Squat with bar in front.", VideoLink = "https://www.youtube.com/watch?v=tlfGU8vv1eU" },
        new Exercise { Name = "Leg Press", ExerciseTypeId = 2, StepsToPerform = "Push weight with legs.", VideoLink = "https://www.youtube.com/watch?v=IZxyjW7MPJQ" },
        new Exercise { Name = "Lunges", ExerciseTypeId = 2, StepsToPerform = "Step forward and lower.", VideoLink = "https://www.youtube.com/watch?v=QOVaHwm-Q6U" },
        new Exercise { Name = "Leg Curl", ExerciseTypeId = 2, StepsToPerform = "Curl legs toward body.", VideoLink = "https://www.youtube.com/watch?v=1Tq3QdYUuHs" },
        new Exercise { Name = "Leg Extension", ExerciseTypeId = 2, StepsToPerform = "Extend legs forward.", VideoLink = "https://www.youtube.com/watch?v=YyvSfVjQeL0" },
        new Exercise { Name = "Calf Raise", ExerciseTypeId = 2, StepsToPerform = "Raise heels upward.", VideoLink = "https://www.youtube.com/watch?v=-M4-G8p8fmc" },

        // CORE
        new Exercise { Name = "Sit Up", ExerciseTypeId = 2, StepsToPerform = "Lift torso upward.", VideoLink = "https://www.youtube.com/watch?v=jDwoBqPH0jk" },
        new Exercise { Name = "Crunch", ExerciseTypeId = 2, StepsToPerform = "Short abdominal lift.", VideoLink = "https://www.youtube.com/watch?v=Xyd_fa5zoEU" },
        new Exercise { Name = "Plank", ExerciseTypeId = 2, StepsToPerform = "Hold straight body position.", VideoLink = "https://www.youtube.com/watch?v=pSHjTRCQxIw" },
        new Exercise { Name = "Leg Raise", ExerciseTypeId = 2, StepsToPerform = "Raise legs upward.", VideoLink = "https://www.youtube.com/watch?v=JB2oyawG9KI" },

        // CARDIO
        new Exercise { Name = "Running", ExerciseTypeId = 1, StepsToPerform = "Run at steady pace.", VideoLink = "https://www.youtube.com/watch?v=brFHyOtTwH4" },
        new Exercise { Name = "Cycling", ExerciseTypeId = 1, StepsToPerform = "Pedal continuously.", VideoLink = "https://www.youtube.com/watch?v=1VYlOKUdylM" },
        new Exercise { Name = "Rowing Machine", ExerciseTypeId = 1, StepsToPerform = "Pull handle toward body.", VideoLink = "https://www.youtube.com/watch?v=roCP6wCXPqo" },
        new Exercise { Name = "Jump Rope", ExerciseTypeId = 1, StepsToPerform = "Jump rope repeatedly.", VideoLink = "https://www.youtube.com/watch?v=1BZM7kGZp0c" }

        );

        context.SaveChanges();

    }
    if (!context.LKP_Metric.Any())
    {
        DbSet<Metric> exercises = context.LKP_Metric;
        exercises.AddRange(
        new Metric { Name = "Reps", Unit = "reps", ValueType = MetricValueType.Numeric },
        new Metric { Name = "Percentage Body Weight", Unit = "%", ValueType = MetricValueType.Numeric },
        new Metric { Name = "Duration", Unit = "sec", ValueType = MetricValueType.Numeric },
        new Metric { Name = "Distance", Unit = "km", ValueType = MetricValueType.Numeric },
        new Metric { Name = "Tempo", Unit = null, ValueType = MetricValueType.Text },
        new Metric { Name = "Notes", Unit = null, ValueType = MetricValueType.Text },
        new Metric { Name = "Heart Rate", Unit = "bpm", ValueType = MetricValueType.Numeric },
        new Metric { Name = "Percent 1RM", Unit = "%", ValueType = MetricValueType.Numeric },
        new Metric { Name = "Percent HRmax", Unit = "%", ValueType = MetricValueType.Numeric });
        context.SaveChanges();
    }
    EnsureMetric(context, "Percent 1RM", "%");
    EnsureMetric(context, "Percent HRmax", "%");
    RebuildSeededWorkouts(context);
    SeedTemplateTrainingGoals(context);
    if (!context.LKP_ExerciseMetric.Any())
    {
        int reps = context.LKP_Metric.First(x => x.Name == "Reps").Id;
        int weight = context.LKP_Metric.First(x => x.Name == "Percentage Body Weight").Id;
        int duration = context.LKP_Metric.First(x => x.Name == "Duration").Id;
        int distance = context.LKP_Metric.First(x => x.Name == "Distance").Id;
        int heartRate = context.LKP_Metric.First(x => x.Name == "Heart Rate").Id;

        var allExercises = context.LKP_Exercise.ToList();

        foreach (var ex in allExercises)
        {
            // CARDIO
            if (ex.ExerciseTypeId == 1)
            {
                context.LKP_ExerciseMetric.AddRange(

                    new ExerciseMetric
                    {
                        ExerciseId = ex.Id,
                        MetricId = duration
                    },

                    new ExerciseMetric
                    {
                        ExerciseId = ex.Id,
                        MetricId = distance
                    },

                    new ExerciseMetric
                    {
                        ExerciseId = ex.Id,
                        MetricId = heartRate
                    }
                );
            }

            // RESISTANCE
            if (ex.ExerciseTypeId == 2)
            {
                context.LKP_ExerciseMetric.AddRange(

                    new ExerciseMetric
                    {
                        ExerciseId = ex.Id,
                        MetricId = reps
                    },

                    new ExerciseMetric
                    {
                        ExerciseId = ex.Id,
                        MetricId = weight
                    }
                );

                // Special case for plank
                if (ex.Name == "Plank")
                {
                    context.LKP_ExerciseMetric.Add(
                        new ExerciseMetric
                        {
                            ExerciseId = ex.Id,
                            MetricId = duration
                        });
                }
            }
        }

        context.SaveChanges();
    }

    if (!context.LKP_WorkoutPeriod.Any(d => d.Id == 1))
    {
        DbSet<WorkoutPeriod> workoutPeriod = context.LKP_WorkoutPeriod;
        workoutPeriod.AddRange(
        new WorkoutPeriod { Description = "Morning",StartTime = new TimeSpan(5,0,0), EndTime = new TimeSpan(12,0,0) },
        new WorkoutPeriod { Description = "Afternoon" ,StartTime = new TimeSpan(12, 1,0), EndTime = new TimeSpan(18, 0,0) },
        new WorkoutPeriod { Description = "Evening", StartTime = new TimeSpan(18, 1,0), EndTime = new TimeSpan(23, 59, 0) }
        );
        context.SaveChanges();
    }

    if (!context.LKP_WorkOutStatus.Any(d => d.Id == 1))
    {
        DbSet<WorkOutStatus> workOutStatus = context.LKP_WorkOutStatus;
        workOutStatus.AddRange(
        new WorkOutStatus { Description = "Started" },
        new WorkOutStatus { Description = "Abandoned" },
        new WorkOutStatus { Description = "Ended" }
        );
        context.SaveChanges();
    }

    SeedNoviceSchedule(context);
}

void EnsureMetric(EntityDBContext context, string name, string unit)
{
    if (context.LKP_Metric.Any(x => x.Name == name))
    {
        return;
    }

    context.LKP_Metric.Add(new Metric
    {
        Name = name,
        Unit = unit,
        ValueType = MetricValueType.Numeric
    });
    context.SaveChanges();
}

void RebuildSeededWorkouts(EntityDBContext context)
{
    const string fullBodyReference = "https://pubmed.ncbi.nlm.nih.gov/21694556/";
    const string splitReference = "https://pubmed.ncbi.nlm.nih.gov/19204579/";
    const string hiitReference = "https://pubmed.ncbi.nlm.nih.gov/17414804/";
    const string cardioReference = "https://www.who.int/publications/i/item/9789240015128";
    const string hypertrophyReference = "https://pubmed.ncbi.nlm.nih.gov/27433992/";

    var workouts = new (string Name, string Notes, string Reference, string Level)[]
    {
        ("Beginner Full Body", "Full-body strength on Monday, Wednesday, and Friday. Two sets. Use a load near 65% of 1RM and leave about 1-3 reps in reserve. Rest 2-3 minutes between compound sets. Plank is a 20-second hold.", fullBodyReference, "Novice"),
        ("Push Workout", "Chest, shoulders, and triceps. Perform twice a week. Compounds are 3 sets of 8 at about 75% of 1RM. Isolation work is 3 sets of 12 at about 65% of 1RM. Rest 2-3 minutes on compounds and 1-2 minutes on isolation.", splitReference, "Intermediate"),
        ("Pull Workout", "Back and biceps. Perform twice a week. Compounds are 3 sets of 8-10. Isolation work is 3 sets of 12 at about 65% of 1RM. Rest 2-3 minutes on compounds and 1-2 minutes on isolation.", splitReference, "Intermediate"),
        ("Leg Workout", "Lower body. Perform twice a week. Squats are 3 sets of 8 at about 75% of 1RM. Lunges are 8 reps per leg. Rest 2-3 minutes on squats and 1-2 minutes on isolation.", splitReference, "Intermediate"),
        ("HIIT 4x4", "Running. Odd sets are 4-minute hard efforts and even sets are 3-minute easy recoveries. Up to 3 days a week, separate from strength training. The 16 hard minutes count as vigorous aerobic time. Hard efforts are prescribed at 90-95% HRmax; the stored target is 90. Recovery is 70% HRmax.", hiitReference, "Advanced"),
        ("Moderate Cardio", "Moderate cycling for 50 minutes. Keep a pace where talking is possible and singing is not. Tuesday, Thursday, and Saturday.", cardioReference, "Novice"),
        ("Advanced Push", "Chest, shoulders, and triceps for trained lifters. Repeat up to twice a week. Once is about 7 hard chest sets; twice is a high-volume week. Rest 2-3 minutes on presses and 1-2 minutes on isolation. Leave about 1-2 reps in reserve.", hypertrophyReference, "Advanced"),
        ("Advanced Pull", "Back and biceps for trained lifters. Repeat up to twice a week. Rest 2-3 minutes on rows and pulldowns and 1-2 minutes on isolation. Leave about 1-2 reps in reserve.", hypertrophyReference, "Advanced"),
        ("Advanced Legs", "Lower body for trained lifters. Repeat up to twice a week. Twice is a high-volume week, so do not also run Strength Squat or Strength Deadlift that week. Lunges are 8 reps per leg. Rest 2-3 minutes on squats.", hypertrophyReference, "Advanced"),
        ("Strength Upper", "Heavy upper body. Perform twice a week for about 8 sets per lift at 85% of 1RM. Rest 3-5 minutes. Leave about 1-2 reps in reserve. Loads are not all-out repetition maximums.", splitReference, "Advanced"),
        ("Strength Squat", "Heavy squat day. Rest 3-5 minutes on the squats. Leave about 1-2 reps in reserve. Pair with Strength Deadlift on another day rather than Advanced Legs in the same week.", splitReference, "Advanced"),
        ("Strength Deadlift", "Heavy deadlift day. Triples at 85% of 1RM leave reps in reserve and are not all-out 3-rep maxes. Lunges are 6 reps per leg. Rest 3-5 minutes on deadlifts.", splitReference, "Advanced"),
        ("Heavy Full Body", "Heavy day of a three-day full-body week. Squat, bench, and row at 85% of 1RM. Rest 3-5 minutes. Leave about 1-2 reps in reserve.", splitReference, "Advanced"),
        ("Hypertrophy Full Body", "Moderate day of a three-day full-body week. Compounds are 3 sets of 8 at 75% of 1RM. Rest 2-3 minutes on compounds.", splitReference, "Intermediate"),
        ("Power Full Body", "Fast day of a three-day full-body week. Use 3 reps at 40-50% of 1RM and accelerate each lift. Lower the bar under control. Rest about 3 minutes. These are normal squats and presses, not Olympic lifts.", splitReference, "Advanced"),
        ("Muscular Endurance", "Local muscular endurance. 15 reps at 40-50% of 1RM with rest under 90 seconds. Lunges are 15 reps per leg. Crunches are bodyweight.", splitReference, "Novice"),
        ("Vigorous Run", "Continuous running for 25 minutes at 80% HRmax, inside the vigorous band of 77-95%. Three sessions equal 75 minutes of vigorous aerobic time.", fullBodyReference, "Intermediate"),
        ("Easy Run", "Easy running for 45 minutes at 65% HRmax. Keep a pace where talking is comfortable. Counts as moderate aerobic time.", cardioReference, "Novice"),
        ("Steady Row", "Steady rowing for 40 minutes at 70% HRmax, in the middle of the moderate band of 64-76%.", fullBodyReference, "Intermediate"),
        ("HIIT 4x4 Cycling", "Cycling. Same 4-minute hard and 3-minute easy pattern as HIIT 4x4. Hard efforts are stored at 90% HRmax, the bottom of the 90-95% zone. Recovery is 70% HRmax. The original study used running.", hiitReference, "Advanced"),
        ("HIIT 4x4 Rowing", "Rowing. Same 4-minute hard and 3-minute easy pattern as HIIT 4x4. Hard efforts are stored at 90% HRmax, the bottom of the 90-95% zone. Recovery is 70% HRmax. The original study used running.", hiitReference, "Advanced")
    };

    foreach (var item in workouts)
    {
        var workout = context.LKP_WorkoutTemplate.FirstOrDefault(x => x.Name == item.Name);
        if (workout == null)
        {
            workout = new TemplateWorkout { Name = item.Name };
            context.LKP_WorkoutTemplate.Add(workout);
        }

        workout.Notes = item.Notes;
        workout.Reference = item.Reference;
        workout.FitnessLevelId = context.LKP_FitnessLevel.First(x => x.Description == item.Level).Id;
    }
    context.SaveChanges();

    var seededNames = workouts.Select(x => x.Name).ToArray();
    var templateExerciseIds = context.LKP_TemplateExercise
        .AsNoTracking()
        .Where(x => seededNames.Contains(x.WorkoutTemplate.Name))
        .Select(x => x.Id)
        .ToList();

    if (templateExerciseIds.Count > 0)
    {
        context.LKP_TemplateSetMetric.Where(x => templateExerciseIds.Contains(x.TemplateSet.TemplateExerciseId)).ExecuteDelete();
        context.LKP_TemplateSet.Where(x => templateExerciseIds.Contains(x.TemplateExerciseId)).ExecuteDelete();
        context.LKP_TemplateExercise.Where(x => templateExerciseIds.Contains(x.Id)).ExecuteDelete();
    }

    int repsMetric = context.LKP_Metric.First(x => x.Name == "Reps").Id;
    int percent1Rm = context.LKP_Metric.First(x => x.Name == "Percent 1RM").Id;
    int durationMetric = context.LKP_Metric.First(x => x.Name == "Duration").Id;
    int percentHrMax = context.LKP_Metric.First(x => x.Name == "Percent HRmax").Id;

    AddResistanceWorkout(context, "Beginner Full Body", repsMetric, percent1Rm,
        ("Barbell Squat", 2, 10, 65m),
        ("Barbell Bench Press", 2, 10, 65m),
        ("Lat Pulldown", 2, 10, 65m),
        ("Dumbbell Shoulder Press", 2, 10, 65m),
        ("Bent Over Row", 2, 10, 65m),
        ("Deadlift", 2, 8, 65m));
    AddTimedExercise(context, "Beginner Full Body", "Plank", 2, 20, durationMetric);

    AddResistanceWorkout(context, "Push Workout", repsMetric, percent1Rm,
        ("Barbell Bench Press", 3, 8, 75m),
        ("Incline Bench Press", 3, 8, 75m),
        ("Dumbbell Shoulder Press", 3, 8, 75m),
        ("Lateral Raise", 3, 12, 65m),
        ("Tricep Pushdown", 3, 12, 65m));

    AddResistanceWorkout(context, "Pull Workout", repsMetric, percent1Rm,
        ("Bent Over Row", 3, 8, 75m),
        ("Lat Pulldown", 3, 8, 75m),
        ("Seated Row", 3, 10, 70m),
        ("Rear Delt Fly", 3, 12, 65m),
        ("Barbell Curl", 3, 12, 65m));

    AddResistanceWorkout(context, "Leg Workout", repsMetric, percent1Rm,
        ("Barbell Squat", 3, 8, 75m),
        ("Leg Press", 2, 10, 70m),
        ("Lunges", 2, 8, 65m),
        ("Leg Curl", 3, 12, 65m),
        ("Calf Raise", 3, 12, 65m));

    AddHiit(context, "HIIT 4x4", "Running", durationMetric, percentHrMax);
    AddTimedExercise(context, "Moderate Cardio", "Cycling", 1, 3000, durationMetric);

    AddResistanceWorkout(context, "Advanced Push", repsMetric, percent1Rm,
        ("Barbell Bench Press", 4, 8, 75m),
        ("Incline Bench Press", 3, 10, 70m),
        ("Dumbbell Shoulder Press", 3, 8, 75m),
        ("Lateral Raise", 3, 12, 65m),
        ("Tricep Pushdown", 3, 12, 65m),
        ("Skull Crusher", 2, 10, 70m));

    AddResistanceWorkout(context, "Advanced Pull", repsMetric, percent1Rm,
        ("Bent Over Row", 4, 8, 75m),
        ("Lat Pulldown", 3, 8, 75m),
        ("Rear Delt Fly", 3, 12, 65m),
        ("Barbell Curl", 3, 10, 70m),
        ("Hammer Curl", 3, 12, 65m));

    AddResistanceWorkout(context, "Advanced Legs", repsMetric, percent1Rm,
        ("Barbell Squat", 4, 6, 80m),
        ("Leg Press", 3, 10, 70m),
        ("Lunges", 3, 8, 70m),
        ("Leg Curl", 3, 12, 65m),
        ("Calf Raise", 4, 12, 65m));

    AddResistanceWorkout(context, "Strength Upper", repsMetric, percent1Rm,
        ("Barbell Bench Press", 4, 5, 85m),
        ("Bent Over Row", 4, 5, 85m),
        ("Shoulder Press", 4, 5, 85m),
        ("Lat Pulldown", 4, 5, 85m));

    AddResistanceWorkout(context, "Strength Squat", repsMetric, percent1Rm,
        ("Barbell Squat", 4, 5, 85m),
        ("Front Squat", 3, 5, 80m),
        ("Leg Curl", 3, 8, 70m),
        ("Calf Raise", 3, 8, 75m));

    AddResistanceWorkout(context, "Strength Deadlift", repsMetric, percent1Rm,
        ("Deadlift", 4, 3, 85m),
        ("Leg Press", 3, 6, 80m),
        ("Lunges", 3, 6, 75m),
        ("Leg Curl", 3, 8, 70m));

    AddResistanceWorkout(context, "Heavy Full Body", repsMetric, percent1Rm,
        ("Barbell Squat", 4, 5, 85m),
        ("Barbell Bench Press", 4, 5, 85m),
        ("Bent Over Row", 4, 5, 85m));

    AddResistanceWorkout(context, "Hypertrophy Full Body", repsMetric, percent1Rm,
        ("Barbell Squat", 3, 8, 75m),
        ("Barbell Bench Press", 3, 8, 75m),
        ("Lat Pulldown", 3, 8, 75m),
        ("Dumbbell Shoulder Press", 3, 10, 70m),
        ("Leg Curl", 3, 10, 70m));

    AddResistanceWorkout(context, "Power Full Body", repsMetric, percent1Rm,
        ("Barbell Squat", 5, 3, 50m),
        ("Barbell Bench Press", 5, 3, 40m),
        ("Shoulder Press", 5, 3, 40m));

    AddResistanceWorkout(context, "Muscular Endurance", repsMetric, percent1Rm,
        ("Barbell Squat", 3, 15, 50m),
        ("Dumbbell Press", 3, 15, 50m),
        ("Seated Row", 3, 15, 50m),
        ("Lunges", 2, 15, 50m),
        ("Lateral Raise", 2, 15, 40m),
        ("Calf Raise", 2, 15, 50m));
    AddRepsOnlyExercise(context, "Muscular Endurance", "Crunch", 3, 15, repsMetric);

    AddSteadyCardio(context, "Vigorous Run", "Running", 1500, 80m, durationMetric, percentHrMax);
    AddSteadyCardio(context, "Easy Run", "Running", 2700, 65m, durationMetric, percentHrMax);
    AddSteadyCardio(context, "Steady Row", "Rowing Machine", 2400, 70m, durationMetric, percentHrMax);
    AddHiit(context, "HIIT 4x4 Cycling", "Cycling", durationMetric, percentHrMax);
    AddHiit(context, "HIIT 4x4 Rowing", "Rowing Machine", durationMetric, percentHrMax);
}

void AddResistanceWorkout(EntityDBContext context, string workoutName, int repsMetricId, int percentMetricId, params (string Exercise, int Sets, int Reps, decimal Percent1Rm)[] exercises)
{
    int workoutId = context.LKP_WorkoutTemplate.First(x => x.Name == workoutName).Id;
    var templateExercises = exercises.Select(item => new TemplateExercise
    {
        WorkoutTemplateId = workoutId,
        ExerciseId = context.LKP_Exercise.First(x => x.Name == item.Exercise).Id
    }).ToList();
    context.LKP_TemplateExercise.AddRange(templateExercises);
    context.SaveChanges();

    var sets = new List<(TemplateSet Set, int Reps, decimal Percent1Rm)>();
    for (int index = 0; index < exercises.Length; index++)
    {
        for (int order = 1; order <= exercises[index].Sets; order++)
        {
            var set = new TemplateSet
            {
                TemplateExerciseId = templateExercises[index].Id,
                Order = order
            };
            context.LKP_TemplateSet.Add(set);
            sets.Add((set, exercises[index].Reps, exercises[index].Percent1Rm));
        }
    }
    context.SaveChanges();

    context.LKP_TemplateSetMetric.AddRange(sets.SelectMany(item => new[]
    {
        new TemplateSetMetric { TemplateSetId = item.Set.Id, MetricId = repsMetricId, NumericValue = item.Reps },
        new TemplateSetMetric { TemplateSetId = item.Set.Id, MetricId = percentMetricId, NumericValue = item.Percent1Rm }
    }));
    context.SaveChanges();
}

void AddTimedExercise(EntityDBContext context, string workoutName, string exerciseName, int sets, int durationSeconds, int durationMetricId)
{
    int workoutId = context.LKP_WorkoutTemplate.First(x => x.Name == workoutName).Id;
    var templateExercise = new TemplateExercise
    {
        WorkoutTemplateId = workoutId,
        ExerciseId = context.LKP_Exercise.First(x => x.Name == exerciseName).Id
    };
    context.LKP_TemplateExercise.Add(templateExercise);
    context.SaveChanges();

    for (int order = 1; order <= sets; order++)
    {
        var set = new TemplateSet { TemplateExerciseId = templateExercise.Id, Order = order };
        context.LKP_TemplateSet.Add(set);
        context.SaveChanges();
        context.LKP_TemplateSetMetric.Add(new TemplateSetMetric
        {
            TemplateSetId = set.Id,
            MetricId = durationMetricId,
            NumericValue = durationSeconds
        });
    }
    context.SaveChanges();
}

void AddRepsOnlyExercise(EntityDBContext context, string workoutName, string exerciseName, int sets, int reps, int repsMetricId)
{
    int workoutId = context.LKP_WorkoutTemplate.First(x => x.Name == workoutName).Id;
    var templateExercise = new TemplateExercise
    {
        WorkoutTemplateId = workoutId,
        ExerciseId = context.LKP_Exercise.First(x => x.Name == exerciseName).Id
    };
    context.LKP_TemplateExercise.Add(templateExercise);
    context.SaveChanges();

    for (int order = 1; order <= sets; order++)
    {
        var set = new TemplateSet { TemplateExerciseId = templateExercise.Id, Order = order };
        context.LKP_TemplateSet.Add(set);
        context.SaveChanges();
        context.LKP_TemplateSetMetric.Add(new TemplateSetMetric
        {
            TemplateSetId = set.Id,
            MetricId = repsMetricId,
            NumericValue = reps
        });
    }
    context.SaveChanges();
}

void AddSteadyCardio(EntityDBContext context, string workoutName, string exerciseName, int durationSeconds, decimal percentHrMax, int durationMetricId, int percentHrMaxMetricId)
{
    int workoutId = context.LKP_WorkoutTemplate.First(x => x.Name == workoutName).Id;
    var templateExercise = new TemplateExercise
    {
        WorkoutTemplateId = workoutId,
        ExerciseId = context.LKP_Exercise.First(x => x.Name == exerciseName).Id
    };
    context.LKP_TemplateExercise.Add(templateExercise);
    context.SaveChanges();

    var set = new TemplateSet { TemplateExerciseId = templateExercise.Id, Order = 1 };
    context.LKP_TemplateSet.Add(set);
    context.SaveChanges();
    context.LKP_TemplateSetMetric.AddRange(
        new TemplateSetMetric { TemplateSetId = set.Id, MetricId = durationMetricId, NumericValue = durationSeconds },
        new TemplateSetMetric { TemplateSetId = set.Id, MetricId = percentHrMaxMetricId, NumericValue = percentHrMax });
    context.SaveChanges();
}

void AddHiit(EntityDBContext context, string workoutName, string exerciseName, int durationMetricId, int percentHrMaxMetricId)
{
    int workoutId = context.LKP_WorkoutTemplate.First(x => x.Name == workoutName).Id;
    var templateExercise = new TemplateExercise
    {
        WorkoutTemplateId = workoutId,
        ExerciseId = context.LKP_Exercise.First(x => x.Name == exerciseName).Id
    };
    context.LKP_TemplateExercise.Add(templateExercise);
    context.SaveChanges();

    (int Duration, decimal PercentHrMax)[] intervals =
    {
        (240, 90m),
        (180, 70m),
        (240, 90m),
        (180, 70m),
        (240, 90m),
        (180, 70m),
        (240, 90m)
    };

    for (int index = 0; index < intervals.Length; index++)
    {
        var set = new TemplateSet { TemplateExerciseId = templateExercise.Id, Order = index + 1 };
        context.LKP_TemplateSet.Add(set);
        context.SaveChanges();
        context.LKP_TemplateSetMetric.AddRange(
            new TemplateSetMetric { TemplateSetId = set.Id, MetricId = durationMetricId, NumericValue = intervals[index].Duration },
            new TemplateSetMetric { TemplateSetId = set.Id, MetricId = percentHrMaxMetricId, NumericValue = intervals[index].PercentHrMax });
    }
    context.SaveChanges();
}

void SeedTemplateTrainingGoals(EntityDBContext context)
{
    (string Goal, string Template)[] links =
    {
        ("Fat loss", "Moderate Cardio"),
        ("Fat loss", "Easy Run"),
        ("Fat loss", "Muscular Endurance"),
        ("Fat loss", "Vigorous Run"),
        ("Fat loss", "Steady Row"),
        ("Fat loss", "HIIT 4x4"),
        ("Fat loss", "HIIT 4x4 Cycling"),
        ("Fat loss", "HIIT 4x4 Rowing"),
        ("Strength", "Beginner Full Body"),
        ("Strength", "Push Workout"),
        ("Strength", "Pull Workout"),
        ("Strength", "Leg Workout"),
        ("Strength", "Hypertrophy Full Body"),
        ("Strength", "Strength Upper"),
        ("Strength", "Strength Squat"),
        ("Strength", "Strength Deadlift"),
        ("Strength", "Heavy Full Body"),
        ("Strength", "Power Full Body"),
        ("Size", "Beginner Full Body"),
        ("Size", "Push Workout"),
        ("Size", "Pull Workout"),
        ("Size", "Leg Workout"),
        ("Size", "Hypertrophy Full Body"),
        ("Size", "Advanced Push"),
        ("Size", "Advanced Pull"),
        ("Size", "Advanced Legs")
    };

    var goalIds = context.LKP_TrainingGoal.ToDictionary(x => x.Description!, x => x.Id);
    var templateIds = context.LKP_WorkoutTemplate.ToDictionary(x => x.Name, x => x.Id);
    var existing = context.LKP_TemplateTrainingGoal
        .Select(x => new { x.TemplateWorkoutId, x.TrainingGoalId })
        .ToList();

    foreach (var link in links)
    {
        int templateId = templateIds[link.Template];
        int goalId = goalIds[link.Goal];
        if (existing.Any(x => x.TemplateWorkoutId == templateId && x.TrainingGoalId == goalId))
        {
            continue;
        }

        context.LKP_TemplateTrainingGoal.Add(new TemplateTrainingGoal
        {
            TemplateWorkoutId = templateId,
            TrainingGoalId = goalId
        });
    }

    context.SaveChanges();
}

void SeedNoviceSchedule(EntityDBContext context)
{
    if (!context.LKP_WorkoutPeriod.Any() || !context.LKP_WorkoutTemplate.Any(x => x.Name == "Beginner Full Body"))
    {
        return;
    }

    var userIds = context.Database.SqlQueryRaw<string>("SELECT Id FROM AspNetUsers")
        .ToList()
        .Select(Guid.Parse)
        .ToList();
    if (userIds.Count == 0)
    {
        return;
    }

    int morningId = context.LKP_WorkoutPeriod.First(x => x.Description == "Morning").Id;
    (DayOfWeek Day, string Workout)[] plan =
    {
        (DayOfWeek.Monday, "Beginner Full Body"),
        (DayOfWeek.Tuesday, "Moderate Cardio"),
        (DayOfWeek.Wednesday, "Beginner Full Body"),
        (DayOfWeek.Thursday, "Moderate Cardio"),
        (DayOfWeek.Friday, "Beginner Full Body"),
        (DayOfWeek.Saturday, "Moderate Cardio")
    };

    foreach (var userId in userIds)
    {
        context.TRN_DOWWorkout.Where(x => x.UserId == userId).ExecuteDelete();
        for (int index = 0; index < plan.Length; index++)
        {
            context.TRN_DOWWorkout.Add(new DOWWorkout
            {
                UserId = userId,
                DayOfWeek = plan[index].Day,
                WorkoutPeriodId = morningId,
                TemplateWorkoutId = context.LKP_WorkoutTemplate.First(x => x.Name == plan[index].Workout).Id,
                SortOrder = index + 1
            });
        }
    }

    context.SaveChanges();
}

void ApplyMigrations()
{

    using (var db = new EntityDBContext(_optionsBuilder.Options))
    {
        db.Database.Migrate();
    }
}

void BuildOptions()
{
    _configuration = ConfigurationBuilderSingleton.ConfigurationRoot;
    _optionsBuilder = new DbContextOptionsBuilder<EntityDBContext>();
    _optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
}