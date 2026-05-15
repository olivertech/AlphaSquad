namespace AlphaSquad.Web.Models;

/// <summary>
/// Representa um aluno selecionável ou já matriculado em um evento.
/// Também expõe o status de presença para facilitar a operação no dashboard.
/// </summary>
public sealed class EventParticipantOptionViewModel
{
    public string Email { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public bool IsPresent { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset? ParticipatedAt { get; set; }
}
