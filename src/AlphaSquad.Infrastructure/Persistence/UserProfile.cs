namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa a camada de profile do usuario autenticado.
/// Esta entidade separa dados de experiencia do usuario dos dados nucleares de autenticacao.
/// </summary>
public class UserProfile
{
    /// <summary>
    /// Identificador unico do registro de profile.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant ao qual o profile pertence.
    /// Mantem o isolamento multi-tenant mesmo sendo um recurso do usuario.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Usuario dono deste profile.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Nome curto de usuario para exibicao no app e em futuras features sociais.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Celular do usuario com DDD para contatos eventuais da academia.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Data de nascimento usada para campanhas e destaque de aniversariantes.
    /// </summary>
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// URL publica da foto de profile.
    /// E mantida para simplificar a leitura no app cliente.
    /// </summary>
    public string? ProfilePhotoUrl { get; set; }

    /// <summary>
    /// Referencia opcional para a midia usada como foto de profile.
    /// Permite substituir ou remover o arquivo com seguranca.
    /// </summary>
    public Guid? ProfileMediaId { get; set; }

    /// <summary>
    /// Campo legado de snapshot textual do plano.
    /// O plano ativo efetivo do usuario deve ser consultado em user_memberships + membership_plans.
    /// </summary>
    public string? ActivePlan { get; set; }

    /// <summary>
    /// Data de criacao do profile.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da ultima atualizacao do profile.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public AppUser User { get; set; } = null!;
    public TenantMedia? ProfileMedia { get; set; }
}
