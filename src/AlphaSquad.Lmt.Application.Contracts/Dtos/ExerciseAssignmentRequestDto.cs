namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class ExerciseAssignmentRequestDto
{
    public Guid? ExerciseId { get; set; }
    public string? Notes { get; set; }
    public int? Order { get; set; }
    public string? Reps { get; set; }
    public int? RestTime { get; set; }
    public int? Sets { get; set; }
}