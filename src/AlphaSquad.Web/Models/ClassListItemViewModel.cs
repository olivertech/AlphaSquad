namespace AlphaSquad.Web.Models;

/// <summary>
/// Representa uma aula pronta para exibição na grade operacional do dashboard.
/// Além dos dados principais, concentra indicadores de ocupação e status para facilitar a leitura rápida.
/// </summary>
public sealed class ClassListItemViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string InstructorName { get; init; } = string.Empty;
    public DateTimeOffset? StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    public string ScheduleLabel { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public int Capacity { get; init; }
    public int BookingCount { get; init; }
    public decimal OccupancyRate { get; init; }
    public string OccupancyLabel { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string StatusLabel { get; init; } = string.Empty;
    public bool IsSpecialClass { get; init; }
    public string SpecialClassLabel { get; init; } = string.Empty;
}
