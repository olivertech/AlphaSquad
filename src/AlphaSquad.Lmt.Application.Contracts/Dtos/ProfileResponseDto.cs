namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

/// <summary>
/// Resposta detalhada do perfil do usuário.
/// </summary>
public sealed class ProfileResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }
}
