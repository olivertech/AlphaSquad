using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserMembershipBillingDueDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "billing_due_day",
                table: "user_memberships",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "billing_due_day",
                table: "user_memberships");
        }
    }
}
