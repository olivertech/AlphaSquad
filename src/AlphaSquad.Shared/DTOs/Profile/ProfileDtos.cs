namespace AlphaSquad.Shared.DTOs.Profile;

using AlphaSquad.Shared.DTOs.Common;

/// <summary>
/// Representa a resposta principal do modulo de profile.
/// Consolida dados basicos do usuario com campos de experiencia pessoal no app.
/// </summary>
public record ProfileResponse(
    Guid UserId,
    string Name,
    string Email,
    string? Username,
    string? PhoneNumber,
    string? BirthDate,
    UserRole Role,
    bool IsActive,
    string? ProfilePhotoUrl,
    Guid? ActivePlanId,
    string? ActivePlan,
    decimal? ActivePlanPrice,
    int? ActivePlanDurationDays,
    DateTime CreatedAt
)
{
    public MediaPresentationResponse ProfilePhoto => MediaPresentationFactory.ForAvatar(Name, ProfilePhotoUrl);
}

/// <summary>
/// Payload para atualizar os dados basicos editaveis pelo proprio usuario.
/// </summary>
public record UpdateProfileRequest(
    string Name,
    string? Username,
    string? PhoneNumber,
    string? BirthDate
);

/// <summary>
/// Payload para troca de e-mail do proprio usuario.
/// Exige o e-mail atual e a senha atual para reduzir risco de alteracao indevida.
/// </summary>
public record UpdateProfileEmailRequest(
    string CurrentEmail,
    string NewEmail,
    string CurrentPassword
);
