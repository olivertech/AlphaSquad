using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class StoreService : IStoreService
{
    private readonly IApiFacade _apiFacade;

    public StoreService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task DELETEApiStoreProductsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.DELETEApiStoreProductsByIdAsync(id, cancellationToken);
    }

    public Task DELETEApiStoreProductsByProductIdVariantsByVariantIdAsync(Guid productId, Guid variantId, CancellationToken cancellationToken = default)
    {
        return _apiFacade.DELETEApiStoreProductsByProductIdVariantsByVariantIdAsync(productId, variantId, cancellationToken);
    }

    public Task<List<StoreOrderListItemResponseDto>?> GETApiStoreOrdersAsync(int? page, int? pageSize, int? status, Guid? userId, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiStoreOrdersAsync(page, pageSize, status, userId, cancellationToken);
    }

    public Task<List<StoreOrderListItemResponseDto>?> GETApiStoreOrdersMeAsync(int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiStoreOrdersMeAsync(page, pageSize, cancellationToken);
    }

    public Task<List<StoreOrderItemResponseDto>?> GETApiStoreOrdersMeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiStoreOrdersMeByIdAsync(id, cancellationToken);
    }

    public Task<List<ProductListItemResponseDto>?> GETApiStoreProductsAsync(bool? includeInactive, int? page, int? pageSize, string search, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiStoreProductsAsync(includeInactive, page, pageSize, search, cancellationToken);
    }

    public Task<ProductDetailResponseDto?> GETApiStoreProductsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiStoreProductsByIdAsync(id, cancellationToken);
    }

    public Task<List<ProductListItemResponseDto>?> GETApiStoreProductsFeedAsync(string cursor, int? limit, string search, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiStoreProductsFeedAsync(cursor, limit, search, cancellationToken);
    }

    public Task<List<StoreOrderItemResponseDto>?> POSTApiStoreOrdersAsync(CreateStoreOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiStoreOrdersAsync(request, cancellationToken);
    }

    public Task<ProductDetailResponseDto?> POSTApiStoreProductsAsync(CreateProductRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiStoreProductsAsync(request, cancellationToken);
    }

    public Task<ProductVariantResponseDto?> POSTApiStoreProductsByIdVariantsAsync(Guid id, CreateProductVariantRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiStoreProductsByIdVariantsAsync(id, request, cancellationToken);
    }

    public Task<List<StoreOrderItemResponseDto>?> PUTApiStoreOrdersByIdStatusAsync(string id, UpdateStoreOrderStatusRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiStoreOrdersByIdStatusAsync(id, request, cancellationToken);
    }

    public Task<ProductDetailResponseDto?> PUTApiStoreProductsByIdAsync(Guid id, UpdateProductRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiStoreProductsByIdAsync(id, request, cancellationToken);
    }

    public Task<ProductVariantResponseDto?> PUTApiStoreProductsByProductIdVariantsByVariantIdAsync(Guid productId, Guid variantId, UpdateProductVariantRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiStoreProductsByProductIdVariantsByVariantIdAsync(productId, variantId, request, cancellationToken);
    }
}