namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UpdateProfileEmailRequestDto
{
    public string? CurrentEmail { get; set; }
    public string? CurrentPassword { get; set; }
    public string? NewEmail { get; set; }
}