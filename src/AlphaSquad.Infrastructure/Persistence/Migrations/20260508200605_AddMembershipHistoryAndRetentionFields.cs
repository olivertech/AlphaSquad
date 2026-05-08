using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipHistoryAndRetentionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "changed_by_user_id",
                table: "user_memberships",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status_reason",
                table: "user_memberships",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_memberships_changed_by_user_id",
                table: "user_memberships",
                column: "changed_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_memberships_users_changed_by_user_id",
                table: "user_memberships",
                column: "changed_by_user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_memberships_users_changed_by_user_id",
                table: "user_memberships");

            migrationBuilder.DropIndex(
                name: "IX_user_memberships_changed_by_user_id",
                table: "user_memberships");

            migrationBuilder.DropColumn(
                name: "changed_by_user_id",
                table: "user_memberships");

            migrationBuilder.DropColumn(
                name: "status_reason",
                table: "user_memberships");
        }
    }
}
