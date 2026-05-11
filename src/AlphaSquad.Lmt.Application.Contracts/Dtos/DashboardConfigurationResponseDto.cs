namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

/// <summary>
/// Representa a configuracao persistida dos medidores exibidos na home do dashboard.
/// </summary>
public sealed class DashboardConfigurationResponseDto
{
    public Guid? Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public List<string> SelectedMetricKeys { get; set; } = [];
    public DateTimeOffset? UpdatedAt { get; set; }
}
