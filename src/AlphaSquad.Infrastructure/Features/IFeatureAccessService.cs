namespace AlphaSquad.Infrastructure.Features;

/// <summary>
/// Contrato para verificar se uma feature opcional esta habilitada para um tenant.
/// Essa camada permite tratar modulos comercializados por pacote sem espalhar a regra pela API.
/// </summary>
public interface IFeatureAccessService
{
    Task<bool> HasFeatureAsync(Guid tenantId, string featureCode);
}
