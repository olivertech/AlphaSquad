using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMediaTableRelationshipWithTenantTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TenantMedias_TenantId",
                table: "TenantMedias",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_TenantMedias_Tenants_TenantId",
                table: "TenantMedias",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantMedias_Tenants_TenantId",
                table: "TenantMedias");

            migrationBuilder.DropIndex(
                name: "IX_TenantMedias_TenantId",
                table: "TenantMedias");
        }
    }
}
