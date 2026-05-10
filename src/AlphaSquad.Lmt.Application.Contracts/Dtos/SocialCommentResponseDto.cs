namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class SocialCommentResponseDto
{
    public DateTimeOffset? CreatedAt { get; set; }
    public Guid? Id { get; set; }
    public string? Message { get; set; }
    public string? SocialCommentResponseUsername { get; set; }
    public Guid? SocialPostId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
}