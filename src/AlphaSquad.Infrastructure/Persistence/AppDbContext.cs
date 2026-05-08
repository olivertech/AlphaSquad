namespace AlphaSquad.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<TenantMedia> TenantMedias => Set<TenantMedia>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<TenantFeature> TenantFeatures => Set<TenantFeature>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();

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
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();

            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
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
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<WorkoutExercise>(entity =>
        {
            entity.ToTable("workout_exercises");
            entity.HasKey(x => new { x.WorkoutId, x.ExerciseId });
            entity.Property(x => x.WorkoutId).HasColumnName("workout_id");
            entity.Property(x => x.ExerciseId).HasColumnName("exercise_id");

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
    }
}
