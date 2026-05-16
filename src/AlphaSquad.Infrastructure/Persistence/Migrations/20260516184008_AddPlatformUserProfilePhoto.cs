using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformUserProfilePhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "profile_photo_storage_key",
                table: "platform_users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profile_photo_url",
                table: "platform_users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profile_photo_storage_key",
                table: "platform_users");

            migrationBuilder.DropColumn(
                name: "profile_photo_url",
                table: "platform_users");
        }
    }
}
