using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlphaSquad.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_TenantMedias_MediaId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantFeatures_Features_FeatureId",
                table: "TenantFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantFeatures_Tenants_TenantId",
                table: "TenantFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantMedias_Tenants_TenantId",
                table: "TenantMedias");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_TenantMedias_LogoMediaId",
                table: "Tenants");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutExercises_Exercises_ExerciseId",
                table: "WorkoutExercises");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutExercises_Workouts_WorkoutId",
                table: "WorkoutExercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_Tenants_TenantId",
                table: "Workouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Workouts",
                table: "Workouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tenants",
                table: "Tenants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Features",
                table: "Features");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Exercises",
                table: "Exercises");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkoutExercises",
                table: "WorkoutExercises");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TenantMedias",
                table: "TenantMedias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TenantFeatures",
                table: "TenantFeatures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "Workouts",
                newName: "workouts");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Tenants",
                newName: "tenants");

            migrationBuilder.RenameTable(
                name: "Features",
                newName: "features");

            migrationBuilder.RenameTable(
                name: "Exercises",
                newName: "exercises");

            migrationBuilder.RenameTable(
                name: "WorkoutExercises",
                newName: "workout_exercises");

            migrationBuilder.RenameTable(
                name: "TenantMedias",
                newName: "tenant_medias");

            migrationBuilder.RenameTable(
                name: "TenantFeatures",
                newName: "tenant_features");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "refresh_tokens");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "workouts",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "workouts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "workouts",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "workouts",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Workouts_TenantId",
                table: "workouts",
                newName: "IX_workouts_tenant_id");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "users",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "users",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "users",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "users",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "users",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Users_TenantId_Email",
                table: "users",
                newName: "IX_users_tenant_id_email");

            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "tenants",
                newName: "slug");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "tenants",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "tenants",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SecondaryColor",
                table: "tenants",
                newName: "secondary_color");

            migrationBuilder.RenameColumn(
                name: "PrimaryColor",
                table: "tenants",
                newName: "primary_color");

            migrationBuilder.RenameColumn(
                name: "LogoUrl",
                table: "tenants",
                newName: "logo_url");

            migrationBuilder.RenameColumn(
                name: "LogoMediaId",
                table: "tenants",
                newName: "logo_media_id");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "tenants",
                newName: "is_active");

            migrationBuilder.RenameIndex(
                name: "IX_Tenants_Slug",
                table: "tenants",
                newName: "IX_tenants_slug");

            migrationBuilder.RenameIndex(
                name: "IX_Tenants_LogoMediaId",
                table: "tenants",
                newName: "IX_tenants_logo_media_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "features",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "features",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "features",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "exercises",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "exercises",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "exercises",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "exercises",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "MuscleGroup",
                table: "exercises",
                newName: "muscle_group");

            migrationBuilder.RenameColumn(
                name: "MediaId",
                table: "exercises",
                newName: "media_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "exercises",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Exercises_TenantId",
                table: "exercises",
                newName: "IX_exercises_tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_Exercises_MediaId",
                table: "exercises",
                newName: "IX_exercises_media_id");

            migrationBuilder.RenameColumn(
                name: "ExerciseId",
                table: "workout_exercises",
                newName: "exercise_id");

            migrationBuilder.RenameColumn(
                name: "WorkoutId",
                table: "workout_exercises",
                newName: "workout_id");

            migrationBuilder.RenameIndex(
                name: "IX_WorkoutExercises_ExerciseId",
                table: "workout_exercises",
                newName: "IX_workout_exercises_exercise_id");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "tenant_medias",
                newName: "url");

            migrationBuilder.RenameColumn(
                name: "Size",
                table: "tenant_medias",
                newName: "size");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "tenant_medias",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "tenant_medias",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "StorageKey",
                table: "tenant_medias",
                newName: "storage_key");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "tenant_medias",
                newName: "file_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "tenant_medias",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "tenant_medias",
                newName: "content_type");

            migrationBuilder.RenameIndex(
                name: "IX_TenantMedias_TenantId",
                table: "tenant_medias",
                newName: "IX_tenant_medias_tenant_id");

            migrationBuilder.RenameColumn(
                name: "FeatureId",
                table: "tenant_features",
                newName: "feature_id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "tenant_features",
                newName: "tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_TenantFeatures_FeatureId",
                table: "tenant_features",
                newName: "IX_tenant_features_feature_id");

            migrationBuilder.RenameColumn(
                name: "Token",
                table: "refresh_tokens",
                newName: "token");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "refresh_tokens",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "refresh_tokens",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "IsUsed",
                table: "refresh_tokens",
                newName: "is_used");

            migrationBuilder.RenameColumn(
                name: "IsRevoked",
                table: "refresh_tokens",
                newName: "is_revoked");

            migrationBuilder.RenameColumn(
                name: "ExpiryDate",
                table: "refresh_tokens",
                newName: "expiry_date");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_UserId",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_Token",
                table: "refresh_tokens",
                newName: "IX_refresh_tokens_token");

            migrationBuilder.AddPrimaryKey(
                name: "PK_workouts",
                table: "workouts",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tenants",
                table: "tenants",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_features",
                table: "features",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_exercises",
                table: "exercises",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_workout_exercises",
                table: "workout_exercises",
                columns: new[] { "workout_id", "exercise_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_tenant_medias",
                table: "tenant_medias",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tenant_features",
                table: "tenant_features",
                columns: new[] { "tenant_id", "feature_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_exercises_tenant_medias_media_id",
                table: "exercises",
                column: "media_id",
                principalTable: "tenant_medias",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_users_user_id",
                table: "refresh_tokens",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tenant_features_features_feature_id",
                table: "tenant_features",
                column: "feature_id",
                principalTable: "features",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tenant_features_tenants_tenant_id",
                table: "tenant_features",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tenant_medias_tenants_tenant_id",
                table: "tenant_medias",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tenants_tenant_medias_logo_media_id",
                table: "tenants",
                column: "logo_media_id",
                principalTable: "tenant_medias",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_users_tenants_tenant_id",
                table: "users",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_workout_exercises_exercises_exercise_id",
                table: "workout_exercises",
                column: "exercise_id",
                principalTable: "exercises",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_workout_exercises_workouts_workout_id",
                table: "workout_exercises",
                column: "workout_id",
                principalTable: "workouts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_workouts_tenants_tenant_id",
                table: "workouts",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_exercises_tenant_medias_media_id",
                table: "exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_users_user_id",
                table: "refresh_tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_tenant_features_features_feature_id",
                table: "tenant_features");

            migrationBuilder.DropForeignKey(
                name: "FK_tenant_features_tenants_tenant_id",
                table: "tenant_features");

            migrationBuilder.DropForeignKey(
                name: "FK_tenant_medias_tenants_tenant_id",
                table: "tenant_medias");

            migrationBuilder.DropForeignKey(
                name: "FK_tenants_tenant_medias_logo_media_id",
                table: "tenants");

            migrationBuilder.DropForeignKey(
                name: "FK_users_tenants_tenant_id",
                table: "users");

            migrationBuilder.DropForeignKey(
                name: "FK_workout_exercises_exercises_exercise_id",
                table: "workout_exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_workout_exercises_workouts_workout_id",
                table: "workout_exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_workouts_tenants_tenant_id",
                table: "workouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_workouts",
                table: "workouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tenants",
                table: "tenants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_features",
                table: "features");

            migrationBuilder.DropPrimaryKey(
                name: "PK_exercises",
                table: "exercises");

            migrationBuilder.DropPrimaryKey(
                name: "PK_workout_exercises",
                table: "workout_exercises");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tenant_medias",
                table: "tenant_medias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tenant_features",
                table: "tenant_features");

            migrationBuilder.DropPrimaryKey(
                name: "PK_refresh_tokens",
                table: "refresh_tokens");

            migrationBuilder.RenameTable(
                name: "workouts",
                newName: "Workouts");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "tenants",
                newName: "Tenants");

            migrationBuilder.RenameTable(
                name: "features",
                newName: "Features");

            migrationBuilder.RenameTable(
                name: "exercises",
                newName: "Exercises");

            migrationBuilder.RenameTable(
                name: "workout_exercises",
                newName: "WorkoutExercises");

            migrationBuilder.RenameTable(
                name: "tenant_medias",
                newName: "TenantMedias");

            migrationBuilder.RenameTable(
                name: "tenant_features",
                newName: "TenantFeatures");

            migrationBuilder.RenameTable(
                name: "refresh_tokens",
                newName: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Workouts",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Workouts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "Workouts",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Workouts",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_workouts_tenant_id",
                table: "Workouts",
                newName: "IX_Workouts_TenantId");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "Users",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Users",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "Users",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Users",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_users_tenant_id_email",
                table: "Users",
                newName: "IX_Users_TenantId_Email");

            migrationBuilder.RenameColumn(
                name: "slug",
                table: "Tenants",
                newName: "Slug");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Tenants",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Tenants",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "secondary_color",
                table: "Tenants",
                newName: "SecondaryColor");

            migrationBuilder.RenameColumn(
                name: "primary_color",
                table: "Tenants",
                newName: "PrimaryColor");

            migrationBuilder.RenameColumn(
                name: "logo_url",
                table: "Tenants",
                newName: "LogoUrl");

            migrationBuilder.RenameColumn(
                name: "logo_media_id",
                table: "Tenants",
                newName: "LogoMediaId");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Tenants",
                newName: "IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_tenants_slug",
                table: "Tenants",
                newName: "IX_Tenants_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_tenants_logo_media_id",
                table: "Tenants",
                newName: "IX_Tenants_LogoMediaId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Features",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Features",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Features",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Exercises",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Exercises",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Exercises",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "Exercises",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "muscle_group",
                table: "Exercises",
                newName: "MuscleGroup");

            migrationBuilder.RenameColumn(
                name: "media_id",
                table: "Exercises",
                newName: "MediaId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Exercises",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_exercises_tenant_id",
                table: "Exercises",
                newName: "IX_Exercises_TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_exercises_media_id",
                table: "Exercises",
                newName: "IX_Exercises_MediaId");

            migrationBuilder.RenameColumn(
                name: "exercise_id",
                table: "WorkoutExercises",
                newName: "ExerciseId");

            migrationBuilder.RenameColumn(
                name: "workout_id",
                table: "WorkoutExercises",
                newName: "WorkoutId");

            migrationBuilder.RenameIndex(
                name: "IX_workout_exercises_exercise_id",
                table: "WorkoutExercises",
                newName: "IX_WorkoutExercises_ExerciseId");

            migrationBuilder.RenameColumn(
                name: "url",
                table: "TenantMedias",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "size",
                table: "TenantMedias",
                newName: "Size");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "TenantMedias",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "TenantMedias",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "storage_key",
                table: "TenantMedias",
                newName: "StorageKey");

            migrationBuilder.RenameColumn(
                name: "file_name",
                table: "TenantMedias",
                newName: "FileName");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "TenantMedias",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "content_type",
                table: "TenantMedias",
                newName: "ContentType");

            migrationBuilder.RenameIndex(
                name: "IX_tenant_medias_tenant_id",
                table: "TenantMedias",
                newName: "IX_TenantMedias_TenantId");

            migrationBuilder.RenameColumn(
                name: "feature_id",
                table: "TenantFeatures",
                newName: "FeatureId");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "TenantFeatures",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_tenant_features_feature_id",
                table: "TenantFeatures",
                newName: "IX_TenantFeatures_FeatureId");

            migrationBuilder.RenameColumn(
                name: "token",
                table: "RefreshTokens",
                newName: "Token");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "RefreshTokens",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "RefreshTokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "is_used",
                table: "RefreshTokens",
                newName: "IsUsed");

            migrationBuilder.RenameColumn(
                name: "is_revoked",
                table: "RefreshTokens",
                newName: "IsRevoked");

            migrationBuilder.RenameColumn(
                name: "expiry_date",
                table: "RefreshTokens",
                newName: "ExpiryDate");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_user_id",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_refresh_tokens_token",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_Token");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Workouts",
                table: "Workouts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tenants",
                table: "Tenants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Features",
                table: "Features",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exercises",
                table: "Exercises",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkoutExercises",
                table: "WorkoutExercises",
                columns: new[] { "WorkoutId", "ExerciseId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_TenantMedias",
                table: "TenantMedias",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TenantFeatures",
                table: "TenantFeatures",
                columns: new[] { "TenantId", "FeatureId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_TenantMedias_MediaId",
                table: "Exercises",
                column: "MediaId",
                principalTable: "TenantMedias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantFeatures_Features_FeatureId",
                table: "TenantFeatures",
                column: "FeatureId",
                principalTable: "Features",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantFeatures_Tenants_TenantId",
                table: "TenantFeatures",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantMedias_Tenants_TenantId",
                table: "TenantMedias",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_TenantMedias_LogoMediaId",
                table: "Tenants",
                column: "LogoMediaId",
                principalTable: "TenantMedias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutExercises_Exercises_ExerciseId",
                table: "WorkoutExercises",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutExercises_Workouts_WorkoutId",
                table: "WorkoutExercises",
                column: "WorkoutId",
                principalTable: "Workouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_Tenants_TenantId",
                table: "Workouts",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
