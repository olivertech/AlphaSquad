using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLogoMediaRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LogoMediaId",
                table: "Tenants",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_LogoMediaId",
                table: "Tenants",
                column: "LogoMediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_TenantMedias_LogoMediaId",
                table: "Tenants",
                column: "LogoMediaId",
                principalTable: "TenantMedias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_TenantMedias_LogoMediaId",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_LogoMediaId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "LogoMediaId",
                table: "Tenants");
        }
    }
}
