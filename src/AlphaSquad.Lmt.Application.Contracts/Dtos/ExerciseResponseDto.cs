namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class ExerciseResponseDto
{
    public DateTimeOffset? CreatedAt { get; set; }
    public string? Description { get; set; }
    public Guid? Id { get; set; }
    public Guid? MediaId { get; set; }
    public string? MediaUrl { get; set; }
    public string? MuscleGroup { get; set; }
    public string? Name { get; set; }
}