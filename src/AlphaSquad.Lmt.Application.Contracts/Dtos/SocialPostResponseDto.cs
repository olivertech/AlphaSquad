namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class SocialPostResponseDto
{
    public int? CommentsCount { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public string? Description { get; set; }
    public Guid? Id { get; set; }
    public bool? IsLikedByCurrentUser { get; set; }
    public int? LikesCount { get; set; }
    public Guid? MediaId { get; set; }
    public string? MediaUrl { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? SocialPostResponseUsername { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
}