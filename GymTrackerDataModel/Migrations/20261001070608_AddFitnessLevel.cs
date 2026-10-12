using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymTrackerDataModel.Migrations
{
    /// <inheritdoc />
    public partial class AddFitnessLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LKP_FitnessLevel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Qualification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrengthLoading = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeeklyTraining = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LKP_FitnessLevel", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "LKP_FitnessLevel",
                columns: new[] { "Id", "Description", "Qualification", "Reference", "StrengthLoading", "WeeklyTraining" },
                values: new object[,]
                {
                    { 1, "Novice", "No resistance-training experience, or none for several years.", "https://pubmed.ncbi.nlm.nih.gov/19204579/", "8-12 reps at about 60-70% of 1RM. Start with 1-3 sets.", "2-3 days, full body." },
                    { 2, "Intermediate", "About 6 months of consistent resistance training.", "https://pubmed.ncbi.nlm.nih.gov/19204579/", "A wider 1-12 rep range, varied over time. For muscle growth, about 70-85% of 1RM for 8-12 reps.", "3-4 days. Three for full body, or four on a split so each muscle is trained twice." },
                    { 3, "Advanced", "Years of consistent resistance training.", "https://pubmed.ncbi.nlm.nih.gov/19204579/", "For strength, cycle 80-100% of 1RM, with most heavy work in the 1-6 rep range. For muscle, 3-6 sets, mostly 6-12 reps, and some heavier work.", "4-6 days." }
                });

            migrationBuilder.AddColumn<int>(
                name: "FitnessLevelId",
                table: "LKP_WorkoutTemplate",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("""
                UPDATE LKP_WorkoutTemplate SET FitnessLevelId = 2
                WHERE Name IN (
                    'Push Workout',
                    'Pull Workout',
                    'Leg Workout',
                    'Hypertrophy Full Body',
                    'Vigorous Run',
                    'Steady Row');

                UPDATE LKP_WorkoutTemplate SET FitnessLevelId = 3
                WHERE Name IN (
                    'HIIT 4x4',
                    'HIIT 4x4 Cycling',
                    'HIIT 4x4 Rowing',
                    'Advanced Push',
                    'Advanced Pull',
                    'Advanced Legs',
                    'Strength Upper',
                    'Strength Squat',
                    'Strength Deadlift',
                    'Heavy Full Body',
                    'Power Full Body');
                """);

            migrationBuilder.AlterColumn<int>(
                name: "FitnessLevelId",
                table: "LKP_WorkoutTemplate",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_LKP_WorkoutTemplate_FitnessLevelId",
                table: "LKP_WorkoutTemplate",
                column: "FitnessLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_LKP_WorkoutTemplate_LKP_FitnessLevel_FitnessLevelId",
                table: "LKP_WorkoutTemplate",
                column: "FitnessLevelId",
                principalTable: "LKP_FitnessLevel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LKP_WorkoutTemplate_LKP_FitnessLevel_FitnessLevelId",
                table: "LKP_WorkoutTemplate");

            migrationBuilder.DropTable(
                name: "LKP_FitnessLevel");

            migrationBuilder.DropIndex(
                name: "IX_LKP_WorkoutTemplate_FitnessLevelId",
                table: "LKP_WorkoutTemplate");

            migrationBuilder.DropColumn(
                name: "FitnessLevelId",
                table: "LKP_WorkoutTemplate");
        }
    }
}
