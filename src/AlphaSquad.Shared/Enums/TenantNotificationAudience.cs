namespace AlphaSquad.Shared.Enums;

/// <summary>
/// Representa o publico-alvo inicial de uma notificacao institucional.
/// A primeira versao privilegia comunicados gerais e avisos voltados aos alunos do app.
/// </summary>
public enum TenantNotificationAudience
{
    StudentsOnly = 1,
    AllTenantUsers = 2
}
