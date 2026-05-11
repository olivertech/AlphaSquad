using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface IConfigurationsService
{
    Task<DashboardConfigurationResponseDto?> GETApiConfigurationsMeDashboardAsync(CancellationToken cancellationToken = default);

    Task<DashboardConfigurationResponseDto?> PUTApiConfigurationsMeDashboardAsync(UpdateDashboardConfigurationRequestDto request, CancellationToken cancellationToken = default);
}
