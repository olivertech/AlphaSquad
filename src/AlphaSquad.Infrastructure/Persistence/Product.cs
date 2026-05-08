namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa um produto comercializado pela academia dentro da loja interna do tenant.
/// O produto concentra os dados principais exibidos no catalogo e pode possuir varias variantes.
/// </summary>
public class Product
{
    /// <summary>
    /// Identificador unico do produto.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant proprietario do produto.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Nome comercial do produto.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descricao opcional apresentada no catalogo e no detalhe.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Midia principal usada como imagem de capa do produto.
    /// </summary>
    public Guid? MainMediaId { get; set; }

    /// <summary>
    /// Indica se o produto esta disponivel para listagem e compra.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Campo opcional para ordenacao manual dentro do catalogo.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Data de criacao do produto.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public TenantMedia? MainMedia { get; set; }
    public ICollection<ProductVariant> Variants { get; set; } = [];
}
