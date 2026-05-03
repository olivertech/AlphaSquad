namespace AlphaSquad.Shared.Contracts;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }

    public AuthenticatedUserResponse User { get; set; } = new();
    public AuthenticatedTenantResponse Tenant { get; set; } = new();
}