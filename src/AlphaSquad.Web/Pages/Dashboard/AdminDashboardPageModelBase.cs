using AlphaSquad.Web.Security;
using Microsoft.AspNetCore.Authorization;

namespace AlphaSquad.Web.Pages.Dashboard;

/// <summary>
/// Base para páginas estritamente administrativas.
/// Ela mantém o painel coerente com as regras do backend, deixando essas áreas apenas para administradores.
/// </summary>
[Authorize(Roles = DashboardRoles.Admin)]
public abstract class AdminDashboardPageModelBase : DashboardPageModelBase
{
}
