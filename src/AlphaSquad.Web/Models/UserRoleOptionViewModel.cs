namespace AlphaSquad.Web.Models;

/// <summary>
/// Define uma opção de perfil exibida nas telas de cadastro e edição de usuários.
/// </summary>
public sealed class UserRoleOptionViewModel
{
    public required int Value { get; init; }
    public required string Label { get; init; }
    public required string Description { get; init; }
}
