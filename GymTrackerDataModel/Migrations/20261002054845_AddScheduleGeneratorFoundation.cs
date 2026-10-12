using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GymTrackerDataModel.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleGeneratorFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxSessionsPerWeek",
                table: "LKP_FitnessLevel",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinSessionsPerWeek",
                table: "LKP_FitnessLevel",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LKP_TrainingGoal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LKP_TrainingGoal", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TRN_ScheduleProposal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FitnessLevelId = table.Column<int>(type: "int", nullable: false),
                    RecommendedSessionCount = table.Column<int>(type: "int", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AbandonedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRN_ScheduleProposal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TRN_ScheduleProposal_LKP_FitnessLevel_FitnessLevelId",
                        column: x => x.FitnessLevelId,
                        principalTable: "LKP_FitnessLevel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "USR_TrainingPreference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FitnessLevelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USR_TrainingPreference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_USR_TrainingPreference_LKP_FitnessLevel_FitnessLevelId",
                        column: x => x.FitnessLevelId,
                        principalTable: "LKP_FitnessLevel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LKP_TemplateTrainingGoal",
                columns: table => new
                {
                    TemplateWorkoutId = table.Column<int>(type: "int", nullable: false),
                    TrainingGoalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LKP_TemplateTrainingGoal", x => new { x.TemplateWorkoutId, x.TrainingGoalId });
                    table.ForeignKey(
                        name: "FK_LKP_TemplateTrainingGoal_LKP_TrainingGoal_TrainingGoalId",
                        column: x => x.TrainingGoalId,
                        principalTable: "LKP_TrainingGoal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LKP_TemplateTrainingGoal_LKP_WorkoutTemplate_TemplateWorkoutId",
                        column: x => x.TemplateWorkoutId,
                        principalTable: "LKP_WorkoutTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TRN_ScheduleProposalGoal",
                columns: table => new
                {
                    ScheduleProposalId = table.Column<int>(type: "int", nullable: false),
                    TrainingGoalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRN_ScheduleProposalGoal", x => new { x.ScheduleProposalId, x.TrainingGoalId });
                    table.ForeignKey(
                        name: "FK_TRN_ScheduleProposalGoal_LKP_TrainingGoal_TrainingGoalId",
                        column: x => x.TrainingGoalId,
                        principalTable: "LKP_TrainingGoal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN_ScheduleProposalGoal_TRN_ScheduleProposal_ScheduleProposalId",
                        column: x => x.ScheduleProposalId,
                        principalTable: "TRN_ScheduleProposal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TRN_ScheduleProposalSlot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleProposalId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    WorkoutPeriodId = table.Column<int>(type: "int", nullable: false),
                    TemplateWorkoutId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRN_ScheduleProposalSlot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TRN_ScheduleProposalSlot_LKP_WorkoutPeriod_WorkoutPeriodId",
                        column: x => x.WorkoutPeriodId,
                        principalTable: "LKP_WorkoutPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN_ScheduleProposalSlot_LKP_WorkoutTemplate_TemplateWorkoutId",
                        column: x => x.TemplateWorkoutId,
                        principalTable: "LKP_WorkoutTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN_ScheduleProposalSlot_TRN_ScheduleProposal_ScheduleProposalId",
                        column: x => x.ScheduleProposalId,
                        principalTable: "TRN_ScheduleProposal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USR_TrainingPreferenceGoal",
                columns: table => new
                {
                    UserTrainingPreferenceId = table.Column<int>(type: "int", nullable: false),
                    TrainingGoalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USR_TrainingPreferenceGoal", x => new { x.UserTrainingPreferenceId, x.TrainingGoalId });
                    table.ForeignKey(
                        name: "FK_USR_TrainingPreferenceGoal_LKP_TrainingGoal_TrainingGoalId",
                        column: x => x.TrainingGoalId,
                        principalTable: "LKP_TrainingGoal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_USR_TrainingPreferenceGoal_USR_TrainingPreference_UserTrainingPreferenceId",
                        column: x => x.UserTrainingPreferenceId,
                        principalTable: "USR_TrainingPreference",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "LKP_FitnessLevel",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MaxSessionsPerWeek", "MinSessionsPerWeek" },
                values: new object[] { 3, 2 });

            migrationBuilder.UpdateData(
                table: "LKP_FitnessLevel",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MaxSessionsPerWeek", "MinSessionsPerWeek" },
                values: new object[] { 4, 3 });

            migrationBuilder.UpdateData(
                table: "LKP_FitnessLevel",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "MaxSessionsPerWeek", "MinSessionsPerWeek" },
                values: new object[] { 6, 4 });

            migrationBuilder.AlterColumn<int>(
                name: "MaxSessionsPerWeek",
                table: "LKP_FitnessLevel",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "MinSessionsPerWeek",
                table: "LKP_FitnessLevel",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.InsertData(
                table: "LKP_TrainingGoal",
                columns: new[] { "Id", "Description" },
                values: new object[,]
                {
                    { 1, "Fat loss" },
                    { 2, "Strength" },
                    { 3, "Size" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LKP_TemplateTrainingGoal_TrainingGoalId",
                table: "LKP_TemplateTrainingGoal",
                column: "TrainingGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_TRN_ScheduleProposal_FitnessLevelId",
                table: "TRN_ScheduleProposal",
                column: "FitnessLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_TRN_ScheduleProposal_UserId",
                table: "TRN_ScheduleProposal",
                column: "UserId",
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_TRN_ScheduleProposalGoal_TrainingGoalId",
                table: "TRN_ScheduleProposalGoal",
                column: "TrainingGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_TRN_ScheduleProposalSlot_ScheduleProposalId_DayOfWeek_WorkoutPeriodId_TemplateWorkoutId",
                table: "TRN_ScheduleProposalSlot",
                columns: new[] { "ScheduleProposalId", "DayOfWeek", "WorkoutPeriodId", "TemplateWorkoutId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TRN_ScheduleProposalSlot_TemplateWorkoutId",
                table: "TRN_ScheduleProposalSlot",
                column: "TemplateWorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_TRN_ScheduleProposalSlot_WorkoutPeriodId",
                table: "TRN_ScheduleProposalSlot",
                column: "WorkoutPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_USR_TrainingPreference_FitnessLevelId",
                table: "USR_TrainingPreference",
                column: "FitnessLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_USR_TrainingPreference_UserId",
                table: "USR_TrainingPreference",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USR_TrainingPreferenceGoal_TrainingGoalId",
                table: "USR_TrainingPreferenceGoal",
                column: "TrainingGoalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LKP_TemplateTrainingGoal");

            migrationBuilder.DropTable(
                name: "TRN_ScheduleProposalGoal");

            migrationBuilder.DropTable(
                name: "TRN_ScheduleProposalSlot");

            migrationBuilder.DropTable(
                name: "USR_TrainingPreferenceGoal");

            migrationBuilder.DropTable(
                name: "TRN_ScheduleProposal");

            migrationBuilder.DropTable(
                name: "LKP_TrainingGoal");

            migrationBuilder.DropTable(
                name: "USR_TrainingPreference");

            migrationBuilder.DropColumn(
                name: "MaxSessionsPerWeek",
                table: "LKP_FitnessLevel");

            migrationBuilder.DropColumn(
                name: "MinSessionsPerWeek",
                table: "LKP_FitnessLevel");
        }
    }
}
