namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class LoginRequestDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? TenantSlug { get; set; }
}