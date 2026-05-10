namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class CreateUserRequestDto
{
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Password { get; set; }
    public int? Role { get; set; }
}