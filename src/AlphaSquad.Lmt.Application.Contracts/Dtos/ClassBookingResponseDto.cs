namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class ClassBookingResponseDto
{
    public DateTimeOffset? BookedAt { get; set; }
    public string? ClassName { get; set; }
    public Guid? GymClassId { get; set; }
    public Guid? Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
}