namespace AlphaSquad.Web.Models;

/// <summary>
/// Representa um usuário pronto para exibição no dashboard.
/// Essa camada evita expor detalhes da API diretamente para a Razor Page.
/// </summary>
public sealed class UserListItemViewModel
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public int? RoleValue { get; init; }
    public required string RoleLabel { get; init; }
    public required string StatusLabel { get; init; }
    public required bool IsActive { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}
