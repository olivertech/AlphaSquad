using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationsAndInstitutionalEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "event_type",
                table: "academy_events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "highlight_ends_at",
                table: "academy_events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "highlight_starts_at",
                table: "academy_events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_highlighted",
                table: "academy_events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "tenant_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    audience = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    summary = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
                    media_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_highlighted = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    related_entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    related_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenant_notifications_tenant_medias_media_id",
                        column: x => x.media_id,
                        principalTable: "tenant_medias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tenant_notifications_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenant_notifications_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_notification_reads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_notification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_notification_reads", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_notification_reads_tenant_notifications_tenant_notific~",
                        column: x => x.tenant_notification_id,
                        principalTable: "tenant_notifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_notification_reads_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_notification_reads_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_academy_events_tenant_id_event_type_is_active",
                table: "academy_events",
                columns: new[] { "tenant_id", "event_type", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_academy_events_tenant_id_is_highlighted_highlight_starts_at~",
                table: "academy_events",
                columns: new[] { "tenant_id", "is_highlighted", "highlight_starts_at", "highlight_ends_at" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_notifications_created_by_user_id",
                table: "tenant_notifications",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_notifications_media_id",
                table: "tenant_notifications",
                column: "media_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_notifications_tenant_id",
                table: "tenant_notifications",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_notifications_tenant_id_is_active_published_at",
                table: "tenant_notifications",
                columns: new[] { "tenant_id", "is_active", "published_at" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_notifications_tenant_id_type_audience_published_at",
                table: "tenant_notifications",
                columns: new[] { "tenant_id", "type", "audience", "published_at" });

            migrationBuilder.CreateIndex(
                name: "IX_user_notification_reads_tenant_id",
                table: "user_notification_reads",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_notification_reads_tenant_id_tenant_notification_id_us~",
                table: "user_notification_reads",
                columns: new[] { "tenant_id", "tenant_notification_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_notification_reads_tenant_id_user_id_read_at",
                table: "user_notification_reads",
                columns: new[] { "tenant_id", "user_id", "read_at" });

            migrationBuilder.CreateIndex(
                name: "IX_user_notification_reads_tenant_notification_id",
                table: "user_notification_reads",
                column: "tenant_notification_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_notification_reads_user_id",
                table: "user_notification_reads",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_notification_reads");

            migrationBuilder.DropTable(
                name: "tenant_notifications");

            migrationBuilder.DropIndex(
                name: "IX_academy_events_tenant_id_event_type_is_active",
                table: "academy_events");

            migrationBuilder.DropIndex(
                name: "IX_academy_events_tenant_id_is_highlighted_highlight_starts_at~",
                table: "academy_events");

            migrationBuilder.DropColumn(
                name: "event_type",
                table: "academy_events");

            migrationBuilder.DropColumn(
                name: "highlight_ends_at",
                table: "academy_events");

            migrationBuilder.DropColumn(
                name: "highlight_starts_at",
                table: "academy_events");

            migrationBuilder.DropColumn(
                name: "is_highlighted",
                table: "academy_events");
        }
    }
}
