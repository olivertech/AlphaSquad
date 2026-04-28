namespace AlphaSquad.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.Slug).IsUnique();entity.Property(x => x.PrimaryColor).HasMaxLength(10).IsRequired();
            entity.Property(x => x.SecondaryColor).HasMaxLength(10).IsRequired();
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.Role).HasConversion<int>().IsRequired();
            entity.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
            entity.HasOne(x => x.Tenant).WithMany(x => x.Users).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}