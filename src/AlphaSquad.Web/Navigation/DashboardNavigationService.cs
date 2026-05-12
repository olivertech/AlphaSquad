using AlphaSquad.Shared.Helpers;
using AlphaSquad.Web.Security;

namespace AlphaSquad.Web.Navigation;

/// <summary>
/// Converte o contexto autenticado do dashboard em uma árvore de navegação amigável.
/// A regra principal é mostrar apenas módulos contratados e esconder áreas restritas para perfis inadequados.
/// </summary>
public sealed class DashboardNavigationService : IDashboardNavigationService
{
    private static readonly IReadOnlyList<DashboardMenuSection> Blueprint =
    [
        new DashboardMenuSection
        {
            Title = "Visão geral",
            Items =
            [
                new DashboardMenuItem
                {
                    Label = "Dashboard",
                    PagePath = "/Dashboard/Index",
                    IconKey = "dashboard",
                    AllowedRoles = [DashboardRoles.Admin, DashboardRoles.Teacher]
                }
            ]
        },
        new DashboardMenuSection
        {
            Title = "Módulos da academia",
            Items =
            [
                new DashboardMenuItem
                {
                    Label = "Operação",
                    IconKey = "operations",
                    AllowedRoles = [DashboardRoles.Admin, DashboardRoles.Teacher],
                    Children =
                    [
                        new DashboardMenuItem
                        {
                            Label = "Check-ins",
                            PagePath = "/Dashboard/Checkins/Index",
                            FeatureCode = FeatureCodes.CheckIn,
                            AllowedRoles = [DashboardRoles.Admin]
                        },
                        new DashboardMenuItem
                        {
                            Label = "Aulas",
                            PagePath = "/Dashboard/Classes/Index",
                            FeatureCode = FeatureCodes.Schedule,
                            AllowedRoles = [DashboardRoles.Admin, DashboardRoles.Teacher]
                        },
                        new DashboardMenuItem
                        {
                            Label = "Eventos",
                            PagePath = "/Dashboard/Events/Index",
                            FeatureCode = FeatureCodes.Events,
                            AllowedRoles = [DashboardRoles.Admin, DashboardRoles.Teacher]
                        },
                        new DashboardMenuItem
                        {
                            Label = "Loja",
                            PagePath = "/Dashboard/Store/Index",
                            FeatureCode = FeatureCodes.Store,
                            AllowedRoles = [DashboardRoles.Admin]
                        }
                    ]
                },
                new DashboardMenuItem
                {
                    Label = "Treinos",
                    IconKey = "training",
                    AllowedRoles = [DashboardRoles.Admin, DashboardRoles.Teacher],
                    Children =
                    [
                        new DashboardMenuItem
                        {
                            Label = "Exercícios",
                            PagePath = "/Dashboard/Exercises/Index",
                            FeatureCode = FeatureCodes.Workouts,
                            AllowedRoles = [DashboardRoles.Admin, DashboardRoles.Teacher]
                        },
                        new DashboardMenuItem
                        {
                            Label = "Treinos",
                            PagePath = "/Dashboard/Workouts/Index",
                            FeatureCode = FeatureCodes.Workouts,
                            AllowedRoles = [DashboardRoles.Admin, DashboardRoles.Teacher]
                        }
                    ]
                },
                new DashboardMenuItem
                {
                    Label = "Comunidade",
                    IconKey = "community",
                    AllowedRoles = [DashboardRoles.Admin, DashboardRoles.Teacher],
                    Children =
                    [
                        new DashboardMenuItem
                        {
                            Label = "Social",
                            PagePath = "/Dashboard/Social/Index",
                            FeatureCode = FeatureCodes.Social,
                            AllowedRoles = [DashboardRoles.Admin, DashboardRoles.Teacher]
                        },
                        new DashboardMenuItem
                        {
                            Label = "Gamificação",
                            PagePath = "/Dashboard/Gamification/Index",
                            FeatureCode = FeatureCodes.Gamification,
                            AllowedRoles = [DashboardRoles.Admin]
                        }
                    ]
                },
                new DashboardMenuItem
                {
                    Label = "Gestão",
                    IconKey = "management",
                    AllowedRoles = [DashboardRoles.Admin],
                    Children =
                    [
                        new DashboardMenuItem
                        {
                            Label = "Usuários",
                            PagePath = "/Dashboard/Users/Index",
                            FeatureCode = FeatureCodes.UserManagement,
                            AllowedRoles = [DashboardRoles.Admin]
                        },
                        new DashboardMenuItem
                        {
                            Label = "Planos",
                            PagePath = "/Dashboard/Plans/Index",
                            AllowedRoles = [DashboardRoles.Admin]
                        },
                        new DashboardMenuItem
                        {
                            Label = "Mídias",
                            PagePath = "/Dashboard/Media/Index",
                            FeatureCode = FeatureCodes.Media,
                            AllowedRoles = [DashboardRoles.Admin]
                        },
                        new DashboardMenuItem
                        {
                            Label = "Academia",
                            PagePath = "/Dashboard/Academy/Index",
                            AllowedRoles = [DashboardRoles.Admin]
                        },
                        new DashboardMenuItem
                        {
                            Label = "Termos e políticas",
                            PagePath = "/Dashboard/Legal/Index",
                            AllowedRoles = [DashboardRoles.Admin]
                        }
                    ]
                }
            ]
        },
        new DashboardMenuSection
        {
            Title = "Conta",
            Items =
            [
                new DashboardMenuItem
                {
                    Label = "Configurações",
                    PagePath = "/Dashboard/Settings/Index",
                    IconKey = "settings",
                    AllowedRoles = [DashboardRoles.Admin, DashboardRoles.Teacher]
                }
            ]
        }
    ];

    public IReadOnlyList<DashboardMenuSection> Build(DashboardSessionState? sessionState)
    {
        var role = sessionState?.Role ?? string.Empty;
        var enabledFeatures = new HashSet<string>(
            sessionState?.EnabledFeatureCodes ?? [],
            StringComparer.OrdinalIgnoreCase);

        return Blueprint
            .Select(section => new DashboardMenuSection
            {
                Title = section.Title,
                Items = FilterItems(section.Items, role, enabledFeatures)
            })
            .Where(section => section.Items.Count > 0)
            .ToList();
    }

    /// <summary>
    /// Aplica as regras de role e feature item a item, preservando apenas o que faz sentido para a sessão atual.
    /// </summary>
    private static IReadOnlyList<DashboardMenuItem> FilterItems(
        IReadOnlyList<DashboardMenuItem> items,
        string role,
        HashSet<string> enabledFeatures)
    {
        return items
            .Where(item => IsAllowedForRole(item, role) && HasRequiredFeature(item, enabledFeatures))
            .Select(item =>
            {
                var children = item.HasChildren
                    ? FilterItems(item.Children, role, enabledFeatures)
                    : [];

                return new DashboardMenuItem
                {
                    Label = item.Label,
                    PagePath = item.PagePath,
                    IconKey = item.IconKey,
                    FeatureCode = item.FeatureCode,
                    AllowedRoles = item.AllowedRoles,
                    Children = children
                };
            })
            .Where(item => !item.HasChildren || item.Children.Count > 0)
            .ToList();
    }

    private static bool IsAllowedForRole(DashboardMenuItem item, string role) =>
        item.AllowedRoles.Count == 0 ||
        item.AllowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase);

    private static bool HasRequiredFeature(DashboardMenuItem item, HashSet<string> enabledFeatures) =>
        string.IsNullOrWhiteSpace(item.FeatureCode) ||
        enabledFeatures.Contains(item.FeatureCode);
}
