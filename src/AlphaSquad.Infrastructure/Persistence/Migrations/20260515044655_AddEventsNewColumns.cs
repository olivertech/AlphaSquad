using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEventsNewColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsCompleted",
                table: "academy_events",
                newName: "is_completed");

            migrationBuilder.RenameColumn(
                name: "CheckInPassword",
                table: "academy_events",
                newName: "check_in_password");

            migrationBuilder.RenameColumn(
                name: "IsPresent",
                table: "academy_event_participations",
                newName: "is_present");

            migrationBuilder.AlterColumn<string>(
                name: "check_in_password",
                table: "academy_events",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_completed",
                table: "academy_events",
                newName: "IsCompleted");

            migrationBuilder.RenameColumn(
                name: "check_in_password",
                table: "academy_events",
                newName: "CheckInPassword");

            migrationBuilder.RenameColumn(
                name: "is_present",
                table: "academy_event_participations",
                newName: "IsPresent");

            migrationBuilder.AlterColumn<string>(
                name: "CheckInPassword",
                table: "academy_events",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
