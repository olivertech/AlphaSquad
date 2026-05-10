namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class ExerciseUpdateRequestDto
{
    public string? Description { get; set; }
    public Guid? MediaId { get; set; }
    public string? MuscleGroup { get; set; }
    public string? Name { get; set; }
}