namespace AlphaSquad.Shared.DTOs.Configurations;

/// <summary>
/// Retorna a configuracao persistida dos medidores da home do dashboard para o usuario autenticado.
/// </summary>
/// <param name="Id">Identificador interno do registro de configuracao.</param>
/// <param name="TenantId">Tenant ao qual a configuracao pertence.</param>
/// <param name="UserId">Usuario dono da configuracao.</param>
/// <param name="SelectedMetricKeys">Chaves dos medidores escolhidos para a home.</param>
/// <param name="UpdatedAt">Data da ultima alteracao da configuracao.</param>
public sealed record DashboardConfigurationResponse(
    Guid? Id,
    Guid TenantId,
    Guid UserId,
    IReadOnlyList<string> SelectedMetricKeys,
    DateTime? UpdatedAt
);

/// <summary>
/// Atualiza a selecao de medidores mostrados na home do dashboard para o usuario autenticado.
/// </summary>
/// <param name="SelectedMetricKeys">Lista de chaves dos medidores escolhidos.</param>
public sealed record UpdateDashboardConfigurationRequest(
    IReadOnlyList<string> SelectedMetricKeys
);
