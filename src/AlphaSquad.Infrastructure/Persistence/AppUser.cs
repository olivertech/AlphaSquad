namespace AlphaSquad.Infrastructure.Persistence;

using System.Collections.Generic;

/// <summary>
/// Representa um usuário do sistema. 
/// Cada usuário está vinculado obrigatoriamente a um Tenant (Academia).
/// </summary>
public class AppUser
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navegação para o Tenant ao qual este usuário pertence.
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Coleção de Refresh Tokens associados a este usuário.
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
