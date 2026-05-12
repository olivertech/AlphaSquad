namespace AlphaSquad.Web.Models;

/// <summary>
/// Representa uma reserva de aula em formato amigável para os blocos laterais do dashboard.
/// Essa visão destaca o aluno, a turma e o momento da reserva sem expor detalhes desnecessários.
/// </summary>
public sealed class ClassBookingListItemViewModel
{
    public Guid BookingId { get; init; }
    public Guid GymClassId { get; init; }
    public Guid UserId { get; init; }
    public string ClassName { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string UserRoleLabel { get; init; } = string.Empty;
    public string ScheduleLabel { get; init; } = string.Empty;
    public DateTimeOffset? BookedAt { get; init; }
    public bool IsSpecialClass { get; init; }
}
