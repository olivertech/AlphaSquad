namespace AlphaSquad.Shared.Helpers;

/// <summary>
/// Centraliza os nomes das policies usadas pela API.
/// Isso evita strings soltas nos endpoints e facilita a evolucao das regras de permissao.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy para acoes restritas a administradores do tenant.
    /// </summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>
    /// Policy para operacoes de gestao que podem ser executadas por administradores e professores.
    /// </summary>
    public const string AdminOrTeacher = "AdminOrTeacher";
}
