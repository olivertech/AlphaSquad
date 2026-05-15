using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePhoneAndBirthDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "birth_date",
                table: "user_profiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                table: "user_profiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "birth_date",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "phone_number",
                table: "user_profiles");
        }
    }
}
