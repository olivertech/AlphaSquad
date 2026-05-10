namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class CreateGamificationEventRuleRequestDto
{
    public string? Description { get; set; }
    public int? EventType { get; set; }
    public string? Name { get; set; }
    public double? Points { get; set; }
}