using AlphaSquad.Web.Security;

namespace AlphaSquad.Web.Navigation;

/// <summary>
/// Centraliza a montagem do menu lateral para que o layout continue simples.
/// O resultado considera o papel do usuário e as features contratadas pela academia.
/// </summary>
public interface IDashboardNavigationService
{
    IReadOnlyList<DashboardMenuSection> Build(DashboardSessionState? sessionState);
}
