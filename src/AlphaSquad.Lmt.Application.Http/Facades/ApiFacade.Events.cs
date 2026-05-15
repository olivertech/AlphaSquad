using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Http.Facades;

public sealed partial class ApiFacade
{
    public async Task DELETEApiEventsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        await _apiClient.Api.Events[id].DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618
    }

    public async Task<List<AcademyEventResponseDto>?> GETApiEventsAsync(bool? isActive, bool? onlyOutdoor, int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        using var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        var url = BuildEventsUrl("api/events", new Dictionary<string, string?>
        {
            ["isActive"] = isActive?.ToString()?.ToLowerInvariant(),
            ["onlyOutdoor"] = onlyOutdoor?.ToString()?.ToLowerInvariant(),
            ["page"] = page?.ToString(),
            ["pageSize"] = pageSize?.ToString()
        });
        var result = await client.GetFromJsonAsync<PagedEnvelope<AcademyEventResponseDto>>(url, cancellationToken).ConfigureAwait(false);

        return result?.Items;
    }

    public async Task<AcademyEventResponseDto?> GETApiEventsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        return await client.GetFromJsonAsync<AcademyEventResponseDto>($"api/events/{id}", cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<AcademyEventResponseDto>?> GETApiEventsFeedAsync(string cursor, int? limit, bool? onlyOutdoor, CancellationToken cancellationToken = default)
    {
        using var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        var url = BuildEventsUrl("api/events/feed", new Dictionary<string, string?>
        {
            ["cursor"] = cursor,
            ["limit"] = limit?.ToString(),
            ["onlyOutdoor"] = onlyOutdoor?.ToString()?.ToLowerInvariant()
        });
        var result = await client.GetFromJsonAsync<CursorFeedEnvelope<AcademyEventResponseDto>>(url, cancellationToken).ConfigureAwait(false);

        return result?.Items;
    }

    public async Task<AcademyEventResponseDto?> POSTApiEventsAsync(CreateAcademyEventRequestDto request, CancellationToken cancellationToken = default)
    {
        using var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        var response = await client.PostAsJsonAsync("api/events", request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AcademyEventResponseDto>(cancellationToken).ConfigureAwait(false);
    }

    public async Task<AcademyEventParticipationResponseDto?> POSTApiEventsByIdCheckinAsync(Guid id, EventCheckInRequestDto request, CancellationToken cancellationToken = default)
    {
        using var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        var response = await client.PostAsJsonAsync($"api/events/{id}/checkin", request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AcademyEventParticipationResponseDto>(cancellationToken).ConfigureAwait(false);
    }

    public async Task<CompleteAcademyEventResponseDto?> POSTApiEventsByIdCompleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        var response = await client.PostAsync($"api/events/{id}/complete", content: null, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CompleteAcademyEventResponseDto>(cancellationToken).ConfigureAwait(false);
    }

    public async Task<AcademyEventParticipationResponseDto?> POSTApiEventsByIdParticipateAsync(string id, CancellationToken cancellationToken = default)
    {
        using var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        var response = await client.PostAsync($"api/events/{id}/participate", content: null, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AcademyEventParticipationResponseDto>(cancellationToken).ConfigureAwait(false);
    }

    public async Task<AcademyEventResponseDto?> PUTApiEventsByIdAsync(Guid id, UpdateAcademyEventRequestDto request, CancellationToken cancellationToken = default)
    {
        using var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        var response = await client.PutAsJsonAsync($"api/events/{id}", request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AcademyEventResponseDto>(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<AcademyEventParticipationResponseDto>?> GETApiEventsByIdParticipantsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        return await client.GetFromJsonAsync<List<AcademyEventParticipationResponseDto>>($"api/events/{id}/participants", cancellationToken).ConfigureAwait(false);
    }

    public async Task<AcademyEventParticipationResponseDto?> POSTApiEventsByIdParticipantsByUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        using var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        var response = await client.PostAsync($"api/events/{id}/participants/{userId}", content: null, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AcademyEventParticipationResponseDto>(cancellationToken).ConfigureAwait(false);
    }

    public async Task DELETEApiEventsByIdParticipantsByUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        using var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        var response = await client.DeleteAsync($"api/events/{id}/participants/{userId}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    private static string BuildEventsUrl(string basePath, IReadOnlyDictionary<string, string?> queryParameters)
    {
        var parts = queryParameters
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}")
            .ToList();

        if (parts.Count == 0)
            return basePath;

        var builder = new StringBuilder(basePath);
        builder.Append('?');
        builder.Append(string.Join("&", parts));
        return builder.ToString();
    }

    private sealed class PagedEnvelope<TItem>
    {
        public List<TItem> Items { get; set; } = [];
    }

    private sealed class CursorFeedEnvelope<TItem>
    {
        public List<TItem> Items { get; set; } = [];
    }
}
