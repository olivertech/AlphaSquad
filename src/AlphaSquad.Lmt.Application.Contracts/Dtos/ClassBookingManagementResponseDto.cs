namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class ClassBookingManagementResponseDto
{
    public DateTimeOffsetDto? BookedAt { get; set; }
    public Guid? BookingId { get; set; }
    public string? ClassName { get; set; }
    public DateTimeOffsetDto? EndsAt { get; set; }
    public Guid? GymClassId { get; set; }
    public bool? IsSpecialClass { get; set; }
    public string? Location { get; set; }
    public DateTimeOffsetDto? StartsAt { get; set; }
    public string? UserEmail { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public int? UserRole { get; set; }
}