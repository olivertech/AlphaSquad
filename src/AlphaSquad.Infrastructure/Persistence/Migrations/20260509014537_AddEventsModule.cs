using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEventsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "academy_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    media_id = table.Column<Guid>(type: "uuid", nullable: true),
                    location = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_outdoor_event = table.Column<bool>(type: "boolean", nullable: false),
                    allow_participation = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_academy_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_academy_events_tenant_medias_media_id",
                        column: x => x.media_id,
                        principalTable: "tenant_medias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_academy_events_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_academy_events_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "academy_event_participations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    academy_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    participated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_academy_event_participations", x => x.id);
                    table.ForeignKey(
                        name: "FK_academy_event_participations_academy_events_academy_event_id",
                        column: x => x.academy_event_id,
                        principalTable: "academy_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_academy_event_participations_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_academy_event_participations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_academy_event_participations_academy_event_id",
                table: "academy_event_participations",
                column: "academy_event_id");

            migrationBuilder.CreateIndex(
                name: "IX_academy_event_participations_tenant_id",
                table: "academy_event_participations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_academy_event_participations_tenant_id_academy_event_id_use~",
                table: "academy_event_participations",
                columns: new[] { "tenant_id", "academy_event_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_academy_event_participations_user_id",
                table: "academy_event_participations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_academy_events_created_by_user_id",
                table: "academy_events",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_academy_events_media_id",
                table: "academy_events",
                column: "media_id");

            migrationBuilder.CreateIndex(
                name: "IX_academy_events_tenant_id",
                table: "academy_events",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_academy_events_tenant_id_is_active_created_at",
                table: "academy_events",
                columns: new[] { "tenant_id", "is_active", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_academy_events_tenant_id_is_outdoor_event_is_active",
                table: "academy_events",
                columns: new[] { "tenant_id", "is_outdoor_event", "is_active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "academy_event_participations");

            migrationBuilder.DropTable(
                name: "academy_events");
        }
    }
}
