namespace AlphaSquad.Infrastructure.Persistence;

public class Tenant
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public string PrimaryColor { get; set; } = "#111827";
    public string SecondaryColor { get; set; } = "#2563EB";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AppUser> Users { get; set; } = [];
}
