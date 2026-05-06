namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa um token de atualização (Refresh Token) utilizado para gerar novos tokens de acesso (Access Tokens)
/// sem que o usuário precise realizar o login novamente.
/// Esta entidade é fundamental para manter a sessão do usuário ativa de forma segura.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// O valor único do token gerado criptograficamente.
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Data de expiração do token. Tokens expirados não podem ser utilizados para refresh.
    /// </summary>
    public DateTime ExpiryDate { get; set; }
    
    /// <summary>
    /// Indica se o token já foi utilizado. No fluxo de rotação de tokens, um token é marcado como usado 
    /// assim que gera um novo par de tokens.
    /// </summary>
    public bool IsUsed { get; set; }
    
    /// <summary>
    /// Indica se o token foi explicitamente revogado (ex: durante o logout).
    /// </summary>
    public bool IsRevoked { get; set; }
    
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    /// <summary>
    /// Verifica se o token já ultrapassou sua data de validade.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    
    /// <summary>
    /// Um token é considerado ativo se não tiver sido usado, não tiver sido revogado e não estiver expirado.
    /// </summary>
    public bool IsActive => !IsUsed && !IsRevoked && !IsExpired;
}
