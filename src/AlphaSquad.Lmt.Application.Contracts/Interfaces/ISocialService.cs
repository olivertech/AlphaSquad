using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface ISocialService
{
    Task DELETEApiSocialPostsByIdLikeAsync(string id, CancellationToken cancellationToken = default);

    Task<List<SocialPostResponseDto>?> GETApiSocialPostsAsync(int? page, int? pageSize, CancellationToken cancellationToken = default);

    Task<SocialPostResponseDto?> GETApiSocialPostsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<SocialCommentResponseDto>?> GETApiSocialPostsByIdCommentsAsync(string id, CancellationToken cancellationToken = default);

    Task<List<SocialPostResponseDto>?> GETApiSocialPostsFeedAsync(string cursor, int? limit, CancellationToken cancellationToken = default);

    Task<SocialResponseDto?> POSTApiSocialPostsAsync(CreateSocialPostRequestDto request, CancellationToken cancellationToken = default);

    Task<SocialCommentResponseDto?> POSTApiSocialPostsByIdCommentsAsync(string id, CreateSocialCommentRequestDto request, CancellationToken cancellationToken = default);

    Task POSTApiSocialPostsByIdLikeAsync(string id, CancellationToken cancellationToken = default);
}