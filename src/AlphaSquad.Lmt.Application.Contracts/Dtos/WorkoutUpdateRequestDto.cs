namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class WorkoutUpdateRequestDto
{
    public string? Description { get; set; }
    public string? Goal { get; set; }
    public bool? IsActive { get; set; }
    public string? Name { get; set; }
}