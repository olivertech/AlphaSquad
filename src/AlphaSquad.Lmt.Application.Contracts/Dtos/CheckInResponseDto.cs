namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class CheckInResponseDto
{
    public DateTimeOffset? CheckedInAt { get; set; }
    public Guid? Id { get; set; }
    public string? Notes { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public int? UserRole { get; set; }
}