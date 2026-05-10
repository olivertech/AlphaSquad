namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class LoginResponseDto
{
    public string? AccessToken { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public string? RefreshToken { get; set; }
    public AuthenticatedTenantResponseDto? Tenant { get; set; }
    public AuthenticatedUserResponseDto? User { get; set; }
}