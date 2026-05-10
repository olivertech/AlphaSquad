namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class WorkoutResponseDto
{
    public DateTimeOffsetDto? CreatedAt { get; set; }
    public string? Description { get; set; }
    public string? Goal { get; set; }
    public Guid? Id { get; set; }
    public bool? IsActive { get; set; }
    public string? Name { get; set; }
}