namespace AlphaSquad.Api.Features.Store;

/// <summary>
/// Registra os endpoints do modulo de loja interna da academia.
/// Este primeiro recorte cobre o catalogo de produtos e a gestao basica de variantes.
/// </summary>
public static class StoreEndpoints
{
    public static IEndpointRouteBuilder MapStoreEndpoints(this IEndpointRouteBuilder app)
    {
        var productGroup = app.MapGroup("/api/store/products")
            .WithTags("Store")
            .RequireAuthorization();

        productGroup.MapGet("/", GetAllAsync)
            .WithName("GetStoreProducts")
            .WithSummary("Lista os produtos da loja do tenant atual.")
            .WithDescription("Retorna um catalogo paginado de produtos da academia, com filtro por status e busca simples por nome.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        productGroup.MapGet("/feed", GetFeedAsync)
            .WithName("GetStoreProductsFeed")
            .WithSummary("Lista os produtos da loja em formato cursor-based.")
            .WithDescription("Retorna o catalogo da loja em formato cursor-based para scroll infinito no app.")
            .Produces<CursorFeedResponse<ProductListItemResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        productGroup.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetStoreProductById")
            .WithSummary("Retorna o detalhe de um produto da loja.")
            .WithDescription("Busca um produto especifico do tenant atual com suas variantes ativas ou administrativas, conforme o contexto do endpoint.")
            .Produces<ProductDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        productGroup.MapPost("/", CreateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("CreateStoreProduct")
            .WithSummary("Cria um novo produto na loja do tenant.")
            .WithDescription("Cadastra um produto da academia com nome, descricao, imagem principal opcional e ordenacao de exibicao.")
            .Produces<ProductDetailResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        productGroup.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("UpdateStoreProduct")
            .WithSummary("Atualiza um produto da loja do tenant.")
            .WithDescription("Permite alterar os dados basicos, a imagem principal e o status ativo do produto.")
            .Produces<ProductDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        productGroup.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("DeleteStoreProduct")
            .WithSummary("Remove um produto da loja do tenant.")
            .WithDescription("Exclui fisicamente um produto e suas variantes, restrito a perfis administrativos e de gestao.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        productGroup.MapPost("/{id:guid}/variants", CreateVariantAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("CreateStoreProductVariant")
            .WithSummary("Cria uma variante para um produto da loja.")
            .WithDescription("Adiciona uma nova variante de tamanho, cor, preco e estoque para um produto existente do tenant.")
            .Produces<ProductVariantResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        productGroup.MapPut("/{productId:guid}/variants/{variantId:guid}", UpdateVariantAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("UpdateStoreProductVariant")
            .WithSummary("Atualiza uma variante de produto da loja.")
            .WithDescription("Permite alterar os dados comerciais e o status de uma variante de produto existente.")
            .Produces<ProductVariantResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        productGroup.MapDelete("/{productId:guid}/variants/{variantId:guid}", DeleteVariantAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("DeleteStoreProductVariant")
            .WithSummary("Remove uma variante de produto da loja.")
            .WithDescription("Exclui fisicamente uma variante de um produto do tenant atual.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        var orderGroup = app.MapGroup("/api/store/orders")
            .WithTags("Store")
            .RequireAuthorization();

        orderGroup.MapPost("/", CreateOrderAsync)
            .WithName("CreateStoreOrder")
            .WithSummary("Cria um novo pedido da loja para retirada presencial.")
            .WithDescription("Permite que o usuario autenticado monte o pedido pelo app, deixando a separacao, retirada e pagamento para a administracao da academia.")
            .Produces<StoreOrderDetailResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        orderGroup.MapGet("/me", GetMyOrdersAsync)
            .WithName("GetMyStoreOrders")
            .WithSummary("Lista os pedidos do usuario autenticado.")
            .WithDescription("Retorna o historico paginado de pedidos realizados pelo proprio usuario na loja do tenant.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        orderGroup.MapGet("/me/{id:guid}", GetMyOrderByIdAsync)
            .WithName("GetMyStoreOrderById")
            .WithSummary("Retorna o detalhe de um pedido do usuario autenticado.")
            .WithDescription("Busca um pedido especifico do proprio usuario, incluindo itens, status e observacoes administrativas.")
            .Produces<StoreOrderDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        orderGroup.MapGet("/", GetTenantOrdersAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("GetTenantStoreOrders")
            .WithSummary("Lista os pedidos da loja do tenant atual.")
            .WithDescription("Retorna a visao administrativa dos pedidos da loja, com filtros por usuario e status.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        orderGroup.MapPut("/{id:guid}/status", UpdateOrderStatusAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("UpdateStoreOrderStatus")
            .WithSummary("Atualiza o status operacional de um pedido da loja.")
            .WithDescription("Permite separar itens, liberar retirada, registrar pagamento local e cancelar pedidos, ajustando estoque quando necessario.")
            .Produces<StoreOrderDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    /// <summary>
    /// Lista o catalogo da loja com paginacao simples e filtros basicos.
    /// Usuarios comuns veem apenas produtos ativos; perfis de gestao podem optar por incluir inativos.
    /// </summary>
    private static async Task<IResult> GetAllAsync(AppDbContext db,
                                                   HttpContext context,
                                                   string? search = null,
                                                   bool includeInactive = false,
                                                   int page = 1,
                                                   int pageSize = 20)
    {
        var tenantId = context.GetTenantId();
        var userRole = GetUserRole(context.User);
        var canManageStore = userRole is UserRole.Admin or UserRole.Teacher;
        var effectiveIncludeInactive = canManageStore && includeInactive;

        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var connection = db.Database.GetDbConnection();
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";

        const string countSql = @"SELECT COUNT(*)
                                  FROM products p
                                  WHERE p.tenant_id = @TenantId
                                    AND (@IncludeInactive = true OR p.is_active = true)
                                    AND (@Search IS NULL OR p.name ILIKE @Search)";

        const string itemsSql = @"SELECT p.id,
                                         p.name,
                                         p.description,
                                         m.url AS MainMediaUrl,
                                         (
                                             SELECT MIN(v.price)
                                             FROM product_variants v
                                             WHERE v.product_id = p.id
                                               AND v.tenant_id = p.tenant_id
                                               AND v.is_active = true
                                         ) AS StartingPrice,
                                         p.is_active AS IsActive,
                                         p.display_order AS DisplayOrder,
                                         p.created_at AS CreatedAt
                                  FROM products p
                                  LEFT JOIN tenant_medias m
                                    ON m.id = p.main_media_id
                                   AND m.tenant_id = p.tenant_id
                                  WHERE p.tenant_id = @TenantId
                                    AND (@IncludeInactive = true OR p.is_active = true)
                                    AND (@Search IS NULL OR p.name ILIKE @Search)
                                  ORDER BY p.display_order ASC, p.created_at DESC
                                  LIMIT @Limit OFFSET @Offset";

        var parameters = new
        {
            TenantId = tenantId,
            IncludeInactive = effectiveIncludeInactive,
            Search = normalizedSearch,
            Limit = pageSize,
            Offset = (page - 1) * pageSize
        };

        var total = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<ProductListItemResponse>(itemsSql, parameters);

        return Results.Ok(new
        {
            page,
            pageSize,
            total,
            items
        });
    }

    /// <summary>
    /// Retorna o catalogo da loja em formato cursor-based para scroll infinito no app.
    /// O contrato foi alinhado ao mesmo padrao usado por `Events` e `Social`.
    /// </summary>
    private static async Task<IResult> GetFeedAsync(AppDbContext db,
                                                    HttpContext context,
                                                    string? search = null,
                                                    string? cursor = null,
                                                    int limit = 20)
    {
        var tenantId = context.GetTenantId();
        var userRole = GetUserRole(context.User);
        var canManageStore = userRole is UserRole.Admin or UserRole.Teacher;
        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";

        limit = limit is < 1 or > 50 ? 20 : limit;
        var cursorData = DecodeProductFeedCursor(cursor);
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT p.id,
                                    p.name,
                                    p.description,
                                    m.url AS MainMediaUrl,
                                    (
                                        SELECT MIN(v.price)
                                        FROM product_variants v
                                        WHERE v.product_id = p.id
                                          AND v.tenant_id = p.tenant_id
                                          AND v.is_active = true
                                    ) AS StartingPrice,
                                    p.is_active AS IsActive,
                                    p.display_order AS DisplayOrder,
                                    p.created_at AS CreatedAt
                             FROM products p
                             LEFT JOIN tenant_medias m
                               ON m.id = p.main_media_id
                              AND m.tenant_id = p.tenant_id
                             WHERE p.tenant_id = @TenantId
                               AND (@CanManageStore = true OR p.is_active = true)
                               AND (@Search IS NULL OR p.name ILIKE @Search)
                               AND (
                                   @CursorDisplayOrder IS NULL
                                   OR p.display_order > @CursorDisplayOrder
                                   OR (
                                       p.display_order = @CursorDisplayOrder
                                       AND (
                                           p.created_at < @CursorCreatedAt
                                           OR (p.created_at = @CursorCreatedAt AND p.id < @CursorId)
                                       )
                                   )
                               )
                             ORDER BY p.display_order ASC, p.created_at DESC, p.id DESC
                             LIMIT @LimitPlusOne";

        var rows = (await connection.QueryAsync<ProductListItemResponse>(sql, new
        {
            TenantId = tenantId,
            CanManageStore = canManageStore,
            Search = normalizedSearch,
            CursorDisplayOrder = cursorData?.DisplayOrder,
            CursorCreatedAt = cursorData?.CreatedAt,
            CursorId = cursorData?.Id,
            LimitPlusOne = limit + 1
        })).ToList();

        var hasMore = rows.Count > limit;
        var items = hasMore ? rows.Take(limit).ToList() : rows;
        string? nextCursor = null;

        if (hasMore && items.Count > 0)
        {
            var lastItem = items[^1];
            nextCursor = EncodeProductFeedCursor(lastItem.DisplayOrder, lastItem.CreatedAt, lastItem.Id);
        }

        return Results.Ok(new CursorFeedResponse<ProductListItemResponse>(items, nextCursor, hasMore));
    }

    /// <summary>
    /// Retorna o detalhe de um produto do tenant atual, incluindo suas variantes.
    /// </summary>
    private static async Task<IResult> GetByIdAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userRole = GetUserRole(context.User);
        var canManageStore = userRole is UserRole.Admin or UserRole.Teacher;

        var response = await BuildProductDetailAsync(id, tenantId, db, includeInactiveVariants: canManageStore, requireActiveProduct: !canManageStore);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    /// <summary>
    /// Cria um novo produto da loja do tenant atual.
    /// </summary>
    private static async Task<IResult> CreateAsync(CreateProductRequest request, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var validation = await ValidateProductRequestAsync(request.Name, request.MainMediaId, tenantId, db);
        if (validation is not null)
            return validation;

        var product = new Product
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Description = NormalizeOptional(request.Description),
            MainMediaId = request.MainMediaId,
            IsActive = true,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        var response = await BuildProductDetailAsync(product.Id, tenantId, db, includeInactiveVariants: true, requireActiveProduct: false);
        return Results.Created($"/api/store/products/{product.Id}", response);
    }

    /// <summary>
    /// Atualiza um produto existente do tenant atual.
    /// </summary>
    private static async Task<IResult> UpdateAsync(Guid id, UpdateProductRequest request, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (product is null)
            return Results.NotFound();

        var validation = await ValidateProductRequestAsync(request.Name, request.MainMediaId, tenantId, db);
        if (validation is not null)
            return validation;

        product.Name = request.Name.Trim();
        product.Description = NormalizeOptional(request.Description);
        product.MainMediaId = request.MainMediaId;
        product.IsActive = request.IsActive;
        product.DisplayOrder = request.DisplayOrder;

        await db.SaveChangesAsync();

        var response = await BuildProductDetailAsync(product.Id, tenantId, db, includeInactiveVariants: true, requireActiveProduct: false);
        return Results.Ok(response);
    }

    /// <summary>
    /// Remove um produto da loja e suas variantes.
    /// </summary>
    private static async Task<IResult> DeleteAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var product = await db.Products
            .Include(x => x.Variants)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (product is null)
            return Results.NotFound();

        db.ProductVariants.RemoveRange(product.Variants);
        db.Products.Remove(product);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    /// <summary>
    /// Cria uma nova variante para um produto do tenant atual.
    /// </summary>
    private static async Task<IResult> CreateVariantAsync(Guid id, CreateProductVariantRequest request, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (product is null)
            return Results.NotFound();

        var validation = ValidateVariantRequest(request.Name, request.Price, request.StockQuantity);
        if (validation is not null)
            return validation;

        var variant = new AlphaSquad.Infrastructure.Persistence.ProductVariant
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductId = id,
            Name = request.Name.Trim(),
            Color = NormalizeOptional(request.Color),
            Size = NormalizeOptional(request.Size),
            Price = decimal.Round(request.Price, 2),
            StockQuantity = request.StockQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync();

        return Results.Created($"/api/store/products/{id}/variants/{variant.Id}", MapVariantResponse(variant));
    }

    /// <summary>
    /// Atualiza uma variante existente de um produto do tenant atual.
    /// </summary>
    private static async Task<IResult> UpdateVariantAsync(Guid productId, Guid variantId, UpdateProductVariantRequest request, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var variant = await db.ProductVariants.FirstOrDefaultAsync(x =>
            x.Id == variantId &&
            x.ProductId == productId &&
            x.TenantId == tenantId);

        if (variant is null)
            return Results.NotFound();

        var validation = ValidateVariantRequest(request.Name, request.Price, request.StockQuantity);
        if (validation is not null)
            return validation;

        variant.Name = request.Name.Trim();
        variant.Color = NormalizeOptional(request.Color);
        variant.Size = NormalizeOptional(request.Size);
        variant.Price = decimal.Round(request.Price, 2);
        variant.StockQuantity = request.StockQuantity;
        variant.IsActive = request.IsActive;

        await db.SaveChangesAsync();

        return Results.Ok(MapVariantResponse(variant));
    }

    /// <summary>
    /// Remove uma variante de produto do tenant atual.
    /// </summary>
    private static async Task<IResult> DeleteVariantAsync(Guid productId, Guid variantId, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var variant = await db.ProductVariants.FirstOrDefaultAsync(x =>
            x.Id == variantId &&
            x.ProductId == productId &&
            x.TenantId == tenantId);

        if (variant is null)
            return Results.NotFound();

        db.ProductVariants.Remove(variant);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    /// <summary>
    /// Cria um pedido pelo app para pagamento presencial na academia.
    /// O estoque nao e abatido neste momento; a reserva acontece quando a administracao confirma o pedido.
    /// </summary>
    private static async Task<IResult> CreateOrderAsync(CreateStoreOrderRequest request, AppDbContext db, HttpContext context)
    {
        if (request.Items is null || request.Items.Count == 0)
            return Results.BadRequest("Order must contain at least one item.");

        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.IsActive);
        if (user is null)
            return Results.Unauthorized();

        var normalizedItems = request.Items
            .GroupBy(x => new { x.ProductId, x.ProductVariantId })
            .Select(group => new
            {
                group.Key.ProductId,
                group.Key.ProductVariantId,
                Quantity = group.Sum(x => x.Quantity)
            })
            .ToList();

        if (normalizedItems.Any(x => x.Quantity <= 0))
            return Results.BadRequest("Item quantity must be greater than zero.");

        var variantIds = normalizedItems.Select(x => x.ProductVariantId).ToList();
        var variants = await db.ProductVariants
            .Include(x => x.Product)
            .Where(x => variantIds.Contains(x.Id) && x.TenantId == tenantId)
            .ToListAsync();

        if (variants.Count != variantIds.Count)
            return Results.BadRequest("One or more variants do not belong to this tenant.");

        var order = new StoreOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            Status = StoreOrderStatus.PendingApproval,
            CustomerNotes = NormalizeOptional(request.CustomerNotes),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var requestedItem in normalizedItems)
        {
            var variant = variants.First(x => x.Id == requestedItem.ProductVariantId);

            if (variant.ProductId != requestedItem.ProductId)
                return Results.BadRequest("Variant does not match the informed product.");

            if (!variant.Product.IsActive || !variant.IsActive)
                return Results.BadRequest("Product or variant is not available for ordering.");

            if (variant.StockQuantity < requestedItem.Quantity)
                return Results.BadRequest($"Insufficient stock for variant {variant.Name}.");

            var lineTotal = decimal.Round(variant.Price * requestedItem.Quantity, 2);
            order.TotalAmount += lineTotal;

            order.Items.Add(new StoreOrderItem
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                StoreOrderId = order.Id,
                ProductId = variant.ProductId,
                ProductVariantId = variant.Id,
                ProductName = variant.Product.Name,
                VariantName = variant.Name,
                VariantColor = variant.Color,
                VariantSize = variant.Size,
                Quantity = requestedItem.Quantity,
                UnitPrice = variant.Price,
                LineTotal = lineTotal
            });
        }

        order.TotalAmount = decimal.Round(order.TotalAmount, 2);

        db.StoreOrders.Add(order);
        await db.SaveChangesAsync();

        var response = await BuildOrderDetailAsync(order.Id, tenantId, db);
        return Results.Created($"/api/store/orders/me/{order.Id}", response);
    }

    /// <summary>
    /// Lista os pedidos do proprio usuario.
    /// </summary>
    private static async Task<IResult> GetMyOrdersAsync(AppDbContext db, HttpContext context, int page = 1, int pageSize = 20)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var connection = db.Database.GetDbConnection();

        const string countSql = @"SELECT COUNT(*)
                                  FROM store_orders o
                                  WHERE o.tenant_id = @TenantId
                                    AND o.user_id = @UserId";

        const string itemsSql = @"SELECT o.id,
                                         o.user_id AS UserId,
                                         u.name AS UserName,
                                         o.status,
                                         o.total_amount AS TotalAmount,
                                         (
                                             SELECT COALESCE(SUM(i.quantity), 0)
                                             FROM store_order_items i
                                             WHERE i.store_order_id = o.id
                                         ) AS TotalItems,
                                         o.created_at AS CreatedAt,
                                         o.updated_at AS UpdatedAt
                                  FROM store_orders o
                                  JOIN users u ON u.id = o.user_id AND u.tenant_id = o.tenant_id
                                  WHERE o.tenant_id = @TenantId
                                    AND o.user_id = @UserId
                                  ORDER BY o.created_at DESC
                                  LIMIT @Limit OFFSET @Offset";

        var parameters = new
        {
            TenantId = tenantId,
            UserId = userId,
            Limit = pageSize,
            Offset = (page - 1) * pageSize
        };

        var total = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<StoreOrderListItemResponse>(itemsSql, parameters);

        return Results.Ok(new
        {
            page,
            pageSize,
            total,
            items
        });
    }

    /// <summary>
    /// Retorna o detalhe de um pedido do proprio usuario.
    /// </summary>
    private static async Task<IResult> GetMyOrderByIdAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        var order = await BuildOrderDetailAsync(id, tenantId, db);
        if (order is null || order.UserId != userId)
            return Results.NotFound();

        return Results.Ok(order);
    }

    /// <summary>
    /// Lista os pedidos da loja para visao administrativa do tenant.
    /// </summary>
    private static async Task<IResult> GetTenantOrdersAsync(AppDbContext db,
                                                            HttpContext context,
                                                            Guid? userId = null,
                                                            StoreOrderStatus? status = null,
                                                            int page = 1,
                                                            int pageSize = 20)
    {
        var tenantId = context.GetTenantId();

        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        if (userId.HasValue)
        {
            var userExists = await db.Users.AnyAsync(x => x.Id == userId.Value && x.TenantId == tenantId && x.IsActive);
            if (!userExists)
                return Results.BadRequest("User does not belong to this tenant.");
        }

        var connection = db.Database.GetDbConnection();

        const string countSql = @"SELECT COUNT(*)
                                  FROM store_orders o
                                  WHERE o.tenant_id = @TenantId
                                    AND (@UserId IS NULL OR o.user_id = @UserId)
                                    AND (@Status IS NULL OR o.status = @Status)";

        const string itemsSql = @"SELECT o.id,
                                         o.user_id AS UserId,
                                         u.name AS UserName,
                                         o.status,
                                         o.total_amount AS TotalAmount,
                                         (
                                             SELECT COALESCE(SUM(i.quantity), 0)
                                             FROM store_order_items i
                                             WHERE i.store_order_id = o.id
                                         ) AS TotalItems,
                                         o.created_at AS CreatedAt,
                                         o.updated_at AS UpdatedAt
                                  FROM store_orders o
                                  JOIN users u ON u.id = o.user_id AND u.tenant_id = o.tenant_id
                                  WHERE o.tenant_id = @TenantId
                                    AND (@UserId IS NULL OR o.user_id = @UserId)
                                    AND (@Status IS NULL OR o.status = @Status)
                                  ORDER BY o.created_at DESC
                                  LIMIT @Limit OFFSET @Offset";

        var parameters = new
        {
            TenantId = tenantId,
            UserId = userId,
            Status = status,
            Limit = pageSize,
            Offset = (page - 1) * pageSize
        };

        var total = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<StoreOrderListItemResponse>(itemsSql, parameters);

        return Results.Ok(new
        {
            page,
            pageSize,
            total,
            items
        });
    }

    /// <summary>
    /// Atualiza o status operacional do pedido e ajusta o estoque quando a reserva e cancelada ou confirmada.
    /// </summary>
    private static async Task<IResult> UpdateOrderStatusAsync(Guid id,
                                                              UpdateStoreOrderStatusRequest request,
                                                              AppDbContext db,
                                                              IGamificationService gamificationService,
                                                              HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var actorUserId = GetUserId(context.User);

        var order = await db.StoreOrders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (order is null)
            return Results.NotFound();

        if (!CanTransitionStatus(order.Status, request.Status))
            return Results.BadRequest("Invalid status transition for this order.");

        var variantIds = order.Items.Select(x => x.ProductVariantId).Distinct().ToList();
        var variants = await db.ProductVariants
            .Where(x => variantIds.Contains(x.Id) && x.TenantId == tenantId)
            .ToDictionaryAsync(x => x.Id);

        var previousStatus = order.Status;
        var currentReservesStock = DoesStatusReserveStock(order.Status);
        var nextReservesStock = DoesStatusReserveStock(request.Status);

        if (!currentReservesStock && nextReservesStock)
        {
            foreach (var item in order.Items)
            {
                var variant = variants[item.ProductVariantId];
                if (variant.StockQuantity < item.Quantity)
                    return Results.BadRequest($"Insufficient stock to reserve variant {item.VariantName}.");
            }

            foreach (var item in order.Items)
            {
                variants[item.ProductVariantId].StockQuantity -= item.Quantity;
            }
        }
        else if (currentReservesStock && !nextReservesStock)
        {
            foreach (var item in order.Items)
            {
                variants[item.ProductVariantId].StockQuantity += item.Quantity;
            }
        }

        order.Status = request.Status;
        order.AdminNotes = NormalizeOptional(request.AdminNotes);
        order.LastUpdatedByUserId = actorUserId;
        order.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        if (previousStatus != StoreOrderStatus.PaidLocally && request.Status == StoreOrderStatus.PaidLocally)
        {
            await gamificationService.AwardEventAsync(
                tenantId,
                order.UserId,
                GamificationEventType.StorePurchase,
                "store_order",
                order.Id,
                order.UpdatedAt,
                "Store order paid locally at the gym administration.");
        }

        var response = await BuildOrderDetailAsync(order.Id, tenantId, db);
        return Results.Ok(response);
    }

    /// <summary>
    /// Reconstroi a resposta detalhada do produto usando consultas orientadas para leitura.
    /// </summary>
    private static async Task<ProductDetailResponse?> BuildProductDetailAsync(Guid id,
                                                                              Guid tenantId,
                                                                              AppDbContext db,
                                                                              bool includeInactiveVariants,
                                                                              bool requireActiveProduct)
    {
        var connection = db.Database.GetDbConnection();

        const string productSql = @"SELECT p.id,
                                           p.name,
                                           p.description,
                                           p.main_media_id AS MainMediaId,
                                           m.url AS MainMediaUrl,
                                           p.is_active AS IsActive,
                                           p.display_order AS DisplayOrder,
                                           p.created_at AS CreatedAt
                                    FROM products p
                                    LEFT JOIN tenant_medias m
                                      ON m.id = p.main_media_id
                                     AND m.tenant_id = p.tenant_id
                                    WHERE p.id = @Id
                                      AND p.tenant_id = @TenantId
                                      AND (@RequireActiveProduct = false OR p.is_active = true)";

        const string variantsSql = @"SELECT v.id,
                                            v.product_id AS ProductId,
                                            v.name,
                                            v.color,
                                            v.size,
                                            v.price,
                                            v.stock_quantity AS StockQuantity,
                                            v.is_active AS IsActive,
                                            v.created_at AS CreatedAt
                                     FROM product_variants v
                                     WHERE v.product_id = @ProductId
                                       AND v.tenant_id = @TenantId
                                       AND (@IncludeInactiveVariants = true OR v.is_active = true)
                                     ORDER BY v.created_at ASC";

        var product = await connection.QueryFirstOrDefaultAsync<ProductDetailProjection>(productSql, new
        {
            Id = id,
            TenantId = tenantId,
            RequireActiveProduct = requireActiveProduct
        });

        if (product is null)
            return null;

        var variants = await connection.QueryAsync<ProductVariantResponse>(variantsSql, new
        {
            ProductId = id,
            TenantId = tenantId,
            IncludeInactiveVariants = includeInactiveVariants
        });

        return new ProductDetailResponse(
            product.Id,
            product.Name,
            product.Description,
            product.MainMediaId,
            product.MainMediaUrl,
            product.IsActive,
            product.DisplayOrder,
            product.CreatedAt,
            variants.ToList());
    }

    /// <summary>
    /// Valida os dados principais do produto antes da escrita.
    /// </summary>
    private static async Task<IResult?> ValidateProductRequestAsync(string name, Guid? mainMediaId, Guid tenantId, AppDbContext db)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Results.BadRequest("Product name is required.");

        if (name.Trim().Length > 150)
            return Results.BadRequest("Product name must have at most 150 characters.");

        if (mainMediaId.HasValue)
        {
            var mediaExists = await db.TenantMedias.AnyAsync(x => x.Id == mainMediaId.Value && x.TenantId == tenantId);
            if (!mediaExists)
                return Results.BadRequest("Main media does not belong to this tenant.");
        }

        return null;
    }

    /// <summary>
    /// Valida os dados comerciais principais de uma variante.
    /// </summary>
    private static IResult? ValidateVariantRequest(string name, decimal price, int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Results.BadRequest("Variant name is required.");

        if (price < 0)
            return Results.BadRequest("Variant price cannot be negative.");

        if (stockQuantity < 0)
            return Results.BadRequest("Variant stock quantity cannot be negative.");

        return null;
    }

    /// <summary>
    /// Converte a entidade da variante para o contrato de resposta da API.
    /// </summary>
    private static ProductVariantResponse MapVariantResponse(AlphaSquad.Infrastructure.Persistence.ProductVariant variant)
    {
        return new ProductVariantResponse(
            variant.Id,
            variant.ProductId,
            variant.Name,
            variant.Color,
            variant.Size,
            variant.Price,
            variant.StockQuantity,
            variant.IsActive,
            variant.CreatedAt
        );
    }

    /// <summary>
    /// Reconstroi a resposta detalhada do pedido com snapshots dos itens e dados do usuario.
    /// </summary>
    private static async Task<StoreOrderDetailResponse?> BuildOrderDetailAsync(Guid id, Guid tenantId, AppDbContext db)
    {
        var connection = db.Database.GetDbConnection();

        const string orderSql = @"SELECT o.id,
                                         o.user_id AS UserId,
                                         u.name AS UserName,
                                         u.email AS UserEmail,
                                         o.status,
                                         o.total_amount AS TotalAmount,
                                         o.customer_notes AS CustomerNotes,
                                         o.admin_notes AS AdminNotes,
                                         o.last_updated_by_user_id AS LastUpdatedByUserId,
                                         changed_by.name AS LastUpdatedByUserName,
                                         o.created_at AS CreatedAt,
                                         o.updated_at AS UpdatedAt
                                  FROM store_orders o
                                  JOIN users u ON u.id = o.user_id AND u.tenant_id = o.tenant_id
                                  LEFT JOIN users changed_by
                                    ON changed_by.id = o.last_updated_by_user_id
                                   AND changed_by.tenant_id = o.tenant_id
                                  WHERE o.id = @Id
                                    AND o.tenant_id = @TenantId";

        const string itemsSql = @"SELECT i.id,
                                         i.product_id AS ProductId,
                                         i.product_variant_id AS ProductVariantId,
                                         i.product_name AS ProductName,
                                         i.variant_name AS VariantName,
                                         i.variant_color AS VariantColor,
                                         i.variant_size AS VariantSize,
                                         i.quantity,
                                         i.unit_price AS UnitPrice,
                                         i.line_total AS LineTotal
                                  FROM store_order_items i
                                  WHERE i.store_order_id = @StoreOrderId
                                    AND i.tenant_id = @TenantId
                                  ORDER BY i.product_name, i.variant_name";

        var order = await connection.QueryFirstOrDefaultAsync<StoreOrderDetailProjection>(orderSql, new { Id = id, TenantId = tenantId });
        if (order is null)
            return null;

        var items = await connection.QueryAsync<StoreOrderItemResponse>(itemsSql, new { StoreOrderId = id, TenantId = tenantId });

        return new StoreOrderDetailResponse(
            order.Id,
            order.UserId,
            order.UserName,
            order.UserEmail,
            order.Status,
            order.TotalAmount,
            order.CustomerNotes,
            order.AdminNotes,
            order.LastUpdatedByUserId,
            order.LastUpdatedByUserName,
            order.CreatedAt,
            order.UpdatedAt,
            items.ToList());
    }

    /// <summary>
    /// Determina se um status representa itens fisicamente reservados no estoque.
    /// </summary>
    private static bool DoesStatusReserveStock(StoreOrderStatus status)
    {
        return status is StoreOrderStatus.Reserved or StoreOrderStatus.ReadyForPickup or StoreOrderStatus.PaidLocally;
    }

    /// <summary>
    /// Valida transicoes permitidas do fluxo presencial da loja.
    /// </summary>
    private static bool CanTransitionStatus(StoreOrderStatus currentStatus, StoreOrderStatus nextStatus)
    {
        if (currentStatus == nextStatus)
            return true;

        return currentStatus switch
        {
            StoreOrderStatus.PendingApproval => nextStatus is StoreOrderStatus.Reserved or StoreOrderStatus.Cancelled,
            StoreOrderStatus.Reserved => nextStatus is StoreOrderStatus.ReadyForPickup or StoreOrderStatus.PaidLocally or StoreOrderStatus.Cancelled,
            StoreOrderStatus.ReadyForPickup => nextStatus is StoreOrderStatus.PaidLocally or StoreOrderStatus.Cancelled,
            StoreOrderStatus.PaidLocally => false,
            StoreOrderStatus.Cancelled => false,
            _ => false
        };
    }

    /// <summary>
    /// Extrai o identificador do usuario autenticado a partir das claims do JWT.
    /// </summary>
    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim))
            throw new UnauthorizedAccessException("User not found in token.");

        return Guid.Parse(userIdClaim);
    }

    /// <summary>
    /// Extrai a role do usuario autenticado para adaptar o comportamento de leitura.
    /// </summary>
    private static UserRole? GetUserRole(ClaimsPrincipal user)
    {
        var roleClaim = user.FindFirstValue(ClaimTypes.Role);
        return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : null;
    }

    /// <summary>
    /// Normaliza campos opcionais para evitar persistencia de espacos em branco.
    /// </summary>
    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Gera um cursor opaco para o feed de produtos.
    /// Ele replica exatamente a ordenacao do catalogo para garantir continuidade estavel.
    /// </summary>
    private static string EncodeProductFeedCursor(int displayOrder, DateTime createdAt, Guid id)
    {
        var rawCursor = $"{displayOrder}|{createdAt.Ticks}|{id:N}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(rawCursor));
    }

    /// <summary>
    /// Decodifica o cursor do feed da loja.
    /// Cursors invalidos sao ignorados para evitar quebra da navegacao no app.
    /// </summary>
    private static ProductFeedCursor? DecodeProductFeedCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var rawCursor = Encoding.UTF8.GetString(Convert.FromBase64String(cursor.Trim()));
            var parts = rawCursor.Split('|', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 3)
                return null;

            if (!int.TryParse(parts[0], out var displayOrder) ||
                !long.TryParse(parts[1], out var createdAtTicks) ||
                !Guid.TryParseExact(parts[2], "N", out var id))
            {
                return null;
            }

            return new ProductFeedCursor(displayOrder, new DateTime(createdAtTicks, DateTimeKind.Utc), id);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Projecao interna para reconstruir o detalhe do produto com Dapper.
    /// </summary>
    private sealed record ProductDetailProjection(
        Guid Id,
        string Name,
        string? Description,
        Guid? MainMediaId,
        string? MainMediaUrl,
        bool IsActive,
        int DisplayOrder,
        DateTime CreatedAt
    );

    /// <summary>
    /// Projecao interna do pedido para montagem da resposta detalhada.
    /// </summary>
    private sealed record StoreOrderDetailProjection(
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
        DateTime UpdatedAt
    );

    /// <summary>
    /// Estrutura interna do cursor do catalogo da loja.
    /// Ela espelha a ordenacao por `DisplayOrder`, `CreatedAt` e `Id`.
    /// </summary>
    private sealed record ProductFeedCursor(
        int DisplayOrder,
        DateTime CreatedAt,
        Guid Id
    );
}
