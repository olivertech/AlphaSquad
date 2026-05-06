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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.PrimaryColor).HasMaxLength(10).IsRequired();
            entity.Property(x => x.SecondaryColor).HasMaxLength(10).IsRequired();

            entity.HasOne(x => x.LogoMedia)
                .WithMany()
                .HasForeignKey(x => x.LogoMediaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasColumnType("citext").HasMaxLength(150).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.Role).HasConversion<int>().IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();

            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TenantMedia>(entity =>
        {
            entity.ToTable("TenantMedias");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FileName).IsRequired();
            entity.Property(x => x.ContentType).IsRequired();
            entity.Property(x => x.StorageKey).IsRequired();
            entity.Property(x => x.Url).IsRequired();
            entity.HasIndex(x => x.TenantId);

            entity.HasOne(x => x.Tenant)
                .WithMany(x => x.Medias)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Token).IsRequired();
            entity.HasIndex(x => x.Token).IsUnique();
            
            entity.HasOne(x => x.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Feature>(entity =>
        {
            entity.ToTable("Features");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(250);
        });

        modelBuilder.Entity<TenantFeature>(entity =>
        {
            entity.ToTable("TenantFeatures");
            entity.HasKey(x => new { x.TenantId, x.FeatureId });

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
            entity.ToTable("Exercises");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.MuscleGroup).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.TenantId);

            entity.HasOne(x => x.Media)
                .WithMany()
                .HasForeignKey(x => x.MediaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Workout>(entity =>
        {
            entity.ToTable("Workouts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<WorkoutExercise>(entity =>
        {
            entity.ToTable("WorkoutExercises");
            entity.HasKey(x => new { x.WorkoutId, x.ExerciseId });

            entity.HasOne(x => x.Workout)
                .WithMany(w => w.WorkoutExercises)
                .HasForeignKey(x => x.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Exercise)
                .WithMany(e => e.WorkoutExercises)
                .HasForeignKey(x => x.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

