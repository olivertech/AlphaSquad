namespace AlphaSquad.Web.Models;

/// <summary>
/// Opcao de instrutor disponivel para vincular uma aula.
/// Ela simplifica a montagem do select sem expor a estrutura completa do usuario na view.
/// </summary>
public sealed class InstructorOptionViewModel
{
    public Guid Value { get; init; }
    public string Name { get; init; } = string.Empty;
    public string RoleLabel { get; init; } = string.Empty;
    public string DisplayLabel => $"{Name} ({RoleLabel})";
}
