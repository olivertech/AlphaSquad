namespace AlphaSquad.Lmt.Application.Contracts.Dtos;

public sealed class TenantFeaturesResponseDto
{
    /// <summary>
    /// Lista os módulos liberados para a academia autenticada.
    /// O dashboard usa essa coleção para decidir o que deve aparecer no menu lateral.
    /// </summary>
    public List<FeatureItemDto> Features { get; set; } = [];

    /// <summary>
    /// Representa uma feature individual retornada pelo backend.
    /// O nome funciona como o código estável do módulo e a descrição ajuda em futuras mensagens de apoio.
    /// </summary>
    public sealed class FeatureItemDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
