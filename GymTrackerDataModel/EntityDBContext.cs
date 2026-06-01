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
      //  public DbSet<UserWorkoutSchedule> TRN_UserWorkoutSchedule { get; set; }
        public DbSet<WorkoutPeriod> LKP_WorkoutPeriod { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Exercise>().ToTable("LKP_Exercise");

            modelBuilder.Entity<Exercise>()
            .HasOne(x => x.ExerciseType)
            .WithMany()
            .HasForeignKey(x => x.ExerciseTypeId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExerciseType>().ToTable("LKP_ExerciseType");

            modelBuilder.Entity<TemplateExercise>().ToTable("LKP_TemplateExercise");

            modelBuilder.Entity<TemplateExercise>()
            .HasOne(x => x.Exercise)
            .WithMany()
            .HasForeignKey(x => x.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

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

            //modelBuilder.Entity<TemplateSetMetric>()
            //.HasIndex(x => new { x.TemplateSetId, x.MetricId })
            //.IsUnique();

            modelBuilder.Entity<TemplateWorkout>().ToTable("LKP_WorkoutTemplate");

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
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Metric)
                    .WithMany(x => x.ExerciseMetrics)
                    .HasForeignKey(x => x.MetricId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

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
