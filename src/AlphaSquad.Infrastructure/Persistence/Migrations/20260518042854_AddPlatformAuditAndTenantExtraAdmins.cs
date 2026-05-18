using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformAuditAndTenantExtraAdmins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "platform_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    summary = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    metadata_json = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_platform_audit_logs_platform_users_platform_user_id",
                        column: x => x.platform_user_id,
                        principalTable: "platform_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_platform_audit_logs_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_platform_audit_logs_created_at",
                table: "platform_audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_platform_audit_logs_platform_user_id",
                table: "platform_audit_logs",
                column: "platform_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_platform_audit_logs_tenant_id_created_at",
                table: "platform_audit_logs",
                columns: new[] { "tenant_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_audit_logs");
        }
    }
}
