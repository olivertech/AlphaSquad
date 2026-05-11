using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class ConfigurationsService : IConfigurationsService
{
    private readonly IApiFacade _apiFacade;

    public ConfigurationsService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task<DashboardConfigurationResponseDto?> GETApiConfigurationsMeDashboardAsync(CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiConfigurationsMeDashboardAsync(cancellationToken);
    }

    public Task<DashboardConfigurationResponseDto?> PUTApiConfigurationsMeDashboardAsync(UpdateDashboardConfigurationRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiConfigurationsMeDashboardAsync(request, cancellationToken);
    }
}
