using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTrackerDataModel.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateWorkoutReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "LKP_WorkoutTemplate",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reference",
                table: "LKP_WorkoutTemplate");
        }
    }
}
