using AlphaSquad.Shared.DTOs.Common;

namespace AlphaSquad.Shared.DTOs.App;

/// <summary>
/// Payload agregado da home do app.
/// Organiza seções prontas para a primeira dobra da experiência mobile.
/// </summary>
public record AppHomeResponse(
    DateTime GeneratedAtUtc,
    List<AppHomeClassSummaryResponse> UpcomingClasses,
    AppHomeCardSectionResponse Events,
    AppHomeCardSectionResponse Social,
    AppHomeCardSectionResponse Store
);

/// <summary>
/// Representa uma seção de cards resumidos da home do app.
/// O cliente pode usar `HasMore` para exibir CTA de navegação ao módulo completo.
/// </summary>
public record AppHomeCardSectionResponse(
    string Module,
    string Title,
    bool IsEnabled,
    bool HasMore,
    List<MobileCardItemResponse> Items
);

/// <summary>
/// Resumo leve de uma aula exibida na home do app.
/// Traz o mínimo necessário para o aluno entender agenda, reserva e disponibilidade.
/// </summary>
public record AppHomeClassSummaryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? InstructorName,
    DateTime StartsAt,
    DateTime EndsAt,
    string? Location,
    bool IsSpecialClass,
    bool IsUserBooked,
    int Capacity,
    int BookedCount
)
{
    public bool HasAvailableSpots => BookedCount < Capacity;
}
