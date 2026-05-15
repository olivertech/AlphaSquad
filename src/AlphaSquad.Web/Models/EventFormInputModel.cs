using System.ComponentModel.DataAnnotations;

namespace AlphaSquad.Web.Models;

/// <summary>
/// Representa os dados administrativos de criação e edição de eventos no dashboard.
/// O mesmo modelo é reaproveitado para manter consistência visual e de validação.
/// </summary>
public sealed class EventFormInputModel
{
    [Required(ErrorMessage = "Informe o título do evento.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public string? Location { get; set; }
    public Guid? MediaId { get; set; }
    public bool IsOutdoorEvent { get; set; } = true;
    public bool AllowParticipation { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
