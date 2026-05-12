namespace AlphaSquad.Web.Models;

/// <summary>
/// Opcao de aluno para reservas administrativas em aula.
/// Mantem a lista enxuta e pronta para o select da tela de detalhes.
/// </summary>
public sealed class StudentOptionViewModel
{
    public Guid Value { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DisplayLabel => $"{Name} - {Email}";
}
