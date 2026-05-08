namespace AlphaSquad.Infrastructure.Persistence;

/// <summary>
/// Representa um item do pedido com snapshot comercial do momento da compra.
/// O snapshot evita distorcoes futuras caso produto, variante ou preco mudem depois.
/// </summary>
public class StoreOrderItem
{
    /// <summary>
    /// Identificador unico do item do pedido.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant ao qual o item pertence.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Pedido pai ao qual este item pertence.
    /// </summary>
    public Guid StoreOrderId { get; set; }

    /// <summary>
    /// Produto original escolhido pelo usuario.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Variante selecionada no pedido.
    /// </summary>
    public Guid ProductVariantId { get; set; }

    /// <summary>
    /// Nome do produto no momento da compra.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Nome da variante no momento da compra.
    /// </summary>
    public string VariantName { get; set; } = string.Empty;

    /// <summary>
    /// Cor da variante no momento da compra.
    /// </summary>
    public string? VariantColor { get; set; }

    /// <summary>
    /// Tamanho da variante no momento da compra.
    /// </summary>
    public string? VariantSize { get; set; }

    /// <summary>
    /// Quantidade solicitada pelo usuario.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Preco unitario da variante no momento da compra.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Valor total do item.
    /// </summary>
    public decimal LineTotal { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public StoreOrder StoreOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}
