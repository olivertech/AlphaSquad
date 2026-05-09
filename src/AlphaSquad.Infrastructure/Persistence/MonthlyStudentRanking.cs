namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa o snapshot de ranking mensal de alunos por tenant.
/// Esta tabela preserva historico de vencedores e facilita consultas do app.
/// </summary>
public class MonthlyStudentRanking
{
    /// <summary>
    /// Identificador unico do snapshot.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant ao qual o ranking pertence.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Usuario aluno classificado.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Ano de competencia do ranking.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Mes de competencia do ranking.
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Posicao final do aluno no fechamento mensal.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Pontos totais do aluno na competencia.
    /// </summary>
    public decimal TotalPoints { get; set; }

    /// <summary>
    /// Premio planejado para esta colocacao, quando houver.
    /// </summary>
    public string? PrizeDescription { get; set; }

    /// <summary>
    /// Data de geracao do snapshot.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}
