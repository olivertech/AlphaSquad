using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Http.Mappers;

namespace AlphaSquad.Lmt.Application.Http.Facades;

public sealed partial class ApiFacade
{
    public async Task DELETEApiMediaByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        await _apiClient.Api.Media[id].DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

    }

    public async Task<List<TenantMediaResponseDto>?> GETApiMediaAsync(int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Media.GetAsync(config =>
        {
            config.QueryParameters.Page = page;
            config.QueryParameters.PageSize = pageSize;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<TenantMediaResponseDto>(result?.Items);

    }

    public async Task<MediaResponseDto?> GETApiMediaByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        var result = await _apiClient.Api.Media[id].GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.Map<MediaResponseDto>(result);

    }

    public async Task<UploadMediaResponseDto?> POSTApiMediaUploadAsync(MultipartBodyDto request, CancellationToken cancellationToken = default)
    {
        using var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(request.Content);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ContentType);
        content.Add(fileContent, "file", request.FileName);

        var response = await client.PostAsync("/api/media/upload", content, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return System.Text.Json.JsonSerializer.Deserialize<UploadMediaResponseDto>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<TenantMediaResponseDto?> PUTApiMediaByIdFileAsync(string id, MultipartBodyDto request, CancellationToken cancellationToken = default)
    {
        using var client = await CreateAuthenticatedClientAsync(cancellationToken).ConfigureAwait(false);
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(request.Content);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ContentType);
        content.Add(fileContent, "file", request.FileName);

        var response = await client.PutAsync($"/api/media/{id}/file", content, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return System.Text.Json.JsonSerializer.Deserialize<TenantMediaResponseDto>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}