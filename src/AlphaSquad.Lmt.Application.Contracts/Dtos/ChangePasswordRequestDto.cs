namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class ChangePasswordRequestDto
{
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}