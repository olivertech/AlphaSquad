using AlphaSquad.Infrastructure.Features;

namespace AlphaSquad.Api.Features.Events;

public static class EventEndpoints
{
    /// <summary>
    /// Registra os endpoints do mural de eventos da academia.
    /// O modulo e opcional por tenant e pode ser usado tanto para comunicados quanto para eventos outdoor.
    /// </summary>
    public static IEndpointRouteBuilder MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/events")
            .WithTags("Events")
            .RequireAuthorization();

        group.MapGet("/", GetAllAsync)
            .WithName("GetEvents")
            .WithSummary("Lista os eventos do mural da academia.")
            .WithDescription("Retorna o feed paginado do modulo de eventos para o tenant atual, respeitando o pacote habilitado para a academia.")
            .Produces<PagedResponse<AcademyEventResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/feed", GetFeedAsync)
            .WithName("GetEventsFeed")
            .WithSummary("Lista os eventos do mural em formato cursor-based.")
            .WithDescription("Retorna o feed cursor-based para scroll infinito no app, mantendo ordenacao estavel e sinalizando se ainda existe mais conteudo.")
            .Produces<CursorFeedResponse<AcademyEventResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetEventById")
            .WithSummary("Busca um evento especifico do mural.")
            .WithDescription("Retorna o detalhamento de um evento do tenant atual, incluindo dados de participacao do usuario autenticado.")
            .Produces<AcademyEventResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("CreateEvent")
            .WithSummary("Cria um novo evento no mural da academia.")
            .WithDescription("Permite que perfis de gestao publiquem acoes institucionais e eventos outdoor no tenant atual.")
            .Produces<AcademyEventResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("UpdateEvent")
            .WithSummary("Atualiza um evento do mural da academia.")
            .WithDescription("Permite ajustar conteudo, status, data e configuracao de participacao de um evento existente.")
            .Produces<AcademyEventResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("DeleteEvent")
            .WithSummary("Remove um evento do mural da academia.")
            .WithDescription("Exclui um evento do tenant atual, incluindo seus registros de participacao associados.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/participate", ParticipateAsync)
            .WithName("ParticipateInEvent")
            .WithSummary("Matricula o aluno em um evento outdoor.")
            .WithDescription("Cria o registro de matricula do aluno em um evento outdoor habilitado, sem distribuir pontos nesse momento.")
            .Produces<AcademyEventParticipationResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/checkin", CheckInAsync)
            .WithName("CheckInInEvent")
            .WithSummary("Confirma a presença do aluno em um evento outdoor.")
            .WithDescription("Recebe a senha do evento, valida a matrícula do aluno e marca a presença para futura pontuação na conclusão administrativa.")
            .Produces<AcademyEventParticipationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/complete", CompleteEventAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("CompleteEvent")
            .WithSummary("Conclui um evento e distribui a pontuação dos presentes.")
            .WithDescription("Encerra o evento, inativa o registro e distribui os pontos de gamificação para os alunos que fizeram check-in com a senha.")
            .Produces<CompleteAcademyEventResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/participants", GetParticipantsAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("GetEventParticipants")
            .WithSummary("Lista os participantes inscritos em um evento.")
            .WithDescription("Retorna a lista de usuários matriculados no evento selecionado (Acesso restrito a gestores).")
            .Produces<List<AcademyEventParticipationResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/participants/{userId:guid}", AddParticipantAdminAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("AddParticipantToEvent")
            .WithSummary("Adiciona um participante a um evento.")
            .WithDescription("Permite que um gestor matricule um aluno manualmente em um evento outdoor apto.")
            .Produces<AcademyEventParticipationResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}/participants/{userId:guid}", RemoveParticipantAdminAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("RemoveParticipantFromEvent")
            .WithSummary("Remove um participante de um evento.")
            .WithDescription("Permite que um gestor cancele a matrícula de um aluno manualmente.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Retorna o feed paginado do mural para o tenant atual.
    /// Perfis de gestao podem incluir eventos inativos na consulta; alunos veem apenas itens ativos.
    /// </summary>
    private static async Task<IResult> GetAllAsync(AppDbContext db,
                                                   IFeatureAccessService featureAccessService,
                                                   HttpContext context,
                                                   bool? isActive = null,
                                                   bool? onlyOutdoor = null,
                                                   int page = 1,
                                                   int pageSize = 20)
    {
        var featureResult = await EnsureEventsFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        var currentUserId = GetUserId(context.User);
        var canManageEvents = CanManageEvents(context.User);

        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var effectiveIsActive = canManageEvents ? isActive : true;
        var connection = db.Database.GetDbConnection();

        const string countSql = @"SELECT COUNT(*)
                                  FROM academy_events e
                                  WHERE e.tenant_id = @TenantId
                                    AND (@IsActive IS NULL OR e.is_active = @IsActive)
                                    AND (@OnlyOutdoor IS NULL OR e.is_outdoor_event = @OnlyOutdoor)";

        const string itemsSql = @"SELECT e.id,
                                         e.title,
                                         e.description,
                                         e.media_id AS MediaId,
                                         m.url AS MediaUrl,
                                         e.location,
                                         e.starts_at AS StartsAt,
                                         e.ends_at AS EndsAt,
                                         e.is_outdoor_event AS IsOutdoorEvent,
                                         e.allow_participation AS AllowParticipation,
                                         e.is_active AS IsActive,
                                         CASE WHEN @CanManage = true THEN e.check_in_password ELSE NULL END AS CheckInPassword,
                                         e.is_completed AS IsCompleted,
                                         e.created_by_user_id AS CreatedByUserId,
                                         creator.name AS CreatedByUserName,
                                         COALESCE(participants.participant_count, 0) AS ParticipantCount,
                                         CASE WHEN current_participation.id IS NULL THEN false ELSE true END AS IsUserParticipating,
                                         e.created_at AS CreatedAt,
                                         e.updated_at AS UpdatedAt
                                  FROM academy_events e
                                  LEFT JOIN tenant_medias m
                                    ON m.id = e.media_id
                                   AND m.tenant_id = @TenantId
                                  JOIN users creator
                                    ON creator.id = e.created_by_user_id
                                   AND creator.tenant_id = @TenantId
                                  LEFT JOIN LATERAL (
                                      SELECT COUNT(*)::INTEGER AS participant_count
                                      FROM academy_event_participations ep
                                      WHERE ep.academy_event_id = e.id
                                        AND ep.tenant_id = @TenantId
                                  ) participants ON true
                                  LEFT JOIN LATERAL (
                                      SELECT ep.id
                                      FROM academy_event_participations ep
                                      WHERE ep.academy_event_id = e.id
                                        AND ep.tenant_id = @TenantId
                                        AND ep.user_id = @CurrentUserId
                                      LIMIT 1
                                  ) current_participation ON true
                                  WHERE e.tenant_id = @TenantId
                                    AND (@IsActive IS NULL OR e.is_active = @IsActive)
                                    AND (@OnlyOutdoor IS NULL OR e.is_outdoor_event = @OnlyOutdoor)
                                  ORDER BY COALESCE(e.starts_at, e.created_at) DESC, e.created_at DESC
                                  LIMIT @Limit OFFSET @Offset";

        var parameters = new
        {
            TenantId = tenantId,
            CurrentUserId = currentUserId,
            CanManage = canManageEvents,
            IsActive = effectiveIsActive,
            OnlyOutdoor = onlyOutdoor,
            Limit = pageSize,
            Offset = (page - 1) * pageSize
        };

        var total = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<AcademyEventResponse>(itemsSql, parameters);

        return Results.Ok(new PagedResponse<AcademyEventResponse>(page, pageSize, total, items.ToList()));
    }

    /// <summary>
    /// Retorna o mural em formato cursor-based para consumo de feed no app.
    /// O contrato foi pensado para scroll infinito e podera ser reaproveitado por loja e rede social.
    /// </summary>
    private static async Task<IResult> GetFeedAsync(AppDbContext db,
                                                    IFeatureAccessService featureAccessService,
                                                    HttpContext context,
                                                    string? cursor = null,
                                                    bool? onlyOutdoor = null,
                                                    int limit = 20)
    {
        var featureResult = await EnsureEventsFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        var currentUserId = GetUserId(context.User);
        var canManageEvents = CanManageEvents(context.User);
        var connection = db.Database.GetDbConnection();

        limit = limit is < 1 or > 50 ? 20 : limit;

        var cursorData = DecodeFeedCursor(cursor);

        const string sql = @"SELECT e.id,
                                    e.title,
                                    e.description,
                                    e.media_id AS MediaId,
                                    m.url AS MediaUrl,
                                    e.location,
                                    e.starts_at AS StartsAt,
                                    e.ends_at AS EndsAt,
                                    e.is_outdoor_event AS IsOutdoorEvent,
                                    e.allow_participation AS AllowParticipation,
                                    e.is_active AS IsActive,
                                    CASE WHEN @CanManage = true THEN e.check_in_password ELSE NULL END AS CheckInPassword,
                                    e.is_completed AS IsCompleted,
                                    e.created_by_user_id AS CreatedByUserId,
                                    creator.name AS CreatedByUserName,
                                    COALESCE(participants.participant_count, 0) AS ParticipantCount,
                                    CASE WHEN current_participation.id IS NULL THEN false ELSE true END AS IsUserParticipating,
                                    e.created_at AS CreatedAt,
                                    e.updated_at AS UpdatedAt,
                                    COALESCE(e.starts_at, e.created_at) AS FeedOrderAt
                             FROM academy_events e
                             LEFT JOIN tenant_medias m
                               ON m.id = e.media_id
                              AND m.tenant_id = @TenantId
                             JOIN users creator
                               ON creator.id = e.created_by_user_id
                              AND creator.tenant_id = @TenantId
                             LEFT JOIN LATERAL (
                                 SELECT COUNT(*)::INTEGER AS participant_count
                                 FROM academy_event_participations ep
                                 WHERE ep.academy_event_id = e.id
                                   AND ep.tenant_id = @TenantId
                             ) participants ON true
                             LEFT JOIN LATERAL (
                                 SELECT ep.id
                                 FROM academy_event_participations ep
                                 WHERE ep.academy_event_id = e.id
                                   AND ep.tenant_id = @TenantId
                                   AND ep.user_id = @CurrentUserId
                                 LIMIT 1
                             ) current_participation ON true
                             WHERE e.tenant_id = @TenantId
                               AND (@CanManage = true OR e.is_active = true)
                               AND (@OnlyOutdoor IS NULL OR e.is_outdoor_event = @OnlyOutdoor)
                               AND (
                                   @CursorFeedOrderAt IS NULL
                                   OR COALESCE(e.starts_at, e.created_at) < @CursorFeedOrderAt
                                   OR (
                                       COALESCE(e.starts_at, e.created_at) = @CursorFeedOrderAt
                                       AND (
                                           e.created_at < @CursorCreatedAt
                                           OR (e.created_at = @CursorCreatedAt AND e.id < @CursorId)
                                       )
                                   )
                               )
                             ORDER BY COALESCE(e.starts_at, e.created_at) DESC, e.created_at DESC, e.id DESC
                             LIMIT @LimitPlusOne";

        var rows = (await connection.QueryAsync<AcademyEventFeedRow>(sql, new
        {
            TenantId = tenantId,
            CurrentUserId = currentUserId,
            CanManage = canManageEvents,
            OnlyOutdoor = onlyOutdoor,
            CursorFeedOrderAt = cursorData?.FeedOrderAt,
            CursorCreatedAt = cursorData?.CreatedAt,
            CursorId = cursorData?.Id,
            LimitPlusOne = limit + 1
        })).ToList();

        var hasMore = rows.Count > limit;
        var pageRows = hasMore ? rows.Take(limit).ToList() : rows;

        string? nextCursor = null;
        if (hasMore && pageRows.Count > 0)
        {
            var lastItem = pageRows[^1];
            nextCursor = EncodeFeedCursor(lastItem.FeedOrderAt, lastItem.CreatedAt, lastItem.Id);
        }

        return Results.Ok(new CursorFeedResponse<AcademyEventResponse>(
            pageRows.Select(MapFeedRow).ToList(),
            nextCursor,
            hasMore));
    }

    /// <summary>
    /// Retorna o detalhe de um item do mural dentro do tenant atual.
    /// </summary>
    private static async Task<IResult> GetByIdAsync(Guid id,
                                                    AppDbContext db,
                                                    IFeatureAccessService featureAccessService,
                                                    HttpContext context)
    {
        var featureResult = await EnsureEventsFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        var currentUserId = GetUserId(context.User);
        var canManageEvents = CanManageEvents(context.User);
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT e.id,
                                    e.title,
                                    e.description,
                                    e.media_id AS MediaId,
                                    m.url AS MediaUrl,
                                    e.location,
                                    e.starts_at AS StartsAt,
                                    e.ends_at AS EndsAt,
                                    e.is_outdoor_event AS IsOutdoorEvent,
                                    e.allow_participation AS AllowParticipation,
                                    e.is_active AS IsActive,
                                    CASE WHEN @CanManage = true THEN e.check_in_password ELSE NULL END AS CheckInPassword,
                                    e.is_completed AS IsCompleted,
                                    e.created_by_user_id AS CreatedByUserId,
                                    creator.name AS CreatedByUserName,
                                    COALESCE(participants.participant_count, 0) AS ParticipantCount,
                                    CASE WHEN current_participation.id IS NULL THEN false ELSE true END AS IsUserParticipating,
                                    e.created_at AS CreatedAt,
                                    e.updated_at AS UpdatedAt
                             FROM academy_events e
                             LEFT JOIN tenant_medias m
                               ON m.id = e.media_id
                              AND m.tenant_id = @TenantId
                             JOIN users creator
                               ON creator.id = e.created_by_user_id
                              AND creator.tenant_id = @TenantId
                             LEFT JOIN LATERAL (
                                 SELECT COUNT(*)::INTEGER AS participant_count
                                 FROM academy_event_participations ep
                                 WHERE ep.academy_event_id = e.id
                                   AND ep.tenant_id = @TenantId
                             ) participants ON true
                             LEFT JOIN LATERAL (
                                 SELECT ep.id
                                 FROM academy_event_participations ep
                                 WHERE ep.academy_event_id = e.id
                                   AND ep.tenant_id = @TenantId
                                   AND ep.user_id = @CurrentUserId
                                 LIMIT 1
                             ) current_participation ON true
                             WHERE e.id = @Id
                               AND e.tenant_id = @TenantId
                               AND (@CanManage = true OR e.is_active = true)";

        var item = await connection.QueryFirstOrDefaultAsync<AcademyEventResponse>(sql, new
        {
            Id = id,
            TenantId = tenantId,
            CurrentUserId = currentUserId,
            CanManage = canManageEvents
        });

        if (item is null)
            return Results.NotFound();

        return Results.Ok(item);
    }

    /// <summary>
    /// Cria um novo item no mural institucional do tenant atual.
    /// </summary>
    private static async Task<IResult> CreateAsync(CreateAcademyEventRequest request,
                                                   AppDbContext db,
                                                   IFeatureAccessService featureAccessService,
                                                   HttpContext context)
    {
        var featureResult = await EnsureEventsFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        var createdByUserId = GetUserId(context.User);
        var startsAtUtc = NormalizeToUtc(request.StartsAt);
        var endsAtUtc = NormalizeToUtc(request.EndsAt);

        var validationResult = await ValidateRequestAsync(request.Title,
                                                          request.MediaId,
                                                          startsAtUtc,
                                                          endsAtUtc,
                                                          request.IsOutdoorEvent,
                                                          request.AllowParticipation,
                                                          tenantId,
                                                          db);

        if (validationResult is not null)
            return validationResult;

        var academyEvent = new AcademyEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = request.Title.Trim(),
            Description = NormalizeOptional(request.Description),
            MediaId = request.MediaId,
            Location = NormalizeOptional(request.Location),
            StartsAt = startsAtUtc,
            EndsAt = endsAtUtc,
            IsOutdoorEvent = request.IsOutdoorEvent,
            AllowParticipation = request.AllowParticipation,
            IsActive = request.IsActive,
            CheckInPassword = GenerateCheckInPassword(),
            IsCompleted = false,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        db.AcademyEvents.Add(academyEvent);
        await db.SaveChangesAsync();

        var response = await BuildResponseAsync(academyEvent.Id, tenantId, createdByUserId, db);
        return Results.Created($"/api/events/{academyEvent.Id}", response);
    }

    /// <summary>
    /// Atualiza um evento existente do mural do tenant atual.
    /// </summary>
    private static async Task<IResult> UpdateAsync(Guid id,
                                                   UpdateAcademyEventRequest request,
                                                   AppDbContext db,
                                                   IFeatureAccessService featureAccessService,
                                                   HttpContext context)
    {
        var featureResult = await EnsureEventsFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        var currentUserId = GetUserId(context.User);
        var academyEvent = await db.AcademyEvents.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        var startsAtUtc = NormalizeToUtc(request.StartsAt);
        var endsAtUtc = NormalizeToUtc(request.EndsAt);

        if (academyEvent is null)
            return Results.NotFound();

        var validationResult = await ValidateRequestAsync(request.Title,
                                                          request.MediaId,
                                                          startsAtUtc,
                                                          endsAtUtc,
                                                          request.IsOutdoorEvent,
                                                          request.AllowParticipation,
                                                          tenantId,
                                                          db);

        if (validationResult is not null)
            return validationResult;

        if (academyEvent.IsCompleted && request.IsActive)
            return Results.BadRequest("Completed events cannot be reactivated.");

        academyEvent.Title = request.Title.Trim();
        academyEvent.Description = NormalizeOptional(request.Description);
        academyEvent.MediaId = request.MediaId;
        academyEvent.Location = NormalizeOptional(request.Location);
        academyEvent.StartsAt = startsAtUtc;
        academyEvent.EndsAt = endsAtUtc;
        academyEvent.IsOutdoorEvent = request.IsOutdoorEvent;
        academyEvent.AllowParticipation = request.AllowParticipation;
        academyEvent.IsActive = request.IsActive;
        academyEvent.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var response = await BuildResponseAsync(academyEvent.Id, tenantId, currentUserId, db);
        return Results.Ok(response);
    }

    /// <summary>
    /// Remove um evento e suas participacoes associadas.
    /// </summary>
    private static async Task<IResult> DeleteAsync(Guid id,
                                                   AppDbContext db,
                                                   IFeatureAccessService featureAccessService,
                                                   HttpContext context)
    {
        var featureResult = await EnsureEventsFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        var academyEvent = await db.AcademyEvents
            .Include(x => x.Participations)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (academyEvent is null)
            return Results.NotFound();

        if (academyEvent.StartsAt.HasValue && academyEvent.StartsAt.Value <= DateTime.UtcNow)
            return Results.BadRequest("Started events must be completed instead of being removed.");

        if (academyEvent.Participations.Count > 0)
            db.AcademyEventParticipations.RemoveRange(academyEvent.Participations);

        db.AcademyEvents.Remove(academyEvent);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    /// <summary>
    /// Matricula o aluno em um evento outdoor apto a receber confirmacao de presenca.
    /// O check-in por senha e a pontuacao ficam para etapas posteriores do fluxo.
    /// </summary>
    private static async Task<IResult> ParticipateAsync(Guid id,
                                                        AppDbContext db,
                                                        IFeatureAccessService featureAccessService,
                                                        HttpContext context)
    {
        var featureResult = await EnsureEventsFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);
        var now = DateTime.UtcNow;

        var appUser = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.IsActive);
        if (appUser is null)
            return Results.Unauthorized();

        if (appUser.Role != UserRole.Student)
            return Results.Forbid();

        var academyEvent = await db.AcademyEvents.FirstOrDefaultAsync(x =>
            x.Id == id &&
            x.TenantId == tenantId &&
            x.IsActive &&
            x.IsOutdoorEvent &&
            x.AllowParticipation);

        if (academyEvent is null)
            return Results.NotFound();

        var alreadyParticipating = await db.AcademyEventParticipations.AnyAsync(x =>
            x.TenantId == tenantId &&
            x.AcademyEventId == academyEvent.Id &&
            x.UserId == userId);

        if (alreadyParticipating)
            return Results.Conflict("User is already enrolled in this event.");

        var participation = new AcademyEventParticipation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AcademyEventId = academyEvent.Id,
            UserId = userId,
            ParticipatedAt = now,
            IsPresent = false
        };

        db.AcademyEventParticipations.Add(participation);
        await db.SaveChangesAsync();

        return Results.Created($"/api/events/{academyEvent.Id}/participate", new AcademyEventParticipationResponse(
            participation.Id,
            academyEvent.Id,
            academyEvent.Title,
            appUser.Id,
            appUser.Name,
            appUser.Email,
            participation.IsPresent,
            participation.ParticipatedAt
        ));
    }

    /// <summary>
    /// Confirma a presenca do aluno em um evento outdoor usando a senha divulgada pela academia.
    /// A pontuacao continua pendente ate a conclusao administrativa do evento.
    /// </summary>
    private static async Task<IResult> CheckInAsync(Guid id,
                                                    EventCheckInRequest request,
                                                    AppDbContext db,
                                                    IFeatureAccessService featureAccessService,
                                                    HttpContext context)
    {
        var featureResult = await EnsureEventsFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        if (string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest("Event password is required.");

        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);
        var now = DateTime.UtcNow;

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.IsActive);
        if (user is null)
            return Results.Unauthorized();

        if (user.Role != UserRole.Student)
            return Results.Forbid();

        var academyEvent = await db.AcademyEvents.FirstOrDefaultAsync(x =>
            x.Id == id &&
            x.TenantId == tenantId &&
            x.IsActive &&
            !x.IsCompleted &&
            x.IsOutdoorEvent &&
            x.AllowParticipation);

        if (academyEvent is null)
            return Results.NotFound();

        if (academyEvent.StartsAt.HasValue && academyEvent.StartsAt.Value > now)
            return Results.BadRequest("Check-in is only available after the event starts.");

        if (!string.Equals(academyEvent.CheckInPassword, request.Password.Trim(), StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest("Invalid event password.");

        var participation = await db.AcademyEventParticipations.FirstOrDefaultAsync(x =>
            x.TenantId == tenantId &&
            x.AcademyEventId == academyEvent.Id &&
            x.UserId == userId);

        if (participation is null)
            return Results.BadRequest("User is not enrolled in this event.");

        if (participation.IsPresent)
            return Results.Conflict("Presence has already been confirmed for this user.");

        participation.IsPresent = true;
        await db.SaveChangesAsync();

        return Results.Ok(new AcademyEventParticipationResponse(
            participation.Id,
            academyEvent.Id,
            academyEvent.Title,
            user.Id,
            user.Name,
            user.Email,
            participation.IsPresent,
            participation.ParticipatedAt
        ));
    }

    /// <summary>
    /// Conclui o evento, inativa o registro e distribui os pontos de gamificacao para os presentes.
    /// </summary>
    private static async Task<IResult> CompleteEventAsync(Guid id,
                                                          AppDbContext db,
                                                          IFeatureAccessService featureAccessService,
                                                          IGamificationService gamificationService,
                                                          HttpContext context)
    {
        var featureResult = await EnsureEventsFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        var completedAt = DateTime.UtcNow;

        var academyEvent = await db.AcademyEvents
            .Include(x => x.Participations)
            .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (academyEvent is null)
            return Results.NotFound();

        if (academyEvent.IsCompleted)
            return Results.BadRequest("Event has already been completed.");

        if (!academyEvent.IsActive)
            return Results.BadRequest("Only active events can be completed.");

        if (academyEvent.StartsAt.HasValue && academyEvent.StartsAt.Value > completedAt)
            return Results.BadRequest("The event can only be completed after it starts.");

        var awardedParticipantsCount = 0;

        if (academyEvent.IsOutdoorEvent && academyEvent.AllowParticipation)
        {
            foreach (var participation in academyEvent.Participations.Where(x => x.IsPresent))
            {
                var awarded = await gamificationService.AwardEventAsync(
                    tenantId,
                    participation.UserId,
                    GamificationEventType.OutdoorEventParticipation,
                    "academy_event_completion",
                    participation.Id,
                    completedAt,
                    "Outdoor event presence confirmed by administrator.");

                if (awarded)
                    awardedParticipantsCount++;
            }
        }

        academyEvent.IsCompleted = true;
        academyEvent.IsActive = false;
        academyEvent.UpdatedAt = completedAt;

        await db.SaveChangesAsync();

        return Results.Ok(new CompleteAcademyEventResponse(
            academyEvent.Id,
            academyEvent.Title,
            awardedParticipantsCount,
            academyEvent.IsCompleted,
            academyEvent.IsActive
        ));
    }

    /// <summary>
    /// Reaplica a projecao de leitura apos operacoes de escrita.
    /// Isso mantem respostas consistentes entre create, update e consultas.
    /// </summary>
    private static async Task<AcademyEventResponse> BuildResponseAsync(Guid eventId, Guid tenantId, Guid currentUserId, AppDbContext db)
    {
        var connection = db.Database.GetDbConnection();
        var canManageEvents = await db.Users
            .Where(x => x.Id == currentUserId && x.TenantId == tenantId && x.IsActive)
            .Select(x => x.Role == UserRole.Admin || x.Role == UserRole.Teacher)
            .FirstOrDefaultAsync();

        const string sql = @"SELECT e.id,
                                    e.title,
                                    e.description,
                                    e.media_id AS MediaId,
                                    m.url AS MediaUrl,
                                    e.location,
                                    e.starts_at AS StartsAt,
                                    e.ends_at AS EndsAt,
                                    e.is_outdoor_event AS IsOutdoorEvent,
                                    e.allow_participation AS AllowParticipation,
                                    e.is_active AS IsActive,
                                    CASE WHEN @CanManage = true THEN e.check_in_password ELSE NULL END AS CheckInPassword,
                                    e.is_completed AS IsCompleted,
                                    e.created_by_user_id AS CreatedByUserId,
                                    creator.name AS CreatedByUserName,
                                    COALESCE(participants.participant_count, 0) AS ParticipantCount,
                                    CASE WHEN current_participation.id IS NULL THEN false ELSE true END AS IsUserParticipating,
                                    e.created_at AS CreatedAt,
                                    e.updated_at AS UpdatedAt
                             FROM academy_events e
                             LEFT JOIN tenant_medias m
                               ON m.id = e.media_id
                              AND m.tenant_id = @TenantId
                             JOIN users creator
                               ON creator.id = e.created_by_user_id
                              AND creator.tenant_id = @TenantId
                             LEFT JOIN LATERAL (
                                 SELECT COUNT(*)::INTEGER AS participant_count
                                 FROM academy_event_participations ep
                                 WHERE ep.academy_event_id = e.id
                                   AND ep.tenant_id = @TenantId
                             ) participants ON true
                             LEFT JOIN LATERAL (
                                 SELECT ep.id
                                 FROM academy_event_participations ep
                                 WHERE ep.academy_event_id = e.id
                                   AND ep.tenant_id = @TenantId
                                   AND ep.user_id = @CurrentUserId
                                 LIMIT 1
                             ) current_participation ON true
                             WHERE e.id = @EventId
                               AND e.tenant_id = @TenantId";

        return (await connection.QueryFirstAsync<AcademyEventResponse>(sql, new
        {
            EventId = eventId,
            TenantId = tenantId,
            CurrentUserId = currentUserId,
            CanManage = canManageEvents
        }))!;
    }

    /// <summary>
    /// Valida as principais regras de consistencia do payload de eventos.
    /// </summary>
    private static async Task<IResult?> ValidateRequestAsync(string title,
                                                             Guid? mediaId,
                                                             DateTime? startsAt,
                                                             DateTime? endsAt,
                                                             bool isOutdoorEvent,
                                                             bool allowParticipation,
                                                             Guid tenantId,
                                                             AppDbContext db)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Results.BadRequest("Event title is required.");

        if (!startsAt.HasValue && endsAt.HasValue)
            return Results.BadRequest("End date requires a start date.");

        if (startsAt.HasValue && endsAt.HasValue && endsAt.Value <= startsAt.Value)
            return Results.BadRequest("End date must be greater than start date.");

        if (allowParticipation && !isOutdoorEvent)
            return Results.BadRequest("Only outdoor events can allow participation in this version.");

        if (mediaId.HasValue)
        {
            var mediaExists = await db.TenantMedias.AnyAsync(x => x.Id == mediaId.Value && x.TenantId == tenantId);
            if (!mediaExists)
                return Results.BadRequest("Media does not belong to this tenant.");
        }

        return null;
    }

    /// <summary>
    /// Verifica se o tenant atual contratou o modulo opcional de eventos.
    /// </summary>
    private static async Task<IResult?> EnsureEventsFeatureEnabledAsync(IFeatureAccessService featureAccessService, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var isEnabled = await featureAccessService.HasFeatureAsync(tenantId, FeatureCodes.Events);

        if (isEnabled)
            return null;

        return Results.Problem(
            title: "Feature not enabled",
            detail: "The EVENTS module is not enabled for the current tenant.",
            statusCode: StatusCodes.Status403Forbidden);
    }

    /// <summary>
    /// Identifica se o usuario autenticado pode gerenciar o mural da academia.
    /// </summary>
    private static bool CanManageEvents(ClaimsPrincipal user)
    {
        return user.IsInRole(nameof(UserRole.Admin)) || user.IsInRole(nameof(UserRole.Teacher));
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
    /// Normaliza campos opcionais para evitar persistencia de espacos em branco.
    /// </summary>
    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Converte datas locais ou sem fuso para UTC antes da persistencia em colunas timestamptz.
    /// Isso evita inconsistencias entre o dashboard web e o PostgreSQL.
    /// </summary>
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

    /// <summary>
    /// Gera uma senha curta e amigavel para uso no check-in do evento.
    /// A senha pode ser divulgada pelo administrador no dia da acao.
    /// </summary>
    private static string GenerateCheckInPassword()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return string.Create(6, alphabet, static (buffer, source) =>
        {
            for (var index = 0; index < buffer.Length; index++)
                buffer[index] = source[Random.Shared.Next(source.Length)];
        });
    }

    /// <summary>
    /// Converte a linha interna do feed no contrato publico usado pelo app.
    /// O campo auxiliar de ordenacao nao faz parte da resposta final.
    /// </summary>
    private static AcademyEventResponse MapFeedRow(AcademyEventFeedRow row)
    {
        return new AcademyEventResponse(
            row.Id,
            row.Title,
            row.Description,
            row.MediaId,
            row.MediaUrl,
            row.Location,
            row.StartsAt,
            row.EndsAt,
            row.IsOutdoorEvent,
            row.AllowParticipation,
            row.IsActive,
            row.CheckInPassword,
            row.IsCompleted,
            row.CreatedByUserId,
            row.CreatedByUserName,
            row.ParticipantCount,
            row.IsUserParticipating,
            row.CreatedAt,
            row.UpdatedAt
        );
    }

    /// <summary>
    /// Gera um cursor opaco a partir das colunas que compoem a ordenacao do feed.
    /// O cliente trata o cursor como string sem precisar conhecer sua estrutura interna.
    /// </summary>
    private static string EncodeFeedCursor(DateTime feedOrderAt, DateTime createdAt, Guid id)
    {
        var rawCursor = $"{feedOrderAt.Ticks}|{createdAt.Ticks}|{id:N}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(rawCursor));
    }

    /// <summary>
    /// Decodifica o cursor recebido do app para continuar a busca a partir do ultimo item exibido.
    /// Cursors invalidos sao ignorados para evitar quebra de experiencia no cliente.
    /// </summary>
    private static AcademyEventFeedCursor? DecodeFeedCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
            return null;

        try
        {
            var rawCursor = Encoding.UTF8.GetString(Convert.FromBase64String(cursor.Trim()));
            var parts = rawCursor.Split('|', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 3)
                return null;

            if (!long.TryParse(parts[0], out var feedOrderTicks) ||
                !long.TryParse(parts[1], out var createdAtTicks) ||
                !Guid.TryParseExact(parts[2], "N", out var id))
            {
                return null;
            }

            return new AcademyEventFeedCursor(
                new DateTime(feedOrderTicks, DateTimeKind.Utc),
                new DateTime(createdAtTicks, DateTimeKind.Utc),
                id
            );
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Lista os usuários inscritos em um evento específico.
    /// </summary>
    private static async Task<IResult> GetParticipantsAsync(Guid id,
                                                            AppDbContext db,
                                                            IFeatureAccessService featureAccessService,
                                                            HttpContext context)
    {
        var featureResult = await EnsureEventsFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();
        
        var academyEvent = await db.AcademyEvents.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (academyEvent is null)
            return Results.NotFound();

        var participations = await db.AcademyEventParticipations
            .Include(x => x.User)
            .Where(x => x.AcademyEventId == id && x.TenantId == tenantId)
            .Select(p => new AcademyEventParticipationResponse(
                p.Id,
                p.AcademyEventId,
                academyEvent.Title,
                p.UserId,
                p.User!.Name,
                p.User.Email,
                p.IsPresent,
                p.ParticipatedAt))
            .ToListAsync();

        return Results.Ok(participations);
    }

    /// <summary>
    /// Permite que um gestor matricule um aluno manualmente em um evento.
    /// </summary>
    private static async Task<IResult> AddParticipantAdminAsync(Guid id,
                                                                Guid userId,
                                                                AppDbContext db,
                                                                IFeatureAccessService featureAccessService,
                                                                HttpContext context)
    {
        var featureResult = await EnsureEventsFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();

        var user = await db.Users.FirstOrDefaultAsync(x =>
            x.Id == userId &&
            x.TenantId == tenantId &&
            x.IsActive &&
            x.Role == UserRole.Student);
        if (user is null)
            return Results.BadRequest("Student not found or inactive.");

        var academyEvent = await db.AcademyEvents.FirstOrDefaultAsync(x =>
            x.Id == id &&
            x.TenantId == tenantId &&
            x.IsActive &&
            !x.IsCompleted &&
            x.IsOutdoorEvent &&
            x.AllowParticipation);
        if (academyEvent is null)
            return Results.NotFound();

        var alreadyParticipating = await db.AcademyEventParticipations.AnyAsync(x => 
            x.AcademyEventId == id && x.UserId == userId && x.TenantId == tenantId);

        if (alreadyParticipating)
            return Results.Conflict("User is already participating in this event.");

        var participation = new AcademyEventParticipation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AcademyEventId = id,
            UserId = userId,
            ParticipatedAt = DateTime.UtcNow,
            IsPresent = false
        };

        db.AcademyEventParticipations.Add(participation);
        await db.SaveChangesAsync();

        return Results.Created($"/api/events/{id}/participants/{userId}", new AcademyEventParticipationResponse(
            participation.Id,
            academyEvent.Id,
            academyEvent.Title,
            user.Id,
            user.Name,
            user.Email,
            participation.IsPresent,
            participation.ParticipatedAt
        ));
    }

    /// <summary>
    /// Permite que um gestor cancele a matrícula de um aluno manualmente.
    /// </summary>
    private static async Task<IResult> RemoveParticipantAdminAsync(Guid id,
                                                                   Guid userId,
                                                                   AppDbContext db,
                                                                   IFeatureAccessService featureAccessService,
                                                                   HttpContext context)
    {
        var featureResult = await EnsureEventsFeatureEnabledAsync(featureAccessService, context);
        if (featureResult is not null)
            return featureResult;

        var tenantId = context.GetTenantId();

        var participation = await db.AcademyEventParticipations.FirstOrDefaultAsync(x => 
            x.AcademyEventId == id && x.UserId == userId && x.TenantId == tenantId);

        if (participation is null)
            return Results.NotFound("Participation not found.");

        db.AcademyEventParticipations.Remove(participation);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    /// <summary>
    /// Projecao interna usada apenas para montar o feed cursor-based.
    /// Ela inclui o campo auxiliar de ordenacao, que nao deve vazar para o contrato externo.
    /// </summary>
    private sealed record AcademyEventFeedRow(
        Guid Id,
        string Title,
        string? Description,
        Guid? MediaId,
        string? MediaUrl,
        string? Location,
        DateTime? StartsAt,
        DateTime? EndsAt,
        bool IsOutdoorEvent,
        bool AllowParticipation,
        bool IsActive,
        string? CheckInPassword,
        bool IsCompleted,
        Guid CreatedByUserId,
        string CreatedByUserName,
        int ParticipantCount,
        bool IsUserParticipating,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        DateTime FeedOrderAt
    );

    /// <summary>
    /// Estrutura interna do cursor do feed.
    /// Ela replica exatamente as colunas usadas na ordenacao para garantir continuidade estavel.
    /// </summary>
    private sealed record AcademyEventFeedCursor(
        DateTime FeedOrderAt,
        DateTime CreatedAt,
        Guid Id
    );
}
