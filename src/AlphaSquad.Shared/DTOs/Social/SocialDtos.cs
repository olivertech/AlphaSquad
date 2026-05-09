namespace AlphaSquad.Shared.DTOs.Social;

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
);

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
