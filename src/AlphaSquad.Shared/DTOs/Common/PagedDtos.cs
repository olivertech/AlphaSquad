namespace AlphaSquad.Shared.DTOs.Common;

/// <summary>
/// Contrato page-based padronizado para dashboards, consultas administrativas e listagens tradicionais.
/// Ele convive com o contrato cursor-based usado pelos feeds infinitos do app.
/// </summary>
public record PagedResponse<T>(
    int Page,
    int PageSize,
    int Total,
    List<T> Items
);
