namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class TenantLegalContentResponseDto
{
    public DateTimeOffset? CreatedAt { get; set; }
    public Guid? Id { get; set; }
    public string? PrivacyPolicy { get; set; }
    public Guid? TenantId { get; set; }
    public string? TermsOfUse { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public string? UpdatedByUserName { get; set; }
}