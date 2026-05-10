namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UpdateTenantLegalContentRequestDto
{
    public string? PrivacyPolicy { get; set; }
    public string? TermsOfUse { get; set; }
}