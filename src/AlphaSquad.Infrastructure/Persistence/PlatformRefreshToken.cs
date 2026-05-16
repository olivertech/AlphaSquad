namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa o refresh token do backoffice master da AlphaSquad.
/// Essa tabela é separada do fluxo multi-tenant para manter as sessões do sponsor isoladas.
/// </summary>
public class PlatformRefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public Guid PlatformUserId { get; set; }
    public PlatformUser PlatformUser { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsActive => !IsUsed && !IsRevoked && !IsExpired;
}
