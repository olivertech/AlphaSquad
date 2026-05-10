using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Http.Mappers;

namespace AlphaSquad.Lmt.Application.Http.Facades;

public sealed partial class ApiFacade
{
    public async Task DELETEApiStoreProductsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        await _apiClient.Api.Store.Products[id].DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

    }

    public async Task DELETEApiStoreProductsByProductIdVariantsByVariantIdAsync(Guid productId, Guid variantId, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        await _apiClient.Api.Store.Products[productId].Variants[variantId].DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

    }

    public async Task<List<StoreOrderListItemResponseDto>?> GETApiStoreOrdersAsync(int? page, int? pageSize, int? status, Guid? userId, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Store.Orders.GetAsync(config =>
        {
            config.QueryParameters.Page = page;
            config.QueryParameters.PageSize = pageSize;
            config.QueryParameters.Status = status;
            config.QueryParameters.UserId = userId;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<StoreOrderListItemResponseDto>(result?.Items);

    }

    public async Task<List<StoreOrderListItemResponseDto>?> GETApiStoreOrdersMeAsync(int? page, int? pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Store.Orders.Me.GetAsync(config =>
        {
            config.QueryParameters.Page = page;
            config.QueryParameters.PageSize = pageSize;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<StoreOrderListItemResponseDto>(result?.Items);

    }

    public async Task<List<StoreOrderItemResponseDto>?> GETApiStoreOrdersMeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        var result = await _apiClient.Api.Store.Orders.Me[id].GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.MapList<StoreOrderItemResponseDto>(result?.Items);

    }

    public async Task<List<ProductListItemResponseDto>?> GETApiStoreProductsAsync(bool? includeInactive, int? page, int? pageSize, string search, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Store.Products.GetAsync(config =>
        {
            config.QueryParameters.IncludeInactive = includeInactive;
            config.QueryParameters.Page = page;
            config.QueryParameters.PageSize = pageSize;
            config.QueryParameters.Search = search;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<ProductListItemResponseDto>(result?.Items);

    }

    public async Task<ProductDetailResponseDto?> GETApiStoreProductsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
#pragma warning disable CS0618
        var result = await _apiClient.Api.Store.Products[id].GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.Map<ProductDetailResponseDto>(result);

    }

    public async Task<List<ProductListItemResponseDto>?> GETApiStoreProductsFeedAsync(string cursor, int? limit, string search, CancellationToken cancellationToken = default)
    {
        var result = await _apiClient.Api.Store.Products.Feed.GetAsync(config =>
        {
            config.QueryParameters.Cursor = cursor;
            config.QueryParameters.Limit = limit;
            config.QueryParameters.Search = search;
        }, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<ProductListItemResponseDto>(result?.Items);

    }

    public async Task<List<StoreOrderItemResponseDto>?> POSTApiStoreOrdersAsync(CreateStoreOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.CreateStoreOrderRequest>(request);

        var result = await _apiClient.Api.Store.Orders.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.MapList<StoreOrderItemResponseDto>(result?.Items);

    }

    public async Task<ProductDetailResponseDto?> POSTApiStoreProductsAsync(CreateProductRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.CreateProductRequest>(request);

        var result = await _apiClient.Api.Store.Products.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

        return GeneratedDtoMapper.Map<ProductDetailResponseDto>(result);

    }

    public async Task<ProductVariantResponseDto?> POSTApiStoreProductsByIdVariantsAsync(Guid id, CreateProductVariantRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.CreateProductVariantRequest>(request);

#pragma warning disable CS0618
        var result = await _apiClient.Api.Store.Products[id].Variants.PostAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.Map<ProductVariantResponseDto>(result);

    }

    public async Task<List<StoreOrderItemResponseDto>?> PUTApiStoreOrdersByIdStatusAsync(string id, UpdateStoreOrderStatusRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.UpdateStoreOrderStatusRequest>(request);

#pragma warning disable CS0618
        var result = await _apiClient.Api.Store.Orders[id].Status.PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.MapList<StoreOrderItemResponseDto>(result?.Items);

    }

    public async Task<ProductDetailResponseDto?> PUTApiStoreProductsByIdAsync(Guid id, UpdateProductRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.UpdateProductRequest>(request);

#pragma warning disable CS0618
        var result = await _apiClient.Api.Store.Products[id].PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.Map<ProductDetailResponseDto>(result);

    }

    public async Task<ProductVariantResponseDto?> PUTApiStoreProductsByProductIdVariantsByVariantIdAsync(Guid productId, Guid variantId, UpdateProductVariantRequestDto request, CancellationToken cancellationToken = default)
    {
        var kiotaRequest = GeneratedDtoMapper.MapRequired<AlphaSquad.Lmt.Application.ApiClient.Models.UpdateProductVariantRequest>(request);

#pragma warning disable CS0618
        var result = await _apiClient.Api.Store.Products[productId].Variants[variantId].PutAsync(kiotaRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618

        return GeneratedDtoMapper.Map<ProductVariantResponseDto>(result);

    }
}