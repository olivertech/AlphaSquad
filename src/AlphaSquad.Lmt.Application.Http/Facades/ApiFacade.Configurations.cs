using System.Net.Http.Json;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Http.Facades;

public sealed partial class ApiFacade
{
    public async Task<DashboardConfigurationResponseDto?> GETApiConfigurationsMeDashboardAsync(CancellationToken cancellationToken = default)
    {
        var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        return await client.GetFromJsonAsync<DashboardConfigurationResponseDto>("api/configurations/me/dashboard", cancellationToken).ConfigureAwait(false);
    }

    public async Task<DashboardConfigurationResponseDto?> PUTApiConfigurationsMeDashboardAsync(UpdateDashboardConfigurationRequestDto request, CancellationToken cancellationToken = default)
    {
        var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        var response = await client.PutAsJsonAsync("api/configurations/me/dashboard", request, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DashboardConfigurationResponseDto>(cancellationToken).ConfigureAwait(false);
    }
}
