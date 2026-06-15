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
        new Metric { Name = "Heart Rate", Unit = "bpm", ValueType = MetricValueType.Numeric });
        context.SaveChanges();
    }
    if (!context.LKP_WorkoutTemplate.Any())
    {
        var now = DateTime.Now;
        var workouts = context.LKP_WorkoutTemplate;
        workouts.AddRange(

        new TemplateWorkout
        {
            Name = "Beginner Full Body",
            Notes = "Full body workout 3x per week",
        },

        new TemplateWorkout
        {
            Name = "Push Workout",
            Notes = "Chest, shoulders, triceps",
        },

        new TemplateWorkout
        {
            Name = "Pull Workout",
            Notes = "Back and biceps",

        },

        new TemplateWorkout
        {
            Name = "Leg Workout",
            Notes = "Lower body strength",
        }
        );

        context.SaveChanges();

    }
    int Bench = context.LKP_Exercise.First(x => x.Name == "Barbell Bench Press").Id;
    int Squat = context.LKP_Exercise.First(x => x.Name == "Barbell Squat").Id;
    int Deadlift = context.LKP_Exercise.First(x => x.Name == "Deadlift").Id;
    int OHP = context.LKP_Exercise.First(x => x.Name == "Dumbbell Shoulder Press").Id;
    int Row = context.LKP_Exercise.First(x => x.Name == "Bent Over Row").Id;
    int Curl = context.LKP_Exercise.First(x => x.Name == "Barbell Curl").Id;
    int Tricep = context.LKP_Exercise.First(x => x.Name == "Tricep Pushdown").Id;
    int Lat = context.LKP_Exercise.First(x => x.Name == "Lat Pulldown").Id;
    int LegPress = context.LKP_Exercise.First(x => x.Name == "Leg Press").Id;

    if (!context.LKP_TemplateExercise.Any())
    {
        var now = DateTime.Now;
        var user = "laurencemaasdorp@gmail.com";

        var fullBody = context.LKP_WorkoutTemplate.First(x => x.Name == "Beginner Full Body").Id;
        var push = context.LKP_WorkoutTemplate.First(x => x.Name == "Push Workout").Id;
        var pull = context.LKP_WorkoutTemplate.First(x => x.Name == "Pull Workout").Id;
        var legs = context.LKP_WorkoutTemplate.First(x => x.Name == "Leg Workout").Id;

        context.LKP_TemplateExercise.AddRange(

        // FULL BODY (Starting Strength style)

        new TemplateExercise { WorkoutTemplateId = fullBody, ExerciseId = Squat },
        new TemplateExercise { WorkoutTemplateId = fullBody, ExerciseId = Bench },
        new TemplateExercise { WorkoutTemplateId = fullBody, ExerciseId = Row },
        new TemplateExercise { WorkoutTemplateId = fullBody, ExerciseId = Deadlift }

        //// PUSH

        //new TemplateExercise { WorkoutTemplateId = push, ExerciseId = Bench },
        //new TemplateExercise { WorkoutTemplateId = push, ExerciseId = OHP },
        //new TemplateExercise { WorkoutTemplateId = push, ExerciseId = Tricep },

        //// PULL

        //new TemplateExercise { WorkoutTemplateId = pull, ExerciseId = Row },
        //new TemplateExercise { WorkoutTemplateId = pull, ExerciseId = Lat },
        //new TemplateExercise { WorkoutTemplateId = pull, ExerciseId = Curl },

        //// LEGS

        //new TemplateExercise { WorkoutTemplateId = legs, ExerciseId = Squat },
        //new TemplateExercise { WorkoutTemplateId = legs, ExerciseId = LegPress },
        //new TemplateExercise { WorkoutTemplateId = legs, ExerciseId = Deadlift }

        );
        context.SaveChanges();

    }
    if (!context.LKP_TemplateSet.Any())
    {

        var workoutIds = context.LKP_WorkoutTemplate.ToList().Select(x => x.Id);
        var exerciseTemplate = context.LKP_TemplateExercise.ToList();
        int i;
        List<TemplateSet> test = new List<TemplateSet>();

        foreach (var workoutTemp in workoutIds)
        {
                      

            foreach (var ex in exerciseTemplate.FindAll(x => x.WorkoutTemplateId == workoutTemp))
            {
                i = 1;
                for (int setNo = 1; setNo <= 4; setNo++)
                {
                    context.LKP_TemplateSet.Add(
                        new TemplateSet
                        {
                            TemplateExerciseId = ex.Id,
                            Order = i
                        });

                    i++;
                }

                context.SaveChanges();
            }

        }

    }
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

    if (!context.LKP_TemplateSetMetric.Any())
    {
        int repsMetric = context.LKP_Metric.First(x => x.Name == "Reps").Id;
        int weightMetric = context.LKP_Metric.First(x => x.Name == "Percentage Body Weight").Id;

        var templateSets = context.LKP_TemplateSet
            .Include(x => x.TemplateExercise)
            .ThenInclude(x => x.Exercise)
            .OrderBy(x => x.TemplateExerciseId)
            .ThenBy(x => x.Order)
            .ToList();

        foreach (var set in templateSets)
        {
            int reps = set.Order switch
            {
                1 => 12,
                2 => 10,
                3 => 8,
                4 => 6,
                _ => 10
            };

            decimal weight = set.Order switch
            {
                1 => 40,
                2 => 50,
                3 => 60,
                4 => 70,
                _ => 40
            };

            context.LKP_TemplateSetMetric.AddRange(

                new TemplateSetMetric
                {
                    TemplateSetId = set.Id,
                    MetricId = repsMetric,
                    NumericValue = reps
                },

                new TemplateSetMetric
                {
                    TemplateSetId = set.Id,
                    MetricId = weightMetric,
                    NumericValue = weight
                }
            );
        }

        context.SaveChanges();
    }

    if (!context.LKP_WorkoutPeriod.Any(d => d.Id == 1))
    {
        DbSet<WorkoutPeriod> workoutPeriod = context.LKP_WorkoutPeriod;
        workoutPeriod.AddRange(
        new WorkoutPeriod { Description = "Morning",StartTime = new TimeSpan(5,0,0), EndTime = new TimeSpan(12,0,0) },
        new WorkoutPeriod { Description = "Afternoon" ,StartTime = new TimeSpan(12, 1,0), EndTime = new TimeSpan(18, 0,0) },
        new WorkoutPeriod { Description = "Evening", StartTime = new TimeSpan(18, 1,0), EndTime = new TimeSpan(4, 59, 0) }
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