namespace AlphaSquad.Shared.DTOs.Store;

using System.Globalization;
using AlphaSquad.Shared.DTOs.Common;

/// <summary>
/// Resposta resumida usada nas listagens do catalogo da loja.
/// </summary>
public record ProductListItemResponse(
    Guid Id,
    string Name,
    string? Description,
    string? MainMediaUrl,
    decimal? StartingPrice,
    bool IsActive,
    int DisplayOrder,
    DateTime CreatedAt
)
{
    public string StatusCode => MobileContractCodes.FromBoolean(IsActive, "active", "inactive");

    public MobileCardItemResponse MobileCard => new(
        "store-product",
        Name,
        BuildSubtitle(),
        Description,
        MainMediaUrl,
        MainMediaUrl,
        BuildBadge(),
        StatusCode,
        CreatedAt,
        "store/products",
        Id,
        MobileContractCodes.BuildRouteHint("store/products", Id));

    private string? BuildSubtitle()
    {
        return StartingPrice.HasValue
            ? $"a partir de R$ {StartingPrice.Value.ToString("0.00", CultureInfo.InvariantCulture)}"
            : null;
    }

    private string? BuildBadge()
    {
        return IsActive ? "pedido presencial" : "indisponivel";
    }
}

/// <summary>
/// Resposta detalhada do produto, incluindo suas variantes vendaveis.
/// </summary>
public record ProductDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? MainMediaId,
    string? MainMediaUrl,
    bool IsActive,
    int DisplayOrder,
    DateTime CreatedAt,
    List<ProductVariantResponse> Variants
);

/// <summary>
/// Representa uma variante de produto retornada pela API.
/// </summary>
public record ProductVariantResponse(
    Guid Id,
    Guid ProductId,
    string Name,
    string? Color,
    string? Size,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAt
);

/// <summary>
/// Payload para criar um produto na loja do tenant.
/// </summary>
public record CreateProductRequest(
    string Name,
    string? Description,
    Guid? MainMediaId,
    int DisplayOrder
);

/// <summary>
/// Payload para atualizar um produto existente.
/// </summary>
public record UpdateProductRequest(
    string Name,
    string? Description,
    Guid? MainMediaId,
    bool IsActive,
    int DisplayOrder
);

/// <summary>
/// Payload para criar uma nova variante de um produto.
/// </summary>
public record CreateProductVariantRequest(
    string Name,
    string? Color,
    string? Size,
    decimal Price,
    int StockQuantity
);

/// <summary>
/// Payload para atualizar uma variante existente.
/// </summary>
public record UpdateProductVariantRequest(
    string Name,
    string? Color,
    string? Size,
    decimal Price,
    int StockQuantity,
    bool IsActive
);

/// <summary>
/// Item enviado pelo app para montar um novo pedido.
/// </summary>
public record CreateStoreOrderItemRequest(
    Guid ProductId,
    Guid ProductVariantId,
    int Quantity
);

/// <summary>
/// Payload para criacao de pedido pelo usuario autenticado.
/// </summary>
public record CreateStoreOrderRequest(
    List<CreateStoreOrderItemRequest> Items,
    string? CustomerNotes
);

/// <summary>
/// Payload administrativo para alteracao de status do pedido.
/// </summary>
public record UpdateStoreOrderStatusRequest(
    StoreOrderStatus Status,
    string? AdminNotes
);

/// <summary>
/// Resposta resumida usada nas listagens de pedidos.
/// </summary>
public record StoreOrderListItemResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    StoreOrderStatus Status,
    decimal TotalAmount,
    int TotalItems,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

/// <summary>
/// Resposta detalhada do pedido da loja.
/// </summary>
public record StoreOrderDetailResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    string UserEmail,
    StoreOrderStatus Status,
    decimal TotalAmount,
    string? CustomerNotes,
    string? AdminNotes,
    Guid? LastUpdatedByUserId,
    string? LastUpdatedByUserName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<StoreOrderItemResponse> Items
);

/// <summary>
/// Resposta de item do pedido com snapshot comercial do momento da compra.
/// </summary>
public record StoreOrderItemResponse(
    Guid Id,
    Guid ProductId,
    Guid ProductVariantId,
    string ProductName,
    string VariantName,
    string? VariantColor,
    string? VariantSize,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);
