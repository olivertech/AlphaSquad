namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

/// <summary>
/// Resposta contendo a lista de todos os participantes de um evento.
/// </summary>
public sealed class AcademyEventParticipantsResponseDto
{
    public List<AcademyEventParticipationResponseDto> Participants { get; set; } = [];
}
