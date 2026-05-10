namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class GamificationEventRuleResponseDto
{
    public DateTimeOffset? CreatedAt { get; set; }
    public string? Description { get; set; }
    public int? EventType { get; set; }
    public Guid? Id { get; set; }
    public bool? IsActive { get; set; }
    public string? Name { get; set; }
    public double? Points { get; set; }
}