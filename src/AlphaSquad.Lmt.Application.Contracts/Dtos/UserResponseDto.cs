namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UserResponseDto
{
    public DateTimeOffset? CreatedAt { get; set; }
    public string? Email { get; set; }
    public Guid? Id { get; set; }
    public bool? IsActive { get; set; }
    public string? Name { get; set; }
    public int? Role { get; set; }
    public Guid? TenantId { get; set; }
}