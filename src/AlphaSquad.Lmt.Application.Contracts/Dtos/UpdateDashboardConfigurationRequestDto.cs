namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

/// <summary>
/// Atualiza a lista de medidores escolhidos para a home do dashboard.
/// </summary>
public sealed class UpdateDashboardConfigurationRequestDto
{
    public List<string> SelectedMetricKeys { get; set; } = [];
}
