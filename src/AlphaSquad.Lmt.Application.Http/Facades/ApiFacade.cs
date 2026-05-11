using System;
using System.Net.Http;
using System.Net.Http.Headers;
using AlphaSquad.Lmt.Application.ApiClient;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Facades;

public sealed partial class ApiFacade : IApiFacade
{
    private readonly global::AlphaSquad.Lmt.Application.ApiClient.AlphaSquadLmtApplicationApiClient _apiClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAccessTokenAccessor _accessTokenAccessor;

    public ApiFacade(
        global::AlphaSquad.Lmt.Application.ApiClient.AlphaSquadLmtApplicationApiClient apiClient,
        IHttpClientFactory httpClientFactory,
        IAccessTokenAccessor accessTokenAccessor)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _accessTokenAccessor = accessTokenAccessor ?? throw new ArgumentNullException(nameof(accessTokenAccessor));
    }

    /// <summary>
    /// Cria um HttpClient autenticado para endpoints ainda nao cobertos pelo client gerado.
    /// Isso permite evoluir a camada Application sem acoplar a Web diretamente na API.
    /// </summary>
    private async Task<HttpClient> CreateAuthenticatedClientAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("GeneratedApi");
        var accessToken = await _accessTokenAccessor.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(accessToken))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return client;
    }
}
