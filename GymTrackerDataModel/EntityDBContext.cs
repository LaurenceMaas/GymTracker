using GymTrackerDataModel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GymTrackerDataModel
{
    public class EntityDBContext : DbContext
    {
        private static IConfigurationRoot? _configuration;
        public EntityDBContext(DbContextOptions<EntityDBContext> options) : base(options)
        {

        }
        public DbSet<Exercise> LKP_Exercise { get; set; }
        public DbSet<ExerciseType> LKP_ExerciseType { get; set; }
        public DbSet<TemplateExercise> LKP_TemplateExercise { get; set; }
        public DbSet<TemplateSet> LKP_TemplateSet { get; set; }
        public DbSet<TemplateWorkout> LKP_WorkoutTemplate { get; set; }
        public DbSet<TemplateSetMetric> LKP_TemplateSetMetric { get; set; }
        public DbSet<Metric> LKP_Metric { get; set; }
        public DbSet<ExerciseMetric> LKP_ExerciseMetric { get; set; }
        public DbSet<DOWWorkout> TRN_DOWWorkout { get; set; }
        public DbSet<WorkoutPeriod> LKP_WorkoutPeriod { get; set; }
        public DbSet<ActualWorkout> TRN_ActualWorkout { get; set; }
        public DbSet<ActualExercise> TRN_ActualExercise { get; set; }
        public DbSet<ActualSet> TRN_ActualSet { get; set; }
        public DbSet<ActualSetMetric> TRN_ActualSetMetric { get; set; }
        public DbSet<WorkOutStatus> LKP_WorkOutStatus { get; set; }
        public DbSet<FitnessLevel> LKP_FitnessLevel { get; set; }
        public DbSet<TrainingGoal> LKP_TrainingGoal { get; set; }
        public DbSet<TemplateTrainingGoal> LKP_TemplateTrainingGoal { get; set; }
        public DbSet<UserTrainingPreference> USR_TrainingPreference { get; set; }
        public DbSet<UserTrainingPreferenceGoal> USR_TrainingPreferenceGoal { get; set; }
        public DbSet<ScheduleProposal> TRN_ScheduleProposal { get; set; }
        public DbSet<ScheduleProposalGoal> TRN_ScheduleProposalGoal { get; set; }
        public DbSet<ScheduleProposalSlot> TRN_ScheduleProposalSlot { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Exercise>().ToTable("LKP_Exercise");

            modelBuilder.Entity<Exercise>()
            .HasOne(x => x.ExerciseType)
            .WithMany()
            .HasForeignKey(x => x.ExerciseTypeId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExerciseType>().ToTable("LKP_ExerciseType");

            modelBuilder.Entity<TemplateExercise>().ToTable("LKP_TemplateExercise");

            modelBuilder.Entity<TemplateExercise>()
            .HasOne(x => x.Exercise)
            .WithMany()
            .HasForeignKey(x => x.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TemplateSet>().ToTable("LKP_TemplateSet")
            .HasOne(x => x.TemplateExercise)
            .WithMany(te => te.TemplateSets)
            .HasForeignKey(ts => ts.TemplateExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TemplateSet>()
            .HasIndex(ts => new { ts.TemplateExerciseId, ts.Order })
            .IsUnique();

            modelBuilder.Entity<TemplateSetMetric>().ToTable("LKP_TemplateSetMetric")
            .HasOne(m => m.TemplateSet)
            .WithMany(s => s.TemplateSetMetrics)
            .HasForeignKey(m => m.TemplateSetId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TemplateSetMetric>()
            .HasOne(tsm => tsm.Metric)
            .WithMany(m => m.TemplateSetMetrics)
            .HasForeignKey(tsm => tsm.MetricId)
            .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<FitnessLevel>().ToTable("LKP_FitnessLevel");
            modelBuilder.Entity<FitnessLevel>().HasData(
                new FitnessLevel
                {
                    Id = 1,
                    Description = "Novice",
                    Qualification = "No resistance-training experience, or none for several years.",
                    StrengthLoading = "8-12 reps at about 60-70% of 1RM. Start with 1-3 sets.",
                    WeeklyTraining = "2-3 days, full body.",
                    MinSessionsPerWeek = 2,
                    MaxSessionsPerWeek = 3,
                    Reference = "https://pubmed.ncbi.nlm.nih.gov/19204579/"
                },
                new FitnessLevel
                {
                    Id = 2,
                    Description = "Intermediate",
                    Qualification = "About 6 months of consistent resistance training.",
                    StrengthLoading = "A wider 1-12 rep range, varied over time. For muscle growth, about 70-85% of 1RM for 8-12 reps.",
                    WeeklyTraining = "3-4 days. Three for full body, or four on a split so each muscle is trained twice.",
                    MinSessionsPerWeek = 3,
                    MaxSessionsPerWeek = 4,
                    Reference = "https://pubmed.ncbi.nlm.nih.gov/19204579/"
                },
                new FitnessLevel
                {
                    Id = 3,
                    Description = "Advanced",
                    Qualification = "Years of consistent resistance training.",
                    StrengthLoading = "For strength, cycle 80-100% of 1RM, with most heavy work in the 1-6 rep range. For muscle, 3-6 sets, mostly 6-12 reps, and some heavier work.",
                    WeeklyTraining = "4-6 days.",
                    MinSessionsPerWeek = 4,
                    MaxSessionsPerWeek = 6,
                    Reference = "https://pubmed.ncbi.nlm.nih.gov/19204579/"
                });

            modelBuilder.Entity<TrainingGoal>().ToTable("LKP_TrainingGoal");
            modelBuilder.Entity<TrainingGoal>().HasData(
                new TrainingGoal { Id = 1, Description = "Fat loss" },
                new TrainingGoal { Id = 2, Description = "Strength" },
                new TrainingGoal { Id = 3, Description = "Size" });

            modelBuilder.Entity<UserTrainingPreference>().ToTable("USR_TrainingPreference");
            modelBuilder.Entity<UserTrainingPreference>()
                .HasIndex(x => x.UserId)
                .IsUnique();
            modelBuilder.Entity<UserTrainingPreference>()
                .HasOne(x => x.FitnessLevel)
                .WithMany()
                .HasForeignKey(x => x.FitnessLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserTrainingPreferenceGoal>().ToTable("USR_TrainingPreferenceGoal");
            modelBuilder.Entity<UserTrainingPreferenceGoal>()
                .HasKey(x => new { x.UserTrainingPreferenceId, x.TrainingGoalId });
            modelBuilder.Entity<UserTrainingPreferenceGoal>()
                .HasOne(x => x.UserTrainingPreference)
                .WithMany(x => x.Goals)
                .HasForeignKey(x => x.UserTrainingPreferenceId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserTrainingPreferenceGoal>()
                .HasOne(x => x.TrainingGoal)
                .WithMany()
                .HasForeignKey(x => x.TrainingGoalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TemplateTrainingGoal>().ToTable("LKP_TemplateTrainingGoal");
            modelBuilder.Entity<TemplateTrainingGoal>()
                .HasKey(x => new { x.TemplateWorkoutId, x.TrainingGoalId });
            modelBuilder.Entity<TemplateTrainingGoal>()
                .HasOne(x => x.TemplateWorkout)
                .WithMany()
                .HasForeignKey(x => x.TemplateWorkoutId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TemplateTrainingGoal>()
                .HasOne(x => x.TrainingGoal)
                .WithMany()
                .HasForeignKey(x => x.TrainingGoalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ScheduleProposal>().ToTable("TRN_ScheduleProposal");
            modelBuilder.Entity<ScheduleProposal>()
                .HasIndex(x => x.UserId)
                .IsUnique()
                .HasFilter("[Status] = 1");
            modelBuilder.Entity<ScheduleProposal>()
                .HasOne(x => x.FitnessLevel)
                .WithMany()
                .HasForeignKey(x => x.FitnessLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ScheduleProposalGoal>().ToTable("TRN_ScheduleProposalGoal");
            modelBuilder.Entity<ScheduleProposalGoal>()
                .HasKey(x => new { x.ScheduleProposalId, x.TrainingGoalId });
            modelBuilder.Entity<ScheduleProposalGoal>()
                .HasOne(x => x.ScheduleProposal)
                .WithMany(x => x.Goals)
                .HasForeignKey(x => x.ScheduleProposalId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ScheduleProposalGoal>()
                .HasOne(x => x.TrainingGoal)
                .WithMany()
                .HasForeignKey(x => x.TrainingGoalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ScheduleProposalSlot>().ToTable("TRN_ScheduleProposalSlot");
            modelBuilder.Entity<ScheduleProposalSlot>()
                .HasIndex(x => new { x.ScheduleProposalId, x.DayOfWeek, x.WorkoutPeriodId, x.TemplateWorkoutId })
                .IsUnique();
            modelBuilder.Entity<ScheduleProposalSlot>()
                .HasOne(x => x.ScheduleProposal)
                .WithMany(x => x.Slots)
                .HasForeignKey(x => x.ScheduleProposalId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ScheduleProposalSlot>()
                .HasOne(x => x.WorkoutPeriod)
                .WithMany()
                .HasForeignKey(x => x.WorkoutPeriodId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ScheduleProposalSlot>()
                .HasOne(x => x.TemplateWorkout)
                .WithMany()
                .HasForeignKey(x => x.TemplateWorkoutId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TemplateWorkout>().ToTable("LKP_WorkoutTemplate");

            modelBuilder.Entity<TemplateWorkout>()
            .HasOne(x => x.FitnessLevel)
            .WithMany()
            .HasForeignKey(x => x.FitnessLevelId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TemplateExercise>()
            .HasOne(x => x.WorkoutTemplate)
            .WithMany(w => w.Exercises)
            .HasForeignKey(x => x.WorkoutTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TemplateExercise>()
            .HasIndex(x => new { x.WorkoutTemplateId, x.ExerciseId })
            .IsUnique();

            modelBuilder.Entity<TemplateSetMetric>()
            .Property(x => x.NumericValue)
            .HasPrecision(10, 2);

            modelBuilder.Entity<ExerciseMetric>(entity =>
            {
                entity.ToTable("LKP_ExerciseMetric");

                entity.HasKey(x => new { x.ExerciseId, x.MetricId });

                entity.HasOne(x => x.Exercise)
                    .WithMany(x => x.ExerciseMetrics)
                    .HasForeignKey(x => x.ExerciseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Metric)
                    .WithMany(x => x.ExerciseMetrics)
                    .HasForeignKey(x => x.MetricId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ActualWorkout>()
            .HasOne(a => a.WorkOutStatus)
            .WithMany()
            .HasForeignKey(a => a.WorkOutStatusId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActualWorkout>()
            .HasOne(a => a.TemplateWorkout)
            .WithMany()
            .HasForeignKey(a => a.TemplateWorkoutId)
            .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ActualExercise>()
            .HasOne(a => a.ActualWorkout)
            .WithMany()
            .HasForeignKey(a => a.ActualWorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActualExercise>()
            .HasOne(a => a.Exercise)
            .WithMany()
            .HasForeignKey(a => a.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ActualSet>()
            .HasOne(a => a.ActualExercise)
            .WithMany()
            .HasForeignKey(a => a.ActualExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActualSet>()
            .HasOne(a => a.TemplateSet)
            .WithMany()
            .HasForeignKey(a => a.TemplateSetId)
            .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ActualSetMetric>()
            .HasOne(a => a.ActualSet)
            .WithMany()
            .HasForeignKey(a => a.ActualSetId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActualSetMetric>()
            .HasOne(a => a.Metric)
            .WithMany()
            .HasForeignKey(a => a.MetricId)
            .OnDelete(DeleteBehavior.Restrict);

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                _configuration = builder.Build();
                var cnstr = _configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(cnstr).LogTo(Console.WriteLine, LogLevel.Information);

            }
        }
    }
}
