using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGamificationPlanPaymentAndSpecialClassIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_special_class",
                table: "gym_classes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "membership_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    membership_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_membership_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount_paid = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_paid_on_time = table.Column<bool>(type: "boolean", nullable: false),
                    recorded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_membership_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_membership_payments_membership_plans_membership_plan_id",
                        column: x => x.membership_plan_id,
                        principalTable: "membership_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_membership_payments_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_membership_payments_user_memberships_user_membership_id",
                        column: x => x.user_membership_id,
                        principalTable: "user_memberships",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_membership_payments_users_recorded_by_user_id",
                        column: x => x.recorded_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_membership_payments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_membership_payments_membership_plan_id",
                table: "membership_payments",
                column: "membership_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_membership_payments_recorded_by_user_id",
                table: "membership_payments",
                column: "recorded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_membership_payments_tenant_id",
                table: "membership_payments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_membership_payments_tenant_id_user_id_due_date",
                table: "membership_payments",
                columns: new[] { "tenant_id", "user_id", "due_date" });

            migrationBuilder.CreateIndex(
                name: "IX_membership_payments_user_id",
                table: "membership_payments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_membership_payments_user_membership_id",
                table: "membership_payments",
                column: "user_membership_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "membership_payments");

            migrationBuilder.DropColumn(
                name: "is_special_class",
                table: "gym_classes");
        }
    }
}
