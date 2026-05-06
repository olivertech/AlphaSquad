using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixWorkoutTenantRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_Tenants_TenantId",
                table: "Workouts",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_Tenants_TenantId",
                table: "Workouts");
        }
    }
}
