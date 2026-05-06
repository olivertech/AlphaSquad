namespace AlphaSquad.Shared.Contracts;

/// <summary>
/// Requisição para atualização da logo do Tenant.
/// Nota: O arquivo é enviado via multipart/form-data, portanto este objeto
/// serve para capturar metadados adicionais se necessário.
/// </summary>
public class UpdateTenantLogoRequest
{
    public string FileName { get; set; } = string.Empty;
}
