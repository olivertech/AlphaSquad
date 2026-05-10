namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UpdateGamificationEventRuleRequestDto
{
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public string? Name { get; set; }
    public double? Points { get; set; }
}