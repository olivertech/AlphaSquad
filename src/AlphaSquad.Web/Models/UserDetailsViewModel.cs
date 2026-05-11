namespace AlphaSquad.Web.Models;

/// <summary>
/// Representa a visão detalhada de um usuário dentro do dashboard administrativo.
/// </summary>
public sealed class UserDetailsViewModel
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string RoleLabel { get; init; }
    public required string RoleDescription { get; init; }
    public required string StatusLabel { get; init; }
    public required bool IsActive { get; init; }
    public required string Initials { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}
