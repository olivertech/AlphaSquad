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
    public async Task DELETEApiSocialPostsByIdLikeAsync(string id, CancellationToken cancellationToken = default)
    {
        await _apiClient.Api.Social.Posts[ParseRequiredGuid(id, nameof(id))].Like.DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

    }

    public async Task<List<SocialPostResponseDto>?> GETApiSocialPostsAsync(int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Social.Posts.GetAsync(config =>
        {
            config.QueryParameters.Page = page;
            config.QueryParameters.PageSize = pageSize;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<SocialPostResponseDto>(result?.Items);

    }

    public async Task<SocialPostResponseDto?> GETApiSocialPostsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Social.Posts[id].GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<SocialPostResponseDto>(result);

    }

    public async Task<List<SocialCommentResponseDto>?> GETApiSocialPostsByIdCommentsAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Social.Posts[ParseRequiredGuid(id, nameof(id))].Comments.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<SocialCommentResponseDto>(result);

    }

    public async Task<List<SocialPostResponseDto>?> GETApiSocialPostsFeedAsync(string cursor, int? limit, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Social.Posts.Feed.GetAsync(config =>
        {
            config.QueryParameters.Cursor = cursor;
            config.QueryParameters.Limit = limit;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<SocialPostResponseDto>(result?.Items);

    }

    public async Task<SocialResponseDto?> POSTApiSocialPostsAsync(CreateSocialPostRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.CreateSocialPostRequest>(request);

        var result = await _apiClient.Api.Social.Posts.PostAsSocialPostResponseAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<SocialResponseDto>(result);

    }

    public async Task<SocialCommentResponseDto?> POSTApiSocialPostsByIdCommentsAsync(string id, CreateSocialCommentRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.CreateSocialCommentRequest>(request);

        var result = await _apiClient.Api.Social.Posts[ParseRequiredGuid(id, nameof(id))].Comments.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<SocialCommentResponseDto>(result);

    }

    public async Task POSTApiSocialPostsByIdLikeAsync(string id, CancellationToken cancellationToken = default)
    {
        await _apiClient.Api.Social.Posts[ParseRequiredGuid(id, nameof(id))].Like.PostAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

    }
}
