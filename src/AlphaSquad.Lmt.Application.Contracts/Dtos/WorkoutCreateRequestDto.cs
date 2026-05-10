namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class WorkoutCreateRequestDto
{
    public string? Description { get; set; }
    public string? Goal { get; set; }
    public string? Name { get; set; }
}