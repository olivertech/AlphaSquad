namespace AlphaSquad.Api.Features.Store;

/// <summary>
/// Registra os endpoints do modulo de loja interna da academia.
/// Este primeiro recorte cobre o catalogo de produtos e a gestao basica de variantes.
/// </summary>
public static class StoreEndpoints
{
    public static IEndpointRouteBuilder MapStoreEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/store/products")
            .WithTags("Store")
            .RequireAuthorization();

        group.MapGet("/", GetAllAsync)
            .WithName("GetStoreProducts")
            .WithSummary("Lista os produtos da loja do tenant atual.")
            .WithDescription("Retorna um catalogo paginado de produtos da academia, com filtro por status e busca simples por nome.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetStoreProductById")
            .WithSummary("Retorna o detalhe de um produto da loja.")
            .WithDescription("Busca um produto especifico do tenant atual com suas variantes ativas ou administrativas, conforme o contexto do endpoint.")
            .Produces<ProductDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("CreateStoreProduct")
            .WithSummary("Cria um novo produto na loja do tenant.")
            .WithDescription("Cadastra um produto da academia com nome, descricao, imagem principal opcional e ordenacao de exibicao.")
            .Produces<ProductDetailResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("UpdateStoreProduct")
            .WithSummary("Atualiza um produto da loja do tenant.")
            .WithDescription("Permite alterar os dados basicos, a imagem principal e o status ativo do produto.")
            .Produces<ProductDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("DeleteStoreProduct")
            .WithSummary("Remove um produto da loja do tenant.")
            .WithDescription("Exclui fisicamente um produto e suas variantes, restrito a perfis administrativos e de gestao.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/{id:guid}/variants", CreateVariantAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("CreateStoreProductVariant")
            .WithSummary("Cria uma variante para um produto da loja.")
            .WithDescription("Adiciona uma nova variante de tamanho, cor, preco e estoque para um produto existente do tenant.")
            .Produces<ProductVariantResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{productId:guid}/variants/{variantId:guid}", UpdateVariantAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("UpdateStoreProductVariant")
            .WithSummary("Atualiza uma variante de produto da loja.")
            .WithDescription("Permite alterar os dados comerciais e o status de uma variante de produto existente.")
            .Produces<ProductVariantResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapDelete("/{productId:guid}/variants/{variantId:guid}", DeleteVariantAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher)
            .WithName("DeleteStoreProductVariant")
            .WithSummary("Remove uma variante de produto da loja.")
            .WithDescription("Exclui fisicamente uma variante de um produto do tenant atual.")
            .Produces(StatusCodes.Status204NoContent)
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
}
