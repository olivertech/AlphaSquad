using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateWorkoutPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_workouts_tenants_tenant_id",
                table: "workouts");

            migrationBuilder.RenameColumn(
                name: "Goal",
                table: "workouts",
                newName: "goal");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "workouts",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "workouts",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "Sets",
                table: "workout_exercises",
                newName: "sets");

            migrationBuilder.RenameColumn(
                name: "Reps",
                table: "workout_exercises",
                newName: "reps");

            migrationBuilder.RenameColumn(
                name: "Order",
                table: "workout_exercises",
                newName: "order");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "workout_exercises",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "RestTime",
                table: "workout_exercises",
                newName: "rest_time");

            migrationBuilder.AddForeignKey(
                name: "FK_workouts_tenants_tenant_id",
                table: "workouts",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_workouts_tenants_tenant_id",
                table: "workouts");

            migrationBuilder.RenameColumn(
                name: "goal",
                table: "workouts",
                newName: "Goal");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "workouts",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "workouts",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "sets",
                table: "workout_exercises",
                newName: "Sets");

            migrationBuilder.RenameColumn(
                name: "reps",
                table: "workout_exercises",
                newName: "Reps");

            migrationBuilder.RenameColumn(
                name: "order",
                table: "workout_exercises",
                newName: "Order");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "workout_exercises",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "rest_time",
                table: "workout_exercises",
                newName: "RestTime");

            migrationBuilder.AddForeignKey(
                name: "FK_workouts_tenants_tenant_id",
                table: "workouts",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
