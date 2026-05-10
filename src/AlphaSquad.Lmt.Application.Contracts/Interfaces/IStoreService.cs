using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface IStoreService
{
    Task DELETEApiStoreProductsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task DELETEApiStoreProductsByProductIdVariantsByVariantIdAsync(Guid productId, Guid variantId, CancellationToken cancellationToken = default);

    Task<List<StoreOrderListItemResponseDto>?> GETApiStoreOrdersAsync(int? page, int? pageSize, int? status, Guid? userId, CancellationToken cancellationToken = default);

    Task<List<StoreOrderListItemResponseDto>?> GETApiStoreOrdersMeAsync(int? page, int? pageSize, CancellationToken cancellationToken = default);

    Task<List<StoreOrderItemResponseDto>?> GETApiStoreOrdersMeByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<ProductListItemResponseDto>?> GETApiStoreProductsAsync(bool? includeInactive, int? page, int? pageSize, string search, CancellationToken cancellationToken = default);

    Task<ProductDetailResponseDto?> GETApiStoreProductsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<ProductListItemResponseDto>?> GETApiStoreProductsFeedAsync(string cursor, int? limit, string search, CancellationToken cancellationToken = default);

    Task<List<StoreOrderItemResponseDto>?> POSTApiStoreOrdersAsync(CreateStoreOrderRequestDto request, CancellationToken cancellationToken = default);

    Task<ProductDetailResponseDto?> POSTApiStoreProductsAsync(CreateProductRequestDto request, CancellationToken cancellationToken = default);

    Task<ProductVariantResponseDto?> POSTApiStoreProductsByIdVariantsAsync(Guid id, CreateProductVariantRequestDto request, CancellationToken cancellationToken = default);

    Task<List<StoreOrderItemResponseDto>?> PUTApiStoreOrdersByIdStatusAsync(string id, UpdateStoreOrderStatusRequestDto request, CancellationToken cancellationToken = default);

    Task<ProductDetailResponseDto?> PUTApiStoreProductsByIdAsync(Guid id, UpdateProductRequestDto request, CancellationToken cancellationToken = default);

    Task<ProductVariantResponseDto?> PUTApiStoreProductsByProductIdVariantsByVariantIdAsync(Guid productId, Guid variantId, UpdateProductVariantRequestDto request, CancellationToken cancellationToken = default);
}