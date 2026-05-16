using AlphaSquad.Backoffice.Models;
using AlphaSquad.Shared.Helpers;

namespace AlphaSquad.Backoffice.Services;

/// <summary>
/// Catalogo local das features exibidas no onboarding de academias.
/// Ele reaproveita os codigos reais do produto, mas apresenta textos amigaveis para o sponsor.
/// </summary>
public static class BackofficeTenantCatalog
{
    public static IReadOnlyList<BackofficeFeatureOptionViewModel> FeatureOptions { get; } =
    [
        new() { Code = FeatureCodes.UserManagement, Label = "Usuarios", Description = "Gestao da equipe, alunos e acessos da academia." },
        new() { Code = FeatureCodes.CheckIn, Label = "Check-ins", Description = "Controle de entradas, frequencia e reengajamento." },
        new() { Code = FeatureCodes.Schedule, Label = "Aulas", Description = "Agenda de turmas, reservas e auloes especiais." },
        new() { Code = FeatureCodes.Workouts, Label = "Treinos", Description = "Biblioteca de exercicios e montagem de treinos." },
        new() { Code = FeatureCodes.Store, Label = "Loja", Description = "Catalogo, pedidos e retirada presencial." },
        new() { Code = FeatureCodes.Events, Label = "Eventos", Description = "Mural institucional e eventos externos da academia." },
        new() { Code = FeatureCodes.Social, Label = "Social", Description = "Publicacoes internas e engajamento dos alunos." },
        new() { Code = FeatureCodes.Gamification, Label = "Gamificacao", Description = "Ranking, pontuacao e campanhas de engajamento." },
        new() { Code = FeatureCodes.Media, Label = "Midias", Description = "Gerenciamento de imagens e arquivos da academia." }
    ];

    public static IReadOnlyList<BackofficeFeatureOptionViewModel> ResolveFeatures(IEnumerable<string> featureCodes)
    {
        var normalizedCodes = featureCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return normalizedCodes
            .Select(code =>
                FeatureOptions.FirstOrDefault(option => string.Equals(option.Code, code, StringComparison.OrdinalIgnoreCase))
                ?? new BackofficeFeatureOptionViewModel
                {
                    Code = code,
                    Label = code,
                    Description = "Feature cadastrada no banco de dados da plataforma."
                })
            .ToList();
    }
}
