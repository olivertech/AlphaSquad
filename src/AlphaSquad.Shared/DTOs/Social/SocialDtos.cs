namespace AlphaSquad.Shared.DTOs.Social;

using AlphaSquad.Shared.DTOs.Common;

/// <summary>
/// Payload para criacao de uma nova publicacao social.
/// O post pode ter descricao, midia, ou ambos.
/// </summary>
public record CreateSocialPostRequest(
    string? Description,
    Guid? MediaId
);

/// <summary>
/// Payload para comentario simples em uma publicacao social.
/// </summary>
public record CreateSocialCommentRequest(
    string Message
);

/// <summary>
/// DTO de resposta principal do feed social.
/// Reune dados do autor, midia, interacoes e estado da curtida para o usuario atual.
/// </summary>
public record SocialPostResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    string? Username,
    string? ProfilePhotoUrl,
    string? Description,
    Guid? MediaId,
    string? MediaUrl,
    int LikesCount,
    int CommentsCount,
    bool IsLikedByCurrentUser,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public string StatusCode => "active";

    public MobileCardItemResponse MobileCard => new(
        "social-post",
        UserName,
        BuildSubtitle(),
        Description,
        MediaUrl,
        MediaUrl,
        BuildBadge(),
        StatusCode,
        CreatedAt,
        "social/posts",
        Id,
        MobileContractCodes.BuildRouteHint("social/posts", Id));

    private string? BuildSubtitle()
    {
        return !string.IsNullOrWhiteSpace(Username)
            ? $"@{Username}"
            : null;
    }

    private string? BuildBadge()
    {
        if (CommentsCount > 0)
            return $"{CommentsCount} comentarios";

        if (LikesCount > 0)
            return $"{LikesCount} curtidas";

        return null;
    }
}

/// <summary>
/// DTO de resposta para comentarios simples da rede social.
/// </summary>
public record SocialCommentResponse(
    Guid Id,
    Guid SocialPostId,
    Guid UserId,
    string UserName,
    string? Username,
    string Message,
    DateTime CreatedAt
);
