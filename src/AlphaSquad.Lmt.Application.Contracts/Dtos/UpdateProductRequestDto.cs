namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class UpdateProductRequestDto
{
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
    public Guid? MainMediaId { get; set; }
    public string? Name { get; set; }
}