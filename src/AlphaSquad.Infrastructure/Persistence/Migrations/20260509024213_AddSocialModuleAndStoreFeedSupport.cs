using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialModuleAndStoreFeedSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "social_posts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    media_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_posts", x => x.id);
                    table.ForeignKey(
                        name: "FK_social_posts_tenant_medias_media_id",
                        column: x => x.media_id,
                        principalTable: "tenant_medias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_posts_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_posts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "social_post_comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    social_post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_post_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_social_post_comments_social_posts_social_post_id",
                        column: x => x.social_post_id,
                        principalTable: "social_posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_social_post_comments_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_post_comments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "social_post_likes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    social_post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_social_post_likes", x => x.id);
                    table.ForeignKey(
                        name: "FK_social_post_likes_social_posts_social_post_id",
                        column: x => x.social_post_id,
                        principalTable: "social_posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_social_post_likes_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_social_post_likes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_social_post_comments_social_post_id",
                table: "social_post_comments",
                column: "social_post_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_post_comments_tenant_id",
                table: "social_post_comments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_post_comments_tenant_id_social_post_id_created_at",
                table: "social_post_comments",
                columns: new[] { "tenant_id", "social_post_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_social_post_comments_user_id",
                table: "social_post_comments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_post_likes_social_post_id",
                table: "social_post_likes",
                column: "social_post_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_post_likes_tenant_id",
                table: "social_post_likes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_post_likes_tenant_id_social_post_id_user_id",
                table: "social_post_likes",
                columns: new[] { "tenant_id", "social_post_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_social_post_likes_user_id",
                table: "social_post_likes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_posts_media_id",
                table: "social_posts",
                column: "media_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_posts_tenant_id",
                table: "social_posts",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_posts_tenant_id_is_active_created_at",
                table: "social_posts",
                columns: new[] { "tenant_id", "is_active", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_social_posts_user_id",
                table: "social_posts",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "social_post_comments");

            migrationBuilder.DropTable(
                name: "social_post_likes");

            migrationBuilder.DropTable(
                name: "social_posts");
        }
    }
}
