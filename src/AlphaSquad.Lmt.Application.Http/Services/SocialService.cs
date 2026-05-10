using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class SocialService : ISocialService
{
    private readonly IApiFacade _apiFacade;

    public SocialService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task DELETEApiSocialPostsByIdLikeAsync(string id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.DELETEApiSocialPostsByIdLikeAsync(id, cancellationToken);
    }

    public Task<List<SocialPostResponseDto>?> GETApiSocialPostsAsync(int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiSocialPostsAsync(page, pageSize, cancellationToken);
    }

    public Task<SocialPostResponseDto?> GETApiSocialPostsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiSocialPostsByIdAsync(id, cancellationToken);
    }

    public Task<List<SocialCommentResponseDto>?> GETApiSocialPostsByIdCommentsAsync(string id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiSocialPostsByIdCommentsAsync(id, cancellationToken);
    }

    public Task<List<SocialPostResponseDto>?> GETApiSocialPostsFeedAsync(string cursor, int? limit, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiSocialPostsFeedAsync(cursor, limit, cancellationToken);
    }

    public Task<SocialResponseDto?> POSTApiSocialPostsAsync(CreateSocialPostRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiSocialPostsAsync(request, cancellationToken);
    }

    public Task<SocialCommentResponseDto?> POSTApiSocialPostsByIdCommentsAsync(string id, CreateSocialCommentRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiSocialPostsByIdCommentsAsync(id, request, cancellationToken);
    }

    public Task POSTApiSocialPostsByIdLikeAsync(string id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiSocialPostsByIdLikeAsync(id, cancellationToken);
    }
}