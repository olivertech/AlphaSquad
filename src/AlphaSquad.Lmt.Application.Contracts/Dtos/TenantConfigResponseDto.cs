namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class TenantConfigResponseDto
{
    public Guid? Id { get; set; }
    public bool? IsActive { get; set; }
    public string? LogoUrl { get; set; }
    public string? Name { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? Slug { get; set; }
}