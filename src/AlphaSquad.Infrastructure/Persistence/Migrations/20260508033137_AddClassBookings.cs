using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClassBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "class_bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gym_class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    booked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_bookings", x => x.id);
                    table.ForeignKey(
                        name: "FK_class_bookings_gym_classes_gym_class_id",
                        column: x => x.gym_class_id,
                        principalTable: "gym_classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_class_bookings_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_class_bookings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_class_bookings_gym_class_id",
                table: "class_bookings",
                column: "gym_class_id");

            migrationBuilder.CreateIndex(
                name: "IX_class_bookings_gym_class_id_user_id",
                table: "class_bookings",
                columns: new[] { "gym_class_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_class_bookings_tenant_id",
                table: "class_bookings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_class_bookings_user_id",
                table: "class_bookings",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "class_bookings");
        }
    }
}
