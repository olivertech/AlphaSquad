namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UpdateTenantRequestDto
{
    public string? LogoUrl { get; set; }
    public string? Name { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
}