namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UpdateProfileRequestDto
{
    public string? Name { get; set; }
    public string? Username { get; set; }
    public string? PhoneNumber { get; set; }
    public string? BirthDate { get; set; }
}
