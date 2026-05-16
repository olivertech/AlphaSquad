namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa um usuário global da AlphaSquad.
/// Esse usuário não pertence a uma academia específica e opera o backoffice master da plataforma.
/// </summary>
public class PlatformUser
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = PlatformRoles.Owner;
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Refresh tokens emitidos para este usuário global.
    /// Eles são separados dos tokens do mundo tenant para evitar colisão entre contextos.
    /// </summary>
    public ICollection<PlatformRefreshToken> RefreshTokens { get; set; } = [];
}
