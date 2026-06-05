using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTrackerDataModel.Migrations
{
    /// <inheritdoc />
    public partial class initialClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LKP_ExerciseType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LKP_ExerciseType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LKP_Metric",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ValueType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LKP_Metric", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LKP_WorkoutPeriod",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LKP_WorkoutPeriod", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LKP_WorkoutTemplate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LKP_WorkoutTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LKP_Exercise",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ExerciseTypeId = table.Column<int>(type: "int", nullable: false),
                    StepsToPerform = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LKP_Exercise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LKP_Exercise_LKP_ExerciseType_ExerciseTypeId",
                        column: x => x.ExerciseTypeId,
                        principalTable: "LKP_ExerciseType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TRN_DOWWorkout",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    WorkoutPeriodId = table.Column<int>(type: "int", nullable: false),
                    TemplateWorkoutId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRN_DOWWorkout", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TRN_DOWWorkout_LKP_WorkoutPeriod_WorkoutPeriodId",
                        column: x => x.WorkoutPeriodId,
                        principalTable: "LKP_WorkoutPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TRN_DOWWorkout_LKP_WorkoutTemplate_TemplateWorkoutId",
                        column: x => x.TemplateWorkoutId,
                        principalTable: "LKP_WorkoutTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LKP_ExerciseMetric",
                columns: table => new
                {
                    ExerciseId = table.Column<int>(type: "int", nullable: false),
                    MetricId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LKP_ExerciseMetric", x => new { x.ExerciseId, x.MetricId });
                    table.ForeignKey(
                        name: "FK_LKP_ExerciseMetric_LKP_Exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "LKP_Exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LKP_ExerciseMetric_LKP_Metric_MetricId",
                        column: x => x.MetricId,
                        principalTable: "LKP_Metric",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LKP_TemplateExercise",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkoutTemplateId = table.Column<int>(type: "int", nullable: false),
                    ExerciseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LKP_TemplateExercise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LKP_TemplateExercise_LKP_Exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "LKP_Exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LKP_TemplateExercise_LKP_WorkoutTemplate_WorkoutTemplateId",
                        column: x => x.WorkoutTemplateId,
                        principalTable: "LKP_WorkoutTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TRN_ActualWorkout",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerformedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DOWWorkoutId = table.Column<int>(type: "int", nullable: false),
                    WorkoutStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkoutStartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    WorkoutEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkoutEndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRN_ActualWorkout", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TRN_ActualWorkout_TRN_DOWWorkout_DOWWorkoutId",
                        column: x => x.DOWWorkoutId,
                        principalTable: "TRN_DOWWorkout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LKP_TemplateSet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateExerciseId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LKP_TemplateSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LKP_TemplateSet_LKP_TemplateExercise_TemplateExerciseId",
                        column: x => x.TemplateExerciseId,
                        principalTable: "LKP_TemplateExercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TRN_ActualExercise",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActualWorkoutId = table.Column<int>(type: "int", nullable: false),
                    TemplateExerciseId = table.Column<int>(type: "int", nullable: false),
                    ActualTemplateExerciseId = table.Column<int>(type: "int", nullable: false),
                    Createdatetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRN_ActualExercise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TRN_ActualExercise_LKP_TemplateExercise_ActualTemplateExerciseId",
                        column: x => x.ActualTemplateExerciseId,
                        principalTable: "LKP_TemplateExercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN_ActualExercise_LKP_TemplateExercise_TemplateExerciseId",
                        column: x => x.TemplateExerciseId,
                        principalTable: "LKP_TemplateExercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN_ActualExercise_TRN_ActualWorkout_ActualWorkoutId",
                        column: x => x.ActualWorkoutId,
                        principalTable: "TRN_ActualWorkout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LKP_TemplateSetMetric",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateSetId = table.Column<int>(type: "int", nullable: false),
                    MetricId = table.Column<int>(type: "int", nullable: false),
                    NumericValue = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    TextValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LKP_TemplateSetMetric", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LKP_TemplateSetMetric_LKP_Metric_MetricId",
                        column: x => x.MetricId,
                        principalTable: "LKP_Metric",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LKP_TemplateSetMetric_LKP_TemplateSet_TemplateSetId",
                        column: x => x.TemplateSetId,
                        principalTable: "LKP_TemplateSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TRN_ActualSet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActualExerciseId = table.Column<int>(type: "int", nullable: false),
                    TemplateSetId = table.Column<int>(type: "int", nullable: false),
                    ExecutionOrder = table.Column<int>(type: "int", nullable: false),
                    Createdatetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRN_ActualSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TRN_ActualSet_LKP_TemplateSet_TemplateSetId",
                        column: x => x.TemplateSetId,
                        principalTable: "LKP_TemplateSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN_ActualSet_TRN_ActualExercise_ActualExerciseId",
                        column: x => x.ActualExerciseId,
                        principalTable: "TRN_ActualExercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TRN_ActualSetMetric",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActualSetId = table.Column<int>(type: "int", nullable: false),
                    TemplateSetMetricId = table.Column<int>(type: "int", nullable: false),
                    PlannedTextValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PlannedNumericValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualTextValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ActualNumericValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Createdatetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRN_ActualSetMetric", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TRN_ActualSetMetric_LKP_TemplateSetMetric_TemplateSetMetricId",
                        column: x => x.TemplateSetMetricId,
                        principalTable: "LKP_TemplateSetMetric",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN_ActualSetMetric_TRN_ActualSet_ActualSetId",
                        column: x => x.ActualSetId,
                        principalTable: "TRN_ActualSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LKP_Exercise_ExerciseTypeId",
                table: "LKP_Exercise",
                column: "ExerciseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LKP_ExerciseMetric_MetricId",
                table: "LKP_ExerciseMetric",
                column: "MetricId");

            migrationBuilder.CreateIndex(
                name: "IX_LKP_TemplateExercise_ExerciseId",
                table: "LKP_TemplateExercise",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_LKP_TemplateExercise_WorkoutTemplateId_ExerciseId",
                table: "LKP_TemplateExercise",
                columns: new[] { "WorkoutTemplateId", "ExerciseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LKP_TemplateSet_TemplateExerciseId_Order",
                table: "LKP_TemplateSet",
                columns: new[] { "TemplateExerciseId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LKP_TemplateSetMetric_MetricId",
                table: "LKP_TemplateSetMetric",
                column: "MetricId");

            migrationBuilder.CreateIndex(
                name: "IX_LKP_TemplateSetMetric_TemplateSetId",
                table: "LKP_TemplateSetMetric",
                column: "TemplateSetId");

            migrationBuilder.CreateIndex(
                name: "IX_TRN_ActualExercise_ActualTemplateExerciseId",
                table: "TRN_ActualExercise",
                column: "ActualTemplateExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_TRN_ActualExercise_ActualWorkoutId",
                table: "TRN_ActualExercise",
                column: "ActualWorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_TRN_ActualExercise_TemplateExerciseId",
                table: "TRN_ActualExercise",
                column: "TemplateExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_TRN_ActualSet_ActualExerciseId",
                table: "TRN_ActualSet",
                column: "ActualExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_TRN_ActualSet_TemplateSetId",
                table: "TRN_ActualSet",
                column: "TemplateSetId");

            migrationBuilder.CreateIndex(
                name: "IX_TRN_ActualSetMetric_ActualSetId",
                table: "TRN_ActualSetMetric",
                column: "ActualSetId");

            migrationBuilder.CreateIndex(
                name: "IX_TRN_ActualSetMetric_TemplateSetMetricId",
                table: "TRN_ActualSetMetric",
                column: "TemplateSetMetricId");

            migrationBuilder.CreateIndex(
                name: "IX_TRN_ActualWorkout_DOWWorkoutId",
                table: "TRN_ActualWorkout",
                column: "DOWWorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_TRN_DOWWorkout_TemplateWorkoutId",
                table: "TRN_DOWWorkout",
                column: "TemplateWorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_TRN_DOWWorkout_WorkoutPeriodId",
                table: "TRN_DOWWorkout",
                column: "WorkoutPeriodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LKP_ExerciseMetric");

            migrationBuilder.DropTable(
                name: "TRN_ActualSetMetric");

            migrationBuilder.DropTable(
                name: "LKP_TemplateSetMetric");

            migrationBuilder.DropTable(
                name: "TRN_ActualSet");

            migrationBuilder.DropTable(
                name: "LKP_Metric");

            migrationBuilder.DropTable(
                name: "LKP_TemplateSet");

            migrationBuilder.DropTable(
                name: "TRN_ActualExercise");

            migrationBuilder.DropTable(
                name: "LKP_TemplateExercise");

            migrationBuilder.DropTable(
                name: "TRN_ActualWorkout");

            migrationBuilder.DropTable(
                name: "LKP_Exercise");

            migrationBuilder.DropTable(
                name: "TRN_DOWWorkout");

            migrationBuilder.DropTable(
                name: "LKP_ExerciseType");

            migrationBuilder.DropTable(
                name: "LKP_WorkoutPeriod");

            migrationBuilder.DropTable(
                name: "LKP_WorkoutTemplate");
        }
    }
}
