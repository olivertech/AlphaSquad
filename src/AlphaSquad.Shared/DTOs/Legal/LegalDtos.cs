namespace AlphaSquad.Shared.DTOs.Legal;

/// <summary>
/// Resposta usada pelo app para exibir os textos legais do tenant atual.
/// </summary>
public record TenantLegalContentResponse(
    Guid? Id,
    Guid TenantId,
    string TermsOfUse,
    string PrivacyPolicy,
    Guid? UpdatedByUserId,
    string? UpdatedByUserName,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Payload administrativo para criar ou atualizar os textos legais do tenant.
/// </summary>
public record UpdateTenantLegalContentRequest(
    string TermsOfUse,
    string PrivacyPolicy
);
