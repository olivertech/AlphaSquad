namespace AlphaSquad.Shared.Enums;

/// <summary>
/// Define os tipos de notificacao que podem chegar ao app do aluno.
/// Essa classificacao ajuda a organizar filtros e futuras integracoes com mural, aulas e loja.
/// </summary>
public enum TenantNotificationType
{
    InformativeNotice = 1,
    Event = 2,
    Class = 3,
    Product = 4,
    BirthdayHighlight = 5,
    GamificationWinnersHighlight = 6
}
