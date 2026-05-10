namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class GymClassResponseDto
{
    public int? Capacity { get; set; }
    public DateTimeOffsetDto? CreatedAt { get; set; }
    public string? Description { get; set; }
    public DateTimeOffsetDto? EndsAt { get; set; }
    public Guid? Id { get; set; }
    public string? InstructorName { get; set; }
    public Guid? InstructorUserId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsSpecialClass { get; set; }
    public string? Location { get; set; }
    public string? Name { get; set; }
    public DateTimeOffsetDto? StartsAt { get; set; }
}