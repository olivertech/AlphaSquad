using System;
using System.Net.Http;
using AlphaSquad.Lmt.Application.ApiClient;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Facades;

public sealed partial class ApiFacade : IApiFacade
{
    private readonly global::AlphaSquad.Lmt.Application.ApiClient.AlphaSquadLmtApplicationApiClient _apiClient;
    private readonly IHttpClientFactory _httpClientFactory;

    public ApiFacade(
        global::AlphaSquad.Lmt.Application.ApiClient.AlphaSquadLmtApplicationApiClient apiClient,
        IHttpClientFactory httpClientFactory)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }
}