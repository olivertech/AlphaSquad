using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGamificationBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gamification_event_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    points = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gamification_event_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_gamification_event_rules_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "monthly_student_rankings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    total_points = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    prize_description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    generated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monthly_student_rankings", x => x.id);
                    table.ForeignKey(
                        name: "FK_monthly_student_rankings_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_monthly_student_rankings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_gamification_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gamification_event_rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<int>(type: "integer", nullable: false),
                    source_entity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    source_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    points_applied = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_gamification_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_gamification_events_gamification_event_rules_gamificat~",
                        column: x => x.gamification_event_rule_id,
                        principalTable: "gamification_event_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_gamification_events_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_gamification_events_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "points_ledger",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_gamification_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    points_delta = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    balance_after = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_points_ledger", x => x.id);
                    table.ForeignKey(
                        name: "FK_points_ledger_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_points_ledger_user_gamification_events_user_gamification_ev~",
                        column: x => x.user_gamification_event_id,
                        principalTable: "user_gamification_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_points_ledger_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gamification_event_rules_tenant_id",
                table: "gamification_event_rules",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_gamification_event_rules_tenant_id_event_type",
                table: "gamification_event_rules",
                columns: new[] { "tenant_id", "event_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_monthly_student_rankings_tenant_id",
                table: "monthly_student_rankings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_monthly_student_rankings_tenant_id_year_month_position",
                table: "monthly_student_rankings",
                columns: new[] { "tenant_id", "year", "month", "position" });

            migrationBuilder.CreateIndex(
                name: "IX_monthly_student_rankings_tenant_id_year_month_user_id",
                table: "monthly_student_rankings",
                columns: new[] { "tenant_id", "year", "month", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_monthly_student_rankings_user_id",
                table: "monthly_student_rankings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_points_ledger_tenant_id",
                table: "points_ledger",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_points_ledger_user_gamification_event_id",
                table: "points_ledger",
                column: "user_gamification_event_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_points_ledger_user_id",
                table: "points_ledger",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_gamification_events_gamification_event_rule_id",
                table: "user_gamification_events",
                column: "gamification_event_rule_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_gamification_events_tenant_id",
                table: "user_gamification_events",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_gamification_events_tenant_id_user_id_event_type_sourc~",
                table: "user_gamification_events",
                columns: new[] { "tenant_id", "user_id", "event_type", "source_entity", "source_entity_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_gamification_events_tenant_id_user_id_occurred_at",
                table: "user_gamification_events",
                columns: new[] { "tenant_id", "user_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_user_gamification_events_user_id",
                table: "user_gamification_events",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "monthly_student_rankings");

            migrationBuilder.DropTable(
                name: "points_ledger");

            migrationBuilder.DropTable(
                name: "user_gamification_events");

            migrationBuilder.DropTable(
                name: "gamification_event_rules");
        }
    }
}
