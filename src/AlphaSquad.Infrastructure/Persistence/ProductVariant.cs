namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa uma variante vendavel de um produto, como tamanho ou cor especificos.
/// Esta entidade prepara a loja para estoque, combinacoes comerciais e compra futura.
/// </summary>
public class ProductVariant
{
    /// <summary>
    /// Identificador unico da variante.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant ao qual a variante pertence.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Produto pai ao qual a variante pertence.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Nome curto da variante para exibicao.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Cor da variante, quando aplicavel.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Tamanho da variante, quando aplicavel.
    /// </summary>
    public string? Size { get; set; }

    /// <summary>
    /// Preco praticado para esta variante.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Quantidade de itens disponiveis em estoque.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Indica se a variante pode aparecer no catalogo e ser comprada.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Data de criacao da variante.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
