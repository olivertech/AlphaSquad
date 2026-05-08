namespace AlphaSquad.Shared.DTOs.Store;

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
);

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
