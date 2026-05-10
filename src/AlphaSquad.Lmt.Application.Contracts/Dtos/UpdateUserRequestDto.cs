namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UpdateUserRequestDto
{
    public bool? IsActive { get; set; }
    public string? Name { get; set; }
    public int? Role { get; set; }
}