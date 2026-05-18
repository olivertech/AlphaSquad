namespace AlphaSquad.Api.Features.Notifications;

using AlphaSquad.Infrastructure.Notifications;

/// <summary>
/// Registra os endpoints da central de notificacoes do app.
/// A base cobre publicacao institucional por tenant, leitura por usuario e filtros de pendencia.
/// </summary>
public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("/admin", GetAdminNotificationsAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("GetAdminNotifications")
            .WithSummary("Lista as notificacoes institucionais publicadas pela academia.")
            .Produces<PagedResponse<AdminNotificationListItemResponse>>(StatusCodes.Status200OK);

        group.MapPost("/admin", CreateAdminNotificationAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("CreateAdminNotification")
            .WithSummary("Cria uma notificacao institucional para o app.")
            .Produces<NotificationDetailsResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/admin/{id:guid}", UpdateAdminNotificationAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("UpdateAdminNotification")
            .WithSummary("Atualiza uma notificacao institucional da academia.")
            .Produces<NotificationDetailsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/admin/{id:guid}", DeleteAdminNotificationAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("DeleteAdminNotification")
            .WithSummary("Inativa uma notificacao institucional da academia.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/unread-count", GetUnreadCountAsync)
            .WithName("GetUnreadNotificationsCount")
            .WithSummary("Retorna a quantidade de notificacoes pendentes para o usuario autenticado.")
            .Produces<UnreadNotificationCountResponse>(StatusCodes.Status200OK);

        group.MapGet("/", GetMyNotificationsAsync)
            .WithName("GetMyNotifications")
            .WithSummary("Lista as notificacoes do usuario autenticado.")
            .Produces<PagedResponse<NotificationListItemResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetNotificationById")
            .WithSummary("Retorna o detalhe de uma notificacao do tenant atual.")
            .Produces<NotificationDetailsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/read", MarkAsReadAsync)
            .WithName("MarkNotificationAsRead")
            .WithSummary("Marca uma notificacao como lida para o usuario autenticado.")
            .Produces<NotificationReadResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetAdminNotificationsAsync(AppDbContext db,
                                                                  HttpContext context,
                                                                  TenantNotificationType? type = null,
                                                                  bool? isActive = null,
                                                                  int page = 1,
                                                                  int pageSize = 20)
    {
        var tenantId = context.GetTenantId();
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = db.TenantNotifications
            .Where(x => x.TenantId == tenantId);

        if (type.HasValue)
            query = query.Where(x => x.Type == type.Value);

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.PublishedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new AdminNotificationListItemResponse(
                x.Id,
                x.Type,
                x.Audience,
                x.Title,
                x.Summary,
                x.IsHighlighted,
                x.IsActive,
                x.PublishedAt,
                x.ExpiresAt,
                db.UserNotificationReads.Count(read => read.TenantNotificationId == x.Id),
                x.CreatedAt))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Results.Ok(new PagedResponse<AdminNotificationListItemResponse>(page, pageSize, total, items));
    }

    private static async Task<IResult> CreateAdminNotificationAsync(CreateNotificationRequest request,
                                                                    AppDbContext db,
                                                                    INotificationService notificationService,
                                                                    HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        var validation = await ValidateRequestAsync(
            tenantId,
            request.Title,
            request.Content,
            request.MediaId,
            request.PublishedAt,
            request.ExpiresAt,
            db);

        if (validation is not null)
            return validation;

        var notification = await notificationService.PublishAsync(new PublishTenantNotificationCommand(
            tenantId,
            request.Type,
            request.Audience,
            request.Title,
            request.Summary,
            request.Content,
            request.MediaId,
            request.IsHighlighted,
            request.RelatedEntityType,
            request.RelatedEntityId,
            userId,
            NormalizeToUtc(request.PublishedAt),
            NormalizeToUtc(request.ExpiresAt)
        ));

        var response = await BuildDetailsResponseAsync(notification.Id, tenantId, userId, db, ignoreAudience: true);
        return Results.Created($"/api/notifications/{notification.Id}", response);
    }

    private static async Task<IResult> UpdateAdminNotificationAsync(Guid id,
                                                                    UpdateNotificationRequest request,
                                                                    AppDbContext db,
                                                                    HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        var validation = await ValidateRequestAsync(
            tenantId,
            request.Title,
            request.Content,
            request.MediaId,
            request.PublishedAt,
            request.ExpiresAt,
            db);

        if (validation is not null)
            return validation;

        var notification = await db.TenantNotifications.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (notification is null)
            return Results.NotFound();

        notification.Type = request.Type;
        notification.Audience = request.Audience;
        notification.Title = request.Title.Trim();
        notification.Summary = NormalizeOptionalText(request.Summary);
        notification.Content = request.Content.Trim();
        notification.MediaId = request.MediaId;
        notification.IsHighlighted = request.IsHighlighted;
        notification.IsActive = request.IsActive;
        notification.RelatedEntityType = NormalizeOptionalText(request.RelatedEntityType);
        notification.RelatedEntityId = request.RelatedEntityId;
        notification.PublishedAt = NormalizeToUtc(request.PublishedAt) ?? notification.PublishedAt;
        notification.ExpiresAt = NormalizeToUtc(request.ExpiresAt);
        notification.UpdatedAt = DateTime.UtcNow;
        notification.CreatedByUserId ??= userId;

        await db.SaveChangesAsync();

        var response = await BuildDetailsResponseAsync(notification.Id, tenantId, userId, db, ignoreAudience: true);
        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteAdminNotificationAsync(Guid id,
                                                                    AppDbContext db,
                                                                    HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var notification = await db.TenantNotifications.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (notification is null)
            return Results.NotFound();

        notification.IsActive = false;
        notification.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> GetUnreadCountAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);
        var role = await GetCurrentRoleAsync(db, tenantId, userId);
        if (role is null)
            return Results.Unauthorized();

        var now = DateTime.UtcNow;
        var visible = ApplyVisibilityFilter(db.TenantNotifications, role.Value)
            .Where(x =>
                x.TenantId == tenantId &&
                x.IsActive &&
                x.PublishedAt <= now &&
                (!x.ExpiresAt.HasValue || x.ExpiresAt > now));

        var count = await visible.CountAsync(x =>
            !db.UserNotificationReads.Any(read =>
                read.TenantNotificationId == x.Id &&
                read.TenantId == tenantId &&
                read.UserId == userId));

        return Results.Ok(new UnreadNotificationCountResponse(count));
    }

    private static async Task<IResult> GetMyNotificationsAsync(AppDbContext db,
                                                               HttpContext context,
                                                               string status = "all",
                                                               TenantNotificationType? type = null,
                                                               int page = 1,
                                                               int pageSize = 20)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);
        var role = await GetCurrentRoleAsync(db, tenantId, userId);
        if (role is null)
            return Results.Unauthorized();

        var normalizedStatus = NormalizeStatus(status);
        if (normalizedStatus is null)
            return Results.BadRequest("Status filter must be all, read or unread.");

        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var now = DateTime.UtcNow;
        var query = ApplyVisibilityFilter(db.TenantNotifications, role.Value)
            .Where(x =>
                x.TenantId == tenantId &&
                x.IsActive &&
                x.PublishedAt <= now &&
                (!x.ExpiresAt.HasValue || x.ExpiresAt > now));

        if (type.HasValue)
            query = query.Where(x => x.Type == type.Value);

        if (normalizedStatus == "read")
        {
            query = query.Where(x => db.UserNotificationReads.Any(read =>
                read.TenantNotificationId == x.Id &&
                read.TenantId == tenantId &&
                read.UserId == userId));
        }
        else if (normalizedStatus == "unread")
        {
            query = query.Where(x => !db.UserNotificationReads.Any(read =>
                read.TenantNotificationId == x.Id &&
                read.TenantId == tenantId &&
                read.UserId == userId));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.IsHighlighted)
            .ThenByDescending(x => x.PublishedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.Type,
                x.Audience,
                x.Title,
                x.Summary,
                x.MediaId,
                MediaUrl = x.Media != null ? x.Media.Url : null,
                x.IsHighlighted,
                x.PublishedAt,
                x.ExpiresAt,
                ReadAt = db.UserNotificationReads
                    .Where(read =>
                        read.TenantNotificationId == x.Id &&
                        read.TenantId == tenantId &&
                        read.UserId == userId)
                    .Select(read => (DateTime?)read.ReadAt)
                    .FirstOrDefault()
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Results.Ok(new PagedResponse<NotificationListItemResponse>(
            page,
            pageSize,
            total,
            items.Select(x => new NotificationListItemResponse(
                x.Id,
                x.Type,
                x.Audience,
                x.Title,
                x.Summary,
                x.MediaId,
                x.MediaUrl,
                x.IsHighlighted,
                x.PublishedAt,
                x.ExpiresAt,
                x.ReadAt.HasValue,
                x.ReadAt
            )).ToList()));
    }

    private static async Task<IResult> GetByIdAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);
        var response = await BuildDetailsResponseAsync(id, tenantId, userId, db);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<IResult> MarkAsReadAsync(Guid id, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        var role = await GetCurrentRoleAsync(db, tenantId, userId);
        if (role is null)
            return Results.Unauthorized();

        var now = DateTime.UtcNow;
        var notificationExists = await ApplyVisibilityFilter(db.TenantNotifications, role.Value)
            .AnyAsync(x =>
                x.Id == id &&
                x.TenantId == tenantId &&
                x.IsActive &&
                x.PublishedAt <= now &&
                (!x.ExpiresAt.HasValue || x.ExpiresAt > now));

        if (!notificationExists)
            return Results.NotFound();

        var existing = await db.UserNotificationReads
            .FirstOrDefaultAsync(x => x.TenantNotificationId == id && x.TenantId == tenantId && x.UserId == userId);

        if (existing is null)
        {
            existing = new UserNotificationRead
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TenantNotificationId = id,
                UserId = userId,
                ReadAt = now
            };

            db.UserNotificationReads.Add(existing);
            await db.SaveChangesAsync();
        }

        return Results.Ok(new NotificationReadResponse(id, true, existing.ReadAt));
    }

    private static async Task<NotificationDetailsResponse?> BuildDetailsResponseAsync(Guid id,
                                                                                      Guid tenantId,
                                                                                      Guid userId,
                                                                                      AppDbContext db,
                                                                                      bool ignoreAudience = false)
    {
        var role = await GetCurrentRoleAsync(db, tenantId, userId);
        if (role is null)
            return null;

        var now = DateTime.UtcNow;
        var query = ignoreAudience ? db.TenantNotifications : ApplyVisibilityFilter(db.TenantNotifications, role.Value);

        var item = await query.Where(x =>
                x.Id == id &&
                x.TenantId == tenantId &&
                x.IsActive &&
                x.PublishedAt <= now &&
                (!x.ExpiresAt.HasValue || x.ExpiresAt > now))
            .Select(x => new
            {
                x.Id,
                x.Type,
                x.Audience,
                x.Title,
                x.Summary,
                x.Content,
                x.MediaId,
                MediaUrl = x.Media != null ? x.Media.Url : null,
                x.IsHighlighted,
                x.RelatedEntityType,
                x.RelatedEntityId,
                x.PublishedAt,
                x.ExpiresAt,
                ReadAt = db.UserNotificationReads
                    .Where(read =>
                        read.TenantNotificationId == x.Id &&
                        read.TenantId == tenantId &&
                        read.UserId == userId)
                    .Select(read => (DateTime?)read.ReadAt)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (item is null)
            return null;

        return new NotificationDetailsResponse(
            item.Id,
            item.Type,
            item.Audience,
            item.Title,
            item.Summary,
            item.Content,
            item.MediaId,
            item.MediaUrl,
            item.IsHighlighted,
            item.RelatedEntityType,
            item.RelatedEntityId,
            item.PublishedAt,
            item.ExpiresAt,
            item.ReadAt.HasValue,
            item.ReadAt
        );
    }

    private static async Task<IResult?> ValidateRequestAsync(Guid tenantId,
                                                             string title,
                                                             string content,
                                                             Guid? mediaId,
                                                             DateTime? publishedAt,
                                                             DateTime? expiresAt,
                                                             AppDbContext db)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Results.BadRequest("Notification title is required.");

        if (string.IsNullOrWhiteSpace(content))
            return Results.BadRequest("Notification content is required.");

        var publishedAtUtc = NormalizeToUtc(publishedAt) ?? DateTime.UtcNow;
        var expiresAtUtc = NormalizeToUtc(expiresAt);
        if (expiresAtUtc.HasValue && expiresAtUtc.Value <= publishedAtUtc)
            return Results.BadRequest("Notification expiration must be greater than publish date.");

        if (mediaId.HasValue)
        {
            var mediaExists = await db.TenantMedias.AnyAsync(x => x.Id == mediaId.Value && x.TenantId == tenantId);
            if (!mediaExists)
                return Results.BadRequest("Media does not belong to this tenant.");
        }

        return null;
    }

    private static IQueryable<TenantNotification> ApplyVisibilityFilter(IQueryable<TenantNotification> query, UserRole role)
    {
        return role == UserRole.Student
            ? query.Where(x => x.Audience == TenantNotificationAudience.StudentsOnly || x.Audience == TenantNotificationAudience.AllTenantUsers)
            : query.Where(x => x.Audience == TenantNotificationAudience.AllTenantUsers);
    }

    private static async Task<UserRole?> GetCurrentRoleAsync(AppDbContext db, Guid tenantId, Guid userId)
    {
        return await db.Users
            .Where(x => x.Id == userId && x.TenantId == tenantId && x.IsActive)
            .Select(x => (UserRole?)x.Role)
            .FirstOrDefaultAsync();
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim))
            throw new UnauthorizedAccessException("User not found in token.");

        return Guid.Parse(userIdClaim);
    }

    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return "all";

        var normalized = status.Trim().ToLowerInvariant();
        return normalized is "all" or "read" or "unread" ? normalized : null;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static DateTime? NormalizeToUtc(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value.Value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Local).ToUniversalTime()
        };
    }
}
