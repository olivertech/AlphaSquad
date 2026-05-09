using AlphaSquad.Infrastructure.Features;

namespace AlphaSquad.Api.Features.Social;

/// <summary>
/// Registra os endpoints da rede social interna do tenant.
/// Nesta V1 o modulo entrega post, feed, like e comentario simples sem threads.
/// </summary>
public static class SocialEndpoints
{
    public static IEndpointRouteBuilder MapSocialEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/social/posts")
            .WithTags("Social")
            .RequireAuthorization();

        group.MapGet("/", GetAllAsync)
            .WithName("GetSocialPosts")
            .WithSummary("Lista as publicacoes da rede interna.")
            .WithDescription("Retorna a listagem paginada das publicacoes do tenant atual, adequada para dashboards e consultas tradicionais.")
            .Produces<PagedResponse<SocialPostResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/feed", GetFeedAsync)
            .WithName("GetSocialPostsFeed")
            .WithSummary("Lista as publicacoes em formato cursor-based.")
            .WithDescription("Retorna o feed social em formato cursor-based para scroll infinito no app.")
            .Produces<CursorFeedResponse<SocialPostResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetSocialPostById")
            .WithSummary("Retorna o detalhe de uma publicacao social.")
            .WithDescription("Busca uma publicacao especifica do tenant atual com os contadores e estado de curtida do usuario autenticado.")
            .Produces<SocialPostResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateSocialPost")
            .WithSummary("Cria uma nova publicacao social.")
            .WithDescription("Permite que o usuario autenticado publique uma foto, uma descricao ou ambos na rede interna da academia.")
            .Produces<SocialPostResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/{id:guid}/like", LikeAsync)
            .WithName("LikeSocialPost")
            .WithSummary("Curte uma publicacao social.")
            .WithDescription("Registra a curtida do usuario autenticado na publicacao informada, sem duplicar likes.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}/like", UnlikeAsync)
            .WithName("UnlikeSocialPost")
            .WithSummary("Remove a curtida de uma publicacao social.")
            .WithDescription("Remove a curtida previamente registrada pelo usuario autenticado.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/comments", GetCommentsAsync)
            .WithName("GetSocialPostComments")
            .WithSummary("Lista os comentarios simples de uma publicacao.")
            .WithDescription("Retorna os comentarios da publicacao social sem suporte a respostas encadeadas.")
            .Produces<List<SocialCommentResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/comments", CreateCommentAsync)
            .WithName("CreateSocialPostComment")
            .WithSummary("Cria um comentario simples em uma publicacao.")
            .WithDescription("Permite comentar uma publicacao da rede interna sem abrir uma thread de respostas.")
            .Produces<SocialCommentResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Retorna a listagem paginada da rede social para uso administrativo ou consultas tradicionais.
    /// </summary>
    private static async Task<IResult> GetAllAsync(AppDbContext db,
                                                   IFeatureAccessService featureAccessService,
                                                   HttpContext context,
                                                   int page = 1,
                                                   int pageSize = 20)
    {
        var featureResult = await EnsureSocialFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        var currentUserId = GetUserId(context.User);
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var connection = db.Database.GetDbConnection();

        const string countSql = @"SELECT COUNT(*)
                                  FROM social_posts p
                                  WHERE p.tenant_id = @TenantId
                                    AND p.is_active = true";

        const string itemsSql = @"SELECT p.id,
                                         p.user_id AS UserId,
                                         u.name AS UserName,
                                         up.username,
                                         up.profile_photo_url AS ProfilePhotoUrl,
                                         p.description,
                                         p.media_id AS MediaId,
                                         m.url AS MediaUrl,
                                         COALESCE(l.likes_count, 0) AS LikesCount,
                                         COALESCE(c.comments_count, 0) AS CommentsCount,
                                         CASE WHEN liked.id IS NULL THEN false ELSE true END AS IsLikedByCurrentUser,
                                         p.created_at AS CreatedAt,
                                         p.updated_at AS UpdatedAt
                                  FROM social_posts p
                                  JOIN users u
                                    ON u.id = p.user_id
                                   AND u.tenant_id = p.tenant_id
                                  LEFT JOIN user_profiles up
                                    ON up.user_id = u.id
                                   AND up.tenant_id = u.tenant_id
                                  LEFT JOIN tenant_medias m
                                    ON m.id = p.media_id
                                   AND m.tenant_id = p.tenant_id
                                  LEFT JOIN LATERAL (
                                      SELECT COUNT(*)::INTEGER AS likes_count
                                      FROM social_post_likes spl
                                      WHERE spl.social_post_id = p.id
                                        AND spl.tenant_id = @TenantId
                                  ) l ON true
                                  LEFT JOIN LATERAL (
                                      SELECT COUNT(*)::INTEGER AS comments_count
                                      FROM social_post_comments spc
                                      WHERE spc.social_post_id = p.id
                                        AND spc.tenant_id = @TenantId
                                  ) c ON true
                                  LEFT JOIN LATERAL (
                                      SELECT spl.id
                                      FROM social_post_likes spl
                                      WHERE spl.social_post_id = p.id
                                        AND spl.tenant_id = @TenantId
                                        AND spl.user_id = @CurrentUserId
                                      LIMIT 1
                                  ) liked ON true
                                  WHERE p.tenant_id = @TenantId
                                    AND p.is_active = true
                                  ORDER BY p.created_at DESC, p.id DESC
                                  LIMIT @Limit OFFSET @Offset";

        var parameters = new
        {
            TenantId = tenantId,
            CurrentUserId = currentUserId,
            Limit = pageSize,
            Offset = (page - 1) * pageSize
        };

        var total = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<SocialPostResponse>(itemsSql, parameters);

        return Results.Ok(new PagedResponse<SocialPostResponse>(page, pageSize, total, items.ToList()));
    }

    /// <summary>
    /// Retorna o feed cursor-based da rede social para scroll infinito no app.
    /// </summary>
    private static async Task<IResult> GetFeedAsync(AppDbContext db,
                                                    IFeatureAccessService featureAccessService,
                                                    HttpContext context,
                                                    string? cursor = null,
                                                    int limit = 20)
    {
        var featureResult = await EnsureSocialFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        var currentUserId = GetUserId(context.User);
        limit = limit is < 1 or > 50 ? 20 : limit;

        var cursorData = DecodeSocialFeedCursor(cursor);
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT p.id,
                                    p.user_id AS UserId,
                                    u.name AS UserName,
                                    up.username,
                                    up.profile_photo_url AS ProfilePhotoUrl,
                                    p.description,
                                    p.media_id AS MediaId,
                                    m.url AS MediaUrl,
                                    COALESCE(l.likes_count, 0) AS LikesCount,
                                    COALESCE(c.comments_count, 0) AS CommentsCount,
                                    CASE WHEN liked.id IS NULL THEN false ELSE true END AS IsLikedByCurrentUser,
                                    p.created_at AS CreatedAt,
                                    p.updated_at AS UpdatedAt
                             FROM social_posts p
                             JOIN users u
                               ON u.id = p.user_id
                              AND u.tenant_id = p.tenant_id
                             LEFT JOIN user_profiles up
                               ON up.user_id = u.id
                              AND up.tenant_id = u.tenant_id
                             LEFT JOIN tenant_medias m
                               ON m.id = p.media_id
                              AND m.tenant_id = p.tenant_id
                             LEFT JOIN LATERAL (
                                 SELECT COUNT(*)::INTEGER AS likes_count
                                 FROM social_post_likes spl
                                 WHERE spl.social_post_id = p.id
                                   AND spl.tenant_id = @TenantId
                             ) l ON true
                             LEFT JOIN LATERAL (
                                 SELECT COUNT(*)::INTEGER AS comments_count
                                 FROM social_post_comments spc
                                 WHERE spc.social_post_id = p.id
                                   AND spc.tenant_id = @TenantId
                             ) c ON true
                             LEFT JOIN LATERAL (
                                 SELECT spl.id
                                 FROM social_post_likes spl
                                 WHERE spl.social_post_id = p.id
                                   AND spl.tenant_id = @TenantId
                                   AND spl.user_id = @CurrentUserId
                                 LIMIT 1
                             ) liked ON true
                             WHERE p.tenant_id = @TenantId
                               AND p.is_active = true
                               AND (
                                   @CursorCreatedAt IS NULL
                                   OR p.created_at < @CursorCreatedAt
                                   OR (p.created_at = @CursorCreatedAt AND p.id < @CursorId)
                               )
                             ORDER BY p.created_at DESC, p.id DESC
                             LIMIT @LimitPlusOne";

        var rows = (await connection.QueryAsync<SocialPostResponse>(sql, new
        {
            TenantId = tenantId,
            CurrentUserId = currentUserId,
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
            nextCursor = EncodeSocialFeedCursor(lastItem.CreatedAt, lastItem.Id);
        }

        return Results.Ok(new CursorFeedResponse<SocialPostResponse>(items, nextCursor, hasMore));
    }

    /// <summary>
    /// Retorna o detalhe de uma publicacao social do tenant atual.
    /// </summary>
    private static async Task<IResult> GetByIdAsync(Guid id,
                                                    AppDbContext db,
                                                    IFeatureAccessService featureAccessService,
                                                    HttpContext context)
    {
        var featureResult = await EnsureSocialFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        var currentUserId = GetUserId(context.User);
        var response = await BuildSocialPostResponseAsync(id, tenantId, currentUserId, db);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    /// <summary>
    /// Cria uma nova publicacao social do tenant atual.
    /// O autor pode publicar com descricao, midia, ou ambos.
    /// </summary>
    private static async Task<IResult> CreateAsync(CreateSocialPostRequest request,
                                                   AppDbContext db,
                                                   IFeatureAccessService featureAccessService,
                                                   IGamificationService gamificationService,
                                                   HttpContext context)
    {
        var featureResult = await EnsureSocialFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);
        var validation = await ValidateSocialPostRequestAsync(request.Description, request.MediaId, tenantId, db);
        if (validation is not null)
            return validation;

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.IsActive);
        if (user is null)
            return Results.Unauthorized();

        var post = new SocialPost
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            Description = NormalizeOptional(request.Description),
            MediaId = request.MediaId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.SocialPosts.Add(post);
        await db.SaveChangesAsync();

        await gamificationService.AwardEventAsync(
            tenantId,
            userId,
            GamificationEventType.SocialPost,
            "social_post",
            post.Id,
            post.CreatedAt,
            "Social post created successfully.");

        var response = await BuildSocialPostResponseAsync(post.Id, tenantId, userId, db);
        return Results.Created($"/api/social/posts/{post.Id}", response);
    }

    /// <summary>
    /// Registra a curtida do usuario autenticado em uma publicacao social.
    /// </summary>
    private static async Task<IResult> LikeAsync(Guid id,
                                                 AppDbContext db,
                                                 IFeatureAccessService featureAccessService,
                                                 HttpContext context)
    {
        var featureResult = await EnsureSocialFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        var postExists = await db.SocialPosts.AnyAsync(x => x.Id == id && x.TenantId == tenantId && x.IsActive);
        if (!postExists)
            return Results.NotFound();

        var alreadyLiked = await db.SocialPostLikes.AnyAsync(x => x.SocialPostId == id && x.TenantId == tenantId && x.UserId == userId);
        if (alreadyLiked)
            return Results.Conflict("User has already liked this post.");

        db.SocialPostLikes.Add(new SocialPostLike
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SocialPostId = id,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    /// <summary>
    /// Remove a curtida do usuario autenticado em uma publicacao social.
    /// </summary>
    private static async Task<IResult> UnlikeAsync(Guid id,
                                                   AppDbContext db,
                                                   IFeatureAccessService featureAccessService,
                                                   HttpContext context)
    {
        var featureResult = await EnsureSocialFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        var like = await db.SocialPostLikes.FirstOrDefaultAsync(x =>
            x.SocialPostId == id &&
            x.TenantId == tenantId &&
            x.UserId == userId);

        if (like is null)
            return Results.NotFound();

        db.SocialPostLikes.Remove(like);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    /// <summary>
    /// Lista os comentarios simples de uma publicacao social.
    /// </summary>
    private static async Task<IResult> GetCommentsAsync(Guid id,
                                                        AppDbContext db,
                                                        IFeatureAccessService featureAccessService,
                                                        HttpContext context)
    {
        var featureResult = await EnsureSocialFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        var postExists = await db.SocialPosts.AnyAsync(x => x.Id == id && x.TenantId == tenantId && x.IsActive);
        if (!postExists)
            return Results.NotFound();

        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT c.id,
                                    c.social_post_id AS SocialPostId,
                                    c.user_id AS UserId,
                                    u.name AS UserName,
                                    up.username,
                                    c.message,
                                    c.created_at AS CreatedAt
                             FROM social_post_comments c
                             JOIN users u
                               ON u.id = c.user_id
                              AND u.tenant_id = c.tenant_id
                             LEFT JOIN user_profiles up
                               ON up.user_id = u.id
                              AND up.tenant_id = u.tenant_id
                             WHERE c.social_post_id = @SocialPostId
                               AND c.tenant_id = @TenantId
                             ORDER BY c.created_at ASC, c.id ASC";

        var items = await connection.QueryAsync<SocialCommentResponse>(sql, new
        {
            SocialPostId = id,
            TenantId = tenantId
        });

        return Results.Ok(items.ToList());
    }

    /// <summary>
    /// Cria um comentario simples em uma publicacao da rede interna.
    /// </summary>
    private static async Task<IResult> CreateCommentAsync(Guid id,
                                                          CreateSocialCommentRequest request,
                                                          AppDbContext db,
                                                          IFeatureAccessService featureAccessService,
                                                          HttpContext context)
    {
        var featureResult = await EnsureSocialFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        if (string.IsNullOrWhiteSpace(request.Message))
            return Results.BadRequest("Comment message is required.");

        if (request.Message.Trim().Length > 500)
            return Results.BadRequest("Comment message must have at most 500 characters.");

        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        var postExists = await db.SocialPosts.AnyAsync(x => x.Id == id && x.TenantId == tenantId && x.IsActive);
        if (!postExists)
            return Results.NotFound();

        var comment = new SocialPostComment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SocialPostId = id,
            UserId = userId,
            Message = request.Message.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        db.SocialPostComments.Add(comment);
        await db.SaveChangesAsync();

        var connection = db.Database.GetDbConnection();
        const string sql = @"SELECT c.id,
                                    c.social_post_id AS SocialPostId,
                                    c.user_id AS UserId,
                                    u.name AS UserName,
                                    up.username,
                                    c.message,
                                    c.created_at AS CreatedAt
                             FROM social_post_comments c
                             JOIN users u
                               ON u.id = c.user_id
                              AND u.tenant_id = c.tenant_id
                             LEFT JOIN user_profiles up
                               ON up.user_id = u.id
                              AND up.tenant_id = u.tenant_id
                             WHERE c.id = @Id
                               AND c.tenant_id = @TenantId";

        var response = await connection.QueryFirstAsync<SocialCommentResponse>(sql, new { comment.Id, TenantId = tenantId });
        return Results.Created($"/api/social/posts/{id}/comments/{comment.Id}", response);
    }

    /// <summary>
    /// Reconstroi a resposta de uma publicacao social com os contadores de interacao e o estado da curtida.
    /// </summary>
    private static async Task<SocialPostResponse?> BuildSocialPostResponseAsync(Guid postId,
                                                                                Guid tenantId,
                                                                                Guid currentUserId,
                                                                                AppDbContext db)
    {
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT p.id,
                                    p.user_id AS UserId,
                                    u.name AS UserName,
                                    up.username,
                                    up.profile_photo_url AS ProfilePhotoUrl,
                                    p.description,
                                    p.media_id AS MediaId,
                                    m.url AS MediaUrl,
                                    COALESCE(l.likes_count, 0) AS LikesCount,
                                    COALESCE(c.comments_count, 0) AS CommentsCount,
                                    CASE WHEN liked.id IS NULL THEN false ELSE true END AS IsLikedByCurrentUser,
                                    p.created_at AS CreatedAt,
                                    p.updated_at AS UpdatedAt
                             FROM social_posts p
                             JOIN users u
                               ON u.id = p.user_id
                              AND u.tenant_id = p.tenant_id
                             LEFT JOIN user_profiles up
                               ON up.user_id = u.id
                              AND up.tenant_id = u.tenant_id
                             LEFT JOIN tenant_medias m
                               ON m.id = p.media_id
                              AND m.tenant_id = p.tenant_id
                             LEFT JOIN LATERAL (
                                 SELECT COUNT(*)::INTEGER AS likes_count
                                 FROM social_post_likes spl
                                 WHERE spl.social_post_id = p.id
                                   AND spl.tenant_id = @TenantId
                             ) l ON true
                             LEFT JOIN LATERAL (
                                 SELECT COUNT(*)::INTEGER AS comments_count
                                 FROM social_post_comments spc
                                 WHERE spc.social_post_id = p.id
                                   AND spc.tenant_id = @TenantId
                             ) c ON true
                             LEFT JOIN LATERAL (
                                 SELECT spl.id
                                 FROM social_post_likes spl
                                 WHERE spl.social_post_id = p.id
                                   AND spl.tenant_id = @TenantId
                                   AND spl.user_id = @CurrentUserId
                                 LIMIT 1
                             ) liked ON true
                             WHERE p.id = @PostId
                               AND p.tenant_id = @TenantId
                               AND p.is_active = true";

        return await connection.QueryFirstOrDefaultAsync<SocialPostResponse>(sql, new
        {
            PostId = postId,
            TenantId = tenantId,
            CurrentUserId = currentUserId
        });
    }

    /// <summary>
    /// Valida o payload principal de uma publicacao social.
    /// A publicacao precisa ter pelo menos um conteudo visivel: descricao ou midia.
    /// </summary>
    private static async Task<IResult?> ValidateSocialPostRequestAsync(string? description,
                                                                       Guid? mediaId,
                                                                       Guid tenantId,
                                                                       AppDbContext db)
    {
        if (string.IsNullOrWhiteSpace(description) && !mediaId.HasValue)
            return Results.BadRequest("Post must contain description, media, or both.");

        if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > 1000)
            return Results.BadRequest("Post description must have at most 1000 characters.");

        if (mediaId.HasValue)
        {
            var mediaExists = await db.TenantMedias.AnyAsync(x => x.Id == mediaId.Value && x.TenantId == tenantId);
            if (!mediaExists)
                return Results.BadRequest("Media does not belong to this tenant.");
        }

        return null;
    }

    /// <summary>
    /// Verifica se o tenant atual contratou o modulo opcional da rede social.
    /// </summary>
    private static async Task<IResult?> EnsureSocialFeatureEnabledAsync(IFeatureAccessService featureAccessService, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var isEnabled = await featureAccessService.HasFeatureAsync(tenantId, FeatureCodes.Social);

        if (isEnabled)
            return null;

        return Results.Problem(
            title: "Feature not enabled",
            detail: "The SOCIAL module is not enabled for the current tenant.",
            statusCode: StatusCodes.Status403Forbidden);
    }

    /// <summary>
    /// Gera um cursor opaco com base na ordenacao principal do feed social.
    /// </summary>
    private static string EncodeSocialFeedCursor(DateTime createdAt, Guid id)
    {
        var rawCursor = $"{createdAt.Ticks}|{id:N}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(rawCursor));
    }

    /// <summary>
    /// Decodifica o cursor do feed social.
    /// Cursors invalidos sao ignorados para preservar a experiencia do app.
    /// </summary>
    private static SocialFeedCursor? DecodeSocialFeedCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var rawCursor = Encoding.UTF8.GetString(Convert.FromBase64String(cursor.Trim()));
            var parts = rawCursor.Split('|', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                return null;

            if (!long.TryParse(parts[0], out var createdAtTicks) ||
                !Guid.TryParseExact(parts[1], "N", out var id))
            {
                return null;
            }

            return new SocialFeedCursor(new DateTime(createdAtTicks, DateTimeKind.Utc), id);
        }
        catch
        {
            return null;
        }
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
    /// Normaliza campos opcionais para evitar persistencia de texto em branco.
    /// </summary>
    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Estrutura interna do cursor da rede social.
    /// Ela replica a ordenacao do feed para continuidade estavel.
    /// </summary>
    private sealed record SocialFeedCursor(
        DateTime CreatedAt,
        Guid Id
    );
}
