namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

/// <summary>
/// Requisição para atualização do e-mail do perfil.
/// </summary>
public sealed class UpdateProfileEmailRequestDto
{
    public string Email { get; set; } = string.Empty;
}
