namespace AlphaSquad.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<PlatformUser> PlatformUsers => Set<PlatformUser>();
    public DbSet<PlatformAuditLog> PlatformAuditLogs => Set<PlatformAuditLog>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<PlatformRefreshToken> PlatformRefreshTokens => Set<PlatformRefreshToken>();
    public DbSet<TenantMedia> TenantMedias => Set<TenantMedia>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<TenantFeature> TenantFeatures => Set<TenantFeature>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();
    public DbSet<GymClass> GymClasses => Set<GymClass>();
    public DbSet<ClassBooking> ClassBookings => Set<ClassBooking>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<UserMembership> UserMemberships => Set<UserMembership>();
    public DbSet<MembershipPayment> MembershipPayments => Set<MembershipPayment>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<StoreOrder> StoreOrders => Set<StoreOrder>();
    public DbSet<StoreOrderItem> StoreOrderItems => Set<StoreOrderItem>();
    public DbSet<AcademyEvent> AcademyEvents => Set<AcademyEvent>();
    public DbSet<AcademyEventParticipation> AcademyEventParticipations => Set<AcademyEventParticipation>();
    public DbSet<SocialPost> SocialPosts => Set<SocialPost>();
    public DbSet<SocialPostLike> SocialPostLikes => Set<SocialPostLike>();
    public DbSet<SocialPostComment> SocialPostComments => Set<SocialPostComment>();
    public DbSet<GamificationEventRule> GamificationEventRules => Set<GamificationEventRule>();
    public DbSet<UserGamificationEvent> UserGamificationEvents => Set<UserGamificationEvent>();
    public DbSet<PointsLedger> PointsLedgers => Set<PointsLedger>();
    public DbSet<MonthlyStudentRanking> MonthlyStudentRankings => Set<MonthlyStudentRanking>();
    public DbSet<TenantLegalContent> TenantLegalContents => Set<TenantLegalContent>();
    public DbSet<Configuration> Configurations => Set<Configuration>();
    public DbSet<TenantNotification> TenantNotifications => Set<TenantNotification>();
    public DbSet<UserNotificationRead> UserNotificationReads => Set<UserNotificationRead>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.PrimaryColor).HasColumnName("primary_color").HasMaxLength(10).IsRequired();
            entity.Property(x => x.SecondaryColor).HasColumnName("secondary_color").HasMaxLength(10).IsRequired();
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.LogoUrl).HasColumnName("logo_url");
            entity.Property(x => x.LogoMediaId).HasColumnName("logo_media_id");

            entity.HasOne(x => x.LogoMedia)
                .WithMany()
                .HasForeignKey(x => x.LogoMediaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlatformUser>(entity =>
        {
            entity.ToTable("platform_users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasColumnName("email").HasColumnType("citext").HasMaxLength(150).IsRequired();
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(x => x.ProfilePhotoUrl).HasColumnName("profile_photo_url");
            entity.Property(x => x.ProfilePhotoStorageKey).HasColumnName("profile_photo_storage_key");
            entity.Property(x => x.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.MustChangePassword).HasColumnName("must_change_password");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<PlatformAuditLog>(entity =>
        {
            entity.ToTable("platform_audit_logs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.PlatformUserId).HasColumnName("platform_user_id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.Action).HasColumnName("action").HasMaxLength(120).IsRequired();
            entity.Property(x => x.EntityType).HasColumnName("entity_type").HasMaxLength(120).IsRequired();
            entity.Property(x => x.EntityId).HasColumnName("entity_id");
            entity.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(400).IsRequired();
            entity.Property(x => x.MetadataJson).HasColumnName("metadata_json");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => new { x.TenantId, x.CreatedAt });

            entity.HasOne(x => x.PlatformUser)
                .WithMany()
                .HasForeignKey(x => x.PlatformUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasColumnName("email").HasColumnType("citext").HasMaxLength(150).IsRequired();
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(x => x.Role).HasColumnName("role").HasConversion<int>().IsRequired();
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.MustChangePassword).HasColumnName("must_change_password");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();

            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlatformRefreshToken>(entity =>
        {
            entity.ToTable("platform_refresh_tokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Token).HasColumnName("token").IsRequired();
            entity.Property(x => x.PlatformUserId).HasColumnName("platform_user_id");
            entity.Property(x => x.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(x => x.IsUsed).HasColumnName("is_used");
            entity.Property(x => x.IsRevoked).HasColumnName("is_revoked");
            entity.HasIndex(x => x.Token).IsUnique();

            entity.HasOne(x => x.PlatformUser)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.PlatformUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TenantMedia>(entity =>
        {
            entity.ToTable("tenant_medias");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.FileName).HasColumnName("file_name").IsRequired();
            entity.Property(x => x.ContentType).HasColumnName("content_type").IsRequired();
            entity.Property(x => x.StorageKey).HasColumnName("storage_key").IsRequired();
            entity.Property(x => x.Url).HasColumnName("url").IsRequired();
            entity.Property(x => x.Size).HasColumnName("size");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TenantId);

            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.Medias)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Token).HasColumnName("token").IsRequired();
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(x => x.IsUsed).HasColumnName("is_used");
            entity.Property(x => x.IsRevoked).HasColumnName("is_revoked");
            entity.HasIndex(x => x.Token).IsUnique();
            
            entity.HasOne(x => x.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Feature>(entity =>
        {
            entity.ToTable("features");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(250);
        });

        modelBuilder.Entity<TenantFeature>(entity =>
        {
            entity.ToTable("tenant_features");
            entity.HasKey(x => new { x.TenantId, x.FeatureId });
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.FeatureId).HasColumnName("feature_id");

            entity.HasOne(x => x.Tenant)
                .WithMany(t => t.TenantFeatures)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Feature)
                .WithMany(x => x.TenantFeatures)
                .HasForeignKey(x => x.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Mapeia configuracoes persistidas por tenant e usuario.
        // A V1 usa essa tabela para guardar a selecao de medidores da home do dashboard.
        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.ToTable("configurations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.Key).HasColumnName("key").HasMaxLength(150).IsRequired();
            entity.Property(x => x.ValueJson).HasColumnName("value_json").IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.Key }).IsUnique();

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Mapeia a notificacao institucional exibida no app do aluno.
        // A mesma estrutura sera reaproveitada por eventos, informes, aulas e produtos.
        modelBuilder.Entity<TenantNotification>(entity =>
        {
            entity.ToTable("tenant_notifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.Type).HasColumnName("type").HasConversion<int>().IsRequired();
            entity.Property(x => x.Audience).HasColumnName("audience").HasConversion<int>().IsRequired();
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(300);
            entity.Property(x => x.Content).HasColumnName("content").IsRequired();
            entity.Property(x => x.MediaId).HasColumnName("media_id");
            entity.Property(x => x.IsHighlighted).HasColumnName("is_highlighted");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.RelatedEntityType).HasColumnName("related_entity_type").HasMaxLength(100);
            entity.Property(x => x.RelatedEntityId).HasColumnName("related_entity_id");
            entity.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(x => x.PublishedAt).HasColumnName("published_at");
            entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.MediaId);
            entity.HasIndex(x => x.CreatedByUserId);
            entity.HasIndex(x => new { x.TenantId, x.IsActive, x.PublishedAt });
            entity.HasIndex(x => new { x.TenantId, x.Type, x.Audience, x.PublishedAt });

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Media)
                .WithMany()
                .HasForeignKey(x => x.MediaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeia a leitura individual da notificacao por usuario.
        // Esse registro permite separar pendencias, historico e contagem do sino do app.
        modelBuilder.Entity<UserNotificationRead>(entity =>
        {
            entity.ToTable("user_notification_reads");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.TenantNotificationId).HasColumnName("tenant_notification_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.ReadAt).HasColumnName("read_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.TenantNotificationId);
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.ReadAt });
            entity.HasIndex(x => new { x.TenantId, x.TenantNotificationId, x.UserId }).IsUnique();

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Notification)
                .WithMany(x => x.Reads)
                .HasForeignKey(x => x.TenantNotificationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Mapeia os textos legais exibidos no app, centralizando Termos de Uso e Politica de Privacidade por tenant.
        modelBuilder.Entity<TenantLegalContent>(entity =>
        {
            entity.ToTable("tenant_legal_contents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.TermsOfUse).HasColumnName("terms_of_use").IsRequired();
            entity.Property(x => x.PrivacyPolicy).HasColumnName("privacy_policy").IsRequired();
            entity.Property(x => x.UpdatedByUserId).HasColumnName("updated_by_user_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.TenantId).IsUnique();
            entity.HasIndex(x => x.UpdatedByUserId);

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.UpdatedByUser)
                .WithMany()
                .HasForeignKey(x => x.UpdatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.ToTable("exercises");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.MuscleGroup).HasColumnName("muscle_group").HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.MediaId).HasColumnName("media_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TenantId);

            entity.HasOne(x => x.Media)
                .WithMany()
                .HasForeignKey(x => x.MediaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Workout>(entity =>
        {
            entity.ToTable("workouts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.Goal).HasColumnName("goal");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TenantId);

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkoutExercise>(entity =>
        {
            entity.ToTable("workout_exercises");
            entity.HasKey(x => new { x.WorkoutId, x.ExerciseId });
            entity.Property(x => x.WorkoutId).HasColumnName("workout_id");
            entity.Property(x => x.ExerciseId).HasColumnName("exercise_id");
            entity.Property(x => x.Order).HasColumnName("order");
            entity.Property(x => x.Sets).HasColumnName("sets");
            entity.Property(x => x.Reps).HasColumnName("reps").IsRequired();
            entity.Property(x => x.RestTime).HasColumnName("rest_time");
            entity.Property(x => x.Notes).HasColumnName("notes");

            entity.HasOne(x => x.Workout)
                .WithMany(w => w.WorkoutExercises)
                .HasForeignKey(x => x.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Exercise)
                .WithMany(e => e.WorkoutExercises)
                .HasForeignKey(x => x.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Mapeia o registro de check-in com os vínculos de tenant e usuário.
        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.ToTable("checkins");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.CheckedInAt).HasColumnName("checked_in_at");
            entity.Property(x => x.Notes).HasColumnName("notes");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.UserId);
            // Este índice ajuda nas consultas por tenant, usuário e período.
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.CheckedInAt });

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Mapeia a agenda de aulas com vínculos de tenant e instrutor.
        modelBuilder.Entity<GymClass>(entity =>
        {
            entity.ToTable("gym_classes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.InstructorUserId).HasColumnName("instructor_user_id");
            entity.Property(x => x.StartsAt).HasColumnName("starts_at");
            entity.Property(x => x.EndsAt).HasColumnName("ends_at");
            entity.Property(x => x.Location).HasColumnName("location").HasMaxLength(150);
            entity.Property(x => x.Capacity).HasColumnName("capacity");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.IsSpecialClass).HasColumnName("is_special_class");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.InstructorUserId);
            // Este índice ajuda na listagem e nos filtros por período dentro do tenant.
            entity.HasIndex(x => new { x.TenantId, x.StartsAt });

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.InstructorUser)
                .WithMany()
                .HasForeignKey(x => x.InstructorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeia a reserva de aula com vínculos de tenant, aula e usuário.
        modelBuilder.Entity<ClassBooking>(entity =>
        {
            entity.ToTable("class_bookings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.GymClassId).HasColumnName("gym_class_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.BookedAt).HasColumnName("booked_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.GymClassId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => new { x.GymClassId, x.UserId }).IsUnique();

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.GymClass)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.GymClassId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Mapeia os dados de profile do usuario, separados do nucleo de autenticacao.
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.Username).HasColumnName("username").HasMaxLength(50);
            entity.Property(x => x.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20);
            entity.Property(x => x.BirthDate).HasColumnName("birth_date").HasColumnType("date");
            entity.Property(x => x.ProfilePhotoUrl).HasColumnName("profile_photo_url");
            entity.Property(x => x.ProfileMediaId).HasColumnName("profile_media_id");
            entity.Property(x => x.ActivePlan).HasColumnName("active_plan").HasMaxLength(150);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Username }).IsUnique();

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ProfileMedia)
                .WithMany()
                .HasForeignKey(x => x.ProfileMediaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeia o catalogo de planos da academia.
        modelBuilder.Entity<MembershipPlan>(entity =>
        {
            entity.ToTable("membership_plans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.Price).HasColumnName("price").HasColumnType("numeric(10,2)");
            entity.Property(x => x.DurationDays).HasColumnName("duration_days");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TenantId);

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeia o vinculo entre usuario e plano.
        modelBuilder.Entity<UserMembership>(entity =>
        {
            entity.ToTable("user_memberships");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.MembershipPlanId).HasColumnName("membership_plan_id");
            entity.Property(x => x.StartsAt).HasColumnName("starts_at");
            entity.Property(x => x.EndsAt).HasColumnName("ends_at");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.BillingDueDay).HasColumnName("billing_due_day");
            entity.Property(x => x.StatusReason).HasColumnName("status_reason").HasMaxLength(300);
            entity.Property(x => x.ChangedByUserId).HasColumnName("changed_by_user_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.MembershipPlanId);
            entity.HasIndex(x => x.ChangedByUserId);
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.IsActive });

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.MembershipPlan)
                .WithMany(x => x.UserMemberships)
                .HasForeignKey(x => x.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeia o registro de pagamento de mensalidade/plano do aluno.
        modelBuilder.Entity<MembershipPayment>(entity =>
        {
            entity.ToTable("membership_payments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.MembershipPlanId).HasColumnName("membership_plan_id");
            entity.Property(x => x.UserMembershipId).HasColumnName("user_membership_id");
            entity.Property(x => x.AmountPaid).HasColumnName("amount_paid").HasColumnType("numeric(10,2)");
            entity.Property(x => x.DueDate).HasColumnName("due_date");
            entity.Property(x => x.PaidAt).HasColumnName("paid_at");
            entity.Property(x => x.IsPaidOnTime).HasColumnName("is_paid_on_time");
            entity.Property(x => x.RecordedByUserId).HasColumnName("recorded_by_user_id");
            entity.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(300);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.MembershipPlanId);
            entity.HasIndex(x => x.UserMembershipId);
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.DueDate });

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.MembershipPlan)
                .WithMany()
                .HasForeignKey(x => x.MembershipPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.UserMembership)
                .WithMany()
                .HasForeignKey(x => x.UserMembershipId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RecordedByUser)
                .WithMany()
                .HasForeignKey(x => x.RecordedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeia os produtos da loja interna do tenant.
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.MainMediaId).HasColumnName("main_media_id");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IsActive });

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.MainMedia)
                .WithMany()
                .HasForeignKey(x => x.MainMediaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeia as variantes comerciais de cada produto.
        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.ToTable("product_variants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Color).HasColumnName("color").HasMaxLength(50);
            entity.Property(x => x.Size).HasColumnName("size").HasMaxLength(50);
            entity.Property(x => x.Price).HasColumnName("price").HasColumnType("numeric(10,2)");
            entity.Property(x => x.StockQuantity).HasColumnName("stock_quantity");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.ProductId);
            entity.HasIndex(x => new { x.TenantId, x.ProductId, x.IsActive });

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.Variants)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Mapeia o pedido da loja com fluxo operacional presencial.
        modelBuilder.Entity<StoreOrder>(entity =>
        {
            entity.ToTable("store_orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<int>().IsRequired();
            entity.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("numeric(10,2)");
            entity.Property(x => x.CustomerNotes).HasColumnName("customer_notes").HasMaxLength(500);
            entity.Property(x => x.AdminNotes).HasColumnName("admin_notes").HasMaxLength(500);
            entity.Property(x => x.LastUpdatedByUserId).HasColumnName("last_updated_by_user_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LastUpdatedByUser)
                .WithMany()
                .HasForeignKey(x => x.LastUpdatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeia os itens do pedido com snapshot comercial.
        modelBuilder.Entity<StoreOrderItem>(entity =>
        {
            entity.ToTable("store_order_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.StoreOrderId).HasColumnName("store_order_id");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.ProductVariantId).HasColumnName("product_variant_id");
            entity.Property(x => x.ProductName).HasColumnName("product_name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.VariantName).HasColumnName("variant_name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.VariantColor).HasColumnName("variant_color").HasMaxLength(50);
            entity.Property(x => x.VariantSize).HasColumnName("variant_size").HasMaxLength(50);
            entity.Property(x => x.Quantity).HasColumnName("quantity");
            entity.Property(x => x.UnitPrice).HasColumnName("unit_price").HasColumnType("numeric(10,2)");
            entity.Property(x => x.LineTotal).HasColumnName("line_total").HasColumnType("numeric(10,2)");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.StoreOrderId);
            entity.HasIndex(x => x.ProductId);
            entity.HasIndex(x => x.ProductVariantId);

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.StoreOrder)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.StoreOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ProductVariant)
                .WithMany()
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeia o mural institucional e outdoor da academia.
        modelBuilder.Entity<AcademyEvent>(entity =>
        {
            entity.ToTable("academy_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.EventType).HasColumnName("event_type").HasConversion<int>().IsRequired();
            entity.Property(x => x.MediaId).HasColumnName("media_id");
            entity.Property(x => x.Location).HasColumnName("location").HasMaxLength(150);
            entity.Property(x => x.StartsAt).HasColumnName("starts_at");
            entity.Property(x => x.EndsAt).HasColumnName("ends_at");
            entity.Property(x => x.IsHighlighted).HasColumnName("is_highlighted");
            entity.Property(x => x.HighlightStartsAt).HasColumnName("highlight_starts_at");
            entity.Property(x => x.HighlightEndsAt).HasColumnName("highlight_ends_at");
            entity.Property(x => x.IsOutdoorEvent).HasColumnName("is_outdoor_event");
            entity.Property(x => x.AllowParticipation).HasColumnName("allow_participation");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CheckInPassword).HasColumnName("check_in_password").HasMaxLength(20).IsRequired();
            entity.Property(x => x.IsCompleted).HasColumnName("is_completed");
            entity.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.MediaId);
            entity.HasIndex(x => x.CreatedByUserId);
            entity.HasIndex(x => new { x.TenantId, x.IsActive, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.IsOutdoorEvent, x.IsActive });
            entity.HasIndex(x => new { x.TenantId, x.EventType, x.IsActive });
            entity.HasIndex(x => new { x.TenantId, x.IsHighlighted, x.HighlightStartsAt, x.HighlightEndsAt });

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Media)
                .WithMany()
                .HasForeignKey(x => x.MediaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeia a confirmacao de participacao de alunos em eventos outdoor.
        modelBuilder.Entity<AcademyEventParticipation>(entity =>
        {
            entity.ToTable("academy_event_participations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.AcademyEventId).HasColumnName("academy_event_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.ParticipatedAt).HasColumnName("participated_at");
            entity.Property(x => x.IsPresent).HasColumnName("is_present");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.AcademyEventId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => new { x.TenantId, x.AcademyEventId, x.UserId }).IsUnique();

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AcademyEvent)
                .WithMany(x => x.Participations)
                .HasForeignKey(x => x.AcademyEventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Mapeia as publicacoes da rede social interna do tenant.
        modelBuilder.Entity<SocialPost>(entity =>
        {
            entity.ToTable("social_posts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.MediaId).HasColumnName("media_id");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.MediaId);
            entity.HasIndex(x => new { x.TenantId, x.IsActive, x.CreatedAt });

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Media)
                .WithMany()
                .HasForeignKey(x => x.MediaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeia as curtidas simples da rede interna.
        modelBuilder.Entity<SocialPostLike>(entity =>
        {
            entity.ToTable("social_post_likes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.SocialPostId).HasColumnName("social_post_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.SocialPostId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => new { x.TenantId, x.SocialPostId, x.UserId }).IsUnique();

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SocialPost)
                .WithMany(x => x.Likes)
                .HasForeignKey(x => x.SocialPostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Mapeia comentarios simples em publicacoes sociais.
        modelBuilder.Entity<SocialPostComment>(entity =>
        {
            entity.ToTable("social_post_comments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.SocialPostId).HasColumnName("social_post_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.Message).HasColumnName("message").HasMaxLength(500).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.SocialPostId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => new { x.TenantId, x.SocialPostId, x.CreatedAt });

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SocialPost)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.SocialPostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Mapeia a tabela de regras de pontuacao do tenant.
        modelBuilder.Entity<GamificationEventRule>(entity =>
        {
            entity.ToTable("gamification_event_rules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.EventType).HasColumnName("event_type").HasConversion<int>().IsRequired();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.Points).HasColumnName("points").HasColumnType("numeric(10,2)");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.EventType }).IsUnique();

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeia o registro de eventos processados para alunos.
        modelBuilder.Entity<UserGamificationEvent>(entity =>
        {
            entity.ToTable("user_gamification_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.GamificationEventRuleId).HasColumnName("gamification_event_rule_id");
            entity.Property(x => x.EventType).HasColumnName("event_type").HasConversion<int>().IsRequired();
            entity.Property(x => x.SourceEntity).HasColumnName("source_entity").HasMaxLength(100).IsRequired();
            entity.Property(x => x.SourceEntityId).HasColumnName("source_entity_id");
            entity.Property(x => x.PointsApplied).HasColumnName("points_applied").HasColumnType("numeric(10,2)");
            entity.Property(x => x.OccurredAt).HasColumnName("occurred_at");
            entity.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(300);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.OccurredAt });
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.EventType, x.SourceEntity, x.SourceEntityId }).IsUnique();

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.GamificationEventRule)
                .WithMany()
                .HasForeignKey(x => x.GamificationEventRuleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeia o razao de pontos com saldo acumulado por aluno.
        modelBuilder.Entity<PointsLedger>(entity =>
        {
            entity.ToTable("points_ledger");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.UserGamificationEventId).HasColumnName("user_gamification_event_id");
            entity.Property(x => x.PointsDelta).HasColumnName("points_delta").HasColumnType("numeric(10,2)");
            entity.Property(x => x.BalanceAfter).HasColumnName("balance_after").HasColumnType("numeric(10,2)");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.UserGamificationEventId).IsUnique();

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.UserGamificationEvent)
                .WithMany()
                .HasForeignKey(x => x.UserGamificationEventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Mapeia o snapshot mensal do ranking de alunos.
        modelBuilder.Entity<MonthlyStudentRanking>(entity =>
        {
            entity.ToTable("monthly_student_rankings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.Year).HasColumnName("year");
            entity.Property(x => x.Month).HasColumnName("month");
            entity.Property(x => x.Position).HasColumnName("position");
            entity.Property(x => x.TotalPoints).HasColumnName("total_points").HasColumnType("numeric(10,2)");
            entity.Property(x => x.PrizeDescription).HasColumnName("prize_description").HasMaxLength(300);
            entity.Property(x => x.GeneratedAt).HasColumnName("generated_at");
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.Year, x.Month, x.UserId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Year, x.Month, x.Position });

            entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
