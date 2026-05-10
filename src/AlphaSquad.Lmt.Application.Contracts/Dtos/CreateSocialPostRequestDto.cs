namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class CreateSocialPostRequestDto
{
    public string? Description { get; set; }
    public Guid? MediaId { get; set; }
}