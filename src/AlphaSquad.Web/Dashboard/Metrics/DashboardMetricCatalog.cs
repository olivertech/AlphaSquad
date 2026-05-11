namespace AlphaSquad.Web.Dashboard.Metrics;

/// <summary>
/// Mantém os medidores disponíveis no painel administrativo.
/// Nesta primeira fase os números são ilustrativos, mas a estrutura já fica pronta para receber dados reais da API.
/// </summary>
public sealed class DashboardMetricCatalog : IDashboardMetricCatalog
{
    public const int MaxDashboardMetrics = 5;

    private static readonly IReadOnlyList<DashboardMetricDefinition> Metrics =
    [
        new DashboardMetricDefinition
        {
            Key = "student-logins",
            Title = "Acessos de alunos",
            ShortDescription = "Mostra o volume de acessos realizados pelos alunos no período.",
            DetailDescription = "Este gráfico ajuda a acompanhar a frequência de uso do ecossistema digital da academia. Quando o volume de acessos cresce, normalmente há mais contato do aluno com treinos, agenda e comunidade.",
            SummaryLabel = "Acessos de alunos",
            SummaryValue = "1.284",
            DetailPageTitle = "Análise de acessos de alunos",
            ChartKind = DashboardMetricChartKind.Line,
            Categories = ["Seg", "Ter", "Qua", "Qui", "Sex", "Sáb", "Dom"],
            Values = [142, 176, 188, 205, 221, 183, 169]
        },
        new DashboardMetricDefinition
        {
            Key = "products-sold",
            Title = "Produtos vendidos",
            ShortDescription = "Resume a quantidade de produtos vendidos na loja da academia.",
            DetailDescription = "Este medidor acompanha o desempenho comercial da loja. Ele ajuda a entender quais períodos trazem mais saídas de produtos e como a comunidade responde às campanhas da academia.",
            SummaryLabel = "Produtos vendidos",
            SummaryValue = "96",
            DetailPageTitle = "Análise de produtos vendidos",
            ChartKind = DashboardMetricChartKind.Bar,
            Categories = ["Camisetas", "Squeezes", "Bonés", "Toalhas", "Copos"],
            Values = [28, 16, 11, 19, 22]
        },
        new DashboardMetricDefinition
        {
            Key = "students-per-class",
            Title = "Média de alunos por aula",
            ShortDescription = "Aponta a média de ocupação das aulas da academia.",
            DetailDescription = "O gráfico de média por aula ajuda a visualizar quais modalidades concentram mais alunos e quais horários têm melhor ocupação ao longo da semana.",
            SummaryLabel = "Média por aula",
            SummaryValue = "18,4",
            DetailPageTitle = "Análise de média de alunos por aula",
            ChartKind = DashboardMetricChartKind.Bar,
            Categories = ["Funcional", "Spinning", "Pilates", "Cross", "Alongamento"],
            Values = [22, 19, 14, 25, 12]
        },
        new DashboardMetricDefinition
        {
            Key = "social-posts",
            Title = "Postagens da rede social",
            ShortDescription = "Indica a quantidade de publicações feitas na rede social da academia.",
            DetailDescription = "O comportamento das postagens ajuda a entender como a comunidade está interagindo com a rede da academia. Isso pode apoiar campanhas internas e ações de engajamento.",
            SummaryLabel = "Postagens da rede",
            SummaryValue = "73",
            DetailPageTitle = "Análise de postagens da rede social",
            ChartKind = DashboardMetricChartKind.Line,
            Categories = ["Sem 1", "Sem 2", "Sem 3", "Sem 4"],
            Values = [14, 18, 21, 20]
        },
        new DashboardMetricDefinition
        {
            Key = "monthly-points",
            Title = "Média de pontos do mês",
            ShortDescription = "Mostra a média de pontos acumulados pelos alunos na gamificação do mês.",
            DetailDescription = "Este indicador ajuda a perceber se a gamificação está sendo realmente vivida pelos alunos. Ele resume o nível médio de participação no mês em curso.",
            SummaryLabel = "Média de pontos",
            SummaryValue = "42,7",
            DetailPageTitle = "Análise da média de pontos do mês",
            ChartKind = DashboardMetricChartKind.Line,
            Categories = ["Sem 1", "Sem 2", "Sem 3", "Sem 4"],
            Values = [31, 37, 44, 59]
        },
        new DashboardMetricDefinition
        {
            Key = "inactive-checkins",
            Title = "Alunos sem check-in",
            ShortDescription = "Mostra quantos alunos estão há mais tempo sem retornar à academia.",
            DetailDescription = "Este gráfico ajuda a localizar rapidamente grupos de alunos que correm risco de esfriar a rotina. É um ótimo apoio para ações de retenção e reengajamento.",
            SummaryLabel = "Sem check-in há X dias",
            SummaryValue = "54",
            DetailPageTitle = "Análise de alunos sem check-in",
            ChartKind = DashboardMetricChartKind.Bar,
            Categories = ["7 dias", "15 dias", "21 dias", "30+ dias"],
            Values = [18, 13, 9, 14]
        },
        new DashboardMetricDefinition
        {
            Key = "plans-expiring",
            Title = "Planos a vencer",
            ShortDescription = "Resume os planos que estão próximos do vencimento.",
            DetailDescription = "Ajuda a antecipar contato comercial e ações de renovação. Quando bem acompanhado, esse indicador reduz perdas e melhora a retenção da base.",
            SummaryLabel = "Planos a vencer em 7 dias",
            SummaryValue = "27",
            DetailPageTitle = "Análise de planos próximos do vencimento",
            ChartKind = DashboardMetricChartKind.Donut,
            Categories = ["Até 3 dias", "4 a 7 dias", "8 a 15 dias"],
            Values = [8, 19, 12]
        },
        new DashboardMetricDefinition
        {
            Key = "pickup-orders",
            Title = "Pedidos pendentes de retirada",
            ShortDescription = "Mostra o volume de pedidos que ainda aguardam retirada na academia.",
            DetailDescription = "Esse medidor apoia a operação da loja presencial. Ele ajuda a equipe a acompanhar reservas, preparar entregas e incentivar a volta do aluno à academia.",
            SummaryLabel = "Pendentes de retirada",
            SummaryValue = "11",
            DetailPageTitle = "Análise de pedidos pendentes de retirada",
            ChartKind = DashboardMetricChartKind.Donut,
            Categories = ["Reservado", "Pronto para retirada", "Aguardando pagamento"],
            Values = [4, 5, 2]
        },
        new DashboardMetricDefinition
        {
            Key = "class-occupancy",
            Title = "Aulas com maior ocupação",
            ShortDescription = "Destaca as aulas com melhor aproveitamento de vagas.",
            DetailDescription = "Esse gráfico ajuda a identificar quais aulas sustentam melhor ocupação, apoiando decisões de grade, professores e oferta de turmas.",
            SummaryLabel = "Top ocupação de aulas",
            SummaryValue = "5 destaques",
            DetailPageTitle = "Análise das aulas com maior ocupação",
            ChartKind = DashboardMetricChartKind.Bar,
            Categories = ["Cross 19h", "Funcional 7h", "Spinning 18h", "Pilates 8h", "Cross 7h"],
            Values = [96, 88, 82, 77, 75]
        },
        new DashboardMetricDefinition
        {
            Key = "new-students",
            Title = "Novos alunos no período",
            ShortDescription = "Mostra a entrada de novos alunos na academia no intervalo acompanhado.",
            DetailDescription = "Ajuda a medir crescimento e ritmo de aquisição de alunos, além de servir de apoio para comparações com campanhas e ações comerciais.",
            SummaryLabel = "Novos alunos",
            SummaryValue = "34",
            DetailPageTitle = "Análise de novos alunos no período",
            ChartKind = DashboardMetricChartKind.Line,
            Categories = ["Jan", "Fev", "Mar", "Abr", "Mai", "Jun"],
            Values = [3, 5, 4, 7, 6, 9]
        }
    ];

    public IReadOnlyList<DashboardMetricDefinition> GetAll() => Metrics;

    public DashboardMetricDefinition? GetByKey(string? key) =>
        Metrics.FirstOrDefault(metric => string.Equals(metric.Key, key, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<DashboardMetricDefinition> GetSelected(IReadOnlyCollection<string>? selectedKeys, int limit)
    {
        var max = Math.Clamp(limit, 1, MaxDashboardMetrics);

        if (selectedKeys is null || selectedKeys.Count == 0)
            return Metrics.Take(max).ToList();

        var selected = Metrics
            .Where(metric => selectedKeys.Contains(metric.Key, StringComparer.OrdinalIgnoreCase))
            .Take(max)
            .ToList();

        return selected.Count > 0
            ? selected
            : Metrics.Take(max).ToList();
    }
}
