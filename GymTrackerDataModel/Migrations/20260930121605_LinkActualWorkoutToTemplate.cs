using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTrackerDataModel.Migrations
{
    /// <inheritdoc />
    public partial class LinkActualWorkoutToTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TemplateWorkoutId",
                table: "TRN_ActualWorkout",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TRN_ActualWorkout_TemplateWorkoutId",
                table: "TRN_ActualWorkout",
                column: "TemplateWorkoutId");

            migrationBuilder.AddForeignKey(
                name: "FK_TRN_ActualWorkout_LKP_WorkoutTemplate_TemplateWorkoutId",
                table: "TRN_ActualWorkout",
                column: "TemplateWorkoutId",
                principalTable: "LKP_WorkoutTemplate",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TRN_ActualWorkout_LKP_WorkoutTemplate_TemplateWorkoutId",
                table: "TRN_ActualWorkout");

            migrationBuilder.DropIndex(
                name: "IX_TRN_ActualWorkout_TemplateWorkoutId",
                table: "TRN_ActualWorkout");

            migrationBuilder.DropColumn(
                name: "TemplateWorkoutId",
                table: "TRN_ActualWorkout");
        }
    }
}
