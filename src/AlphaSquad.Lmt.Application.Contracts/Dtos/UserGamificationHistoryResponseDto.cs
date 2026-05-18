namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UserGamificationHistoryResponseDto
{
    public List<UserGamificationHistoryEntryResponseDto>? Entries { get; set; }
    public bool? IsGamificationParticipant { get; set; }
    public double? TotalAccumulatedPoints { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
}
