namespace AlphaSquad.Api.Features.Legal;

/// <summary>
/// Registra os endpoints do modulo de textos legais do tenant.
/// O app consome esses textos para exibir Termos de Uso e Politica de Privacidade aos usuarios autenticados.
/// </summary>
public static class LegalEndpoints
{
    public static IEndpointRouteBuilder MapLegalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/legal")
            .WithTags("Legal")
            .RequireAuthorization();

        group.MapGet("/current", GetCurrentAsync)
            .WithName("GetCurrentLegalContent")
            .WithSummary("Retorna os textos legais do tenant atual.")
            .WithDescription("Devolve Termos de Uso e Politica de Privacidade do tenant autenticado para exibicao no app.")
            .Produces<TenantLegalContentResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/current", UpsertAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("UpsertCurrentLegalContent")
            .WithSummary("Cria ou atualiza os textos legais do tenant atual.")
            .WithDescription("Permite que apenas administradores mantenham os textos de Termos de Uso e Politica de Privacidade exibidos no app.")
            .Produces<TenantLegalContentResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    /// <summary>
    /// Retorna os textos legais atuais do tenant.
    /// Quando o tenant ainda nao configurou esses textos, a API responde com estrutura vazia para simplificar o consumo no app.
    /// </summary>
    private static async Task<IResult> GetCurrentAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var response = await BuildResponseAsync(tenantId, db);

        return Results.Ok(response ?? new TenantLegalContentResponse(
            null,
            tenantId,
            string.Empty,
            string.Empty,
            null,
            null,
            null,
            null
        ));
    }

    /// <summary>
    /// Cria ou atualiza o registro legal do tenant atual.
    /// A V1 trabalha com um unico registro por tenant para facilitar manutencao administrativa e exibicao no app.
    /// </summary>
    private static async Task<IResult> UpsertAsync(UpdateTenantLegalContentRequest request,
                                                   AppDbContext db,
                                                   HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(request.TermsOfUse))
            return Results.BadRequest("TermsOfUse is required.");

        if (string.IsNullOrWhiteSpace(request.PrivacyPolicy))
            return Results.BadRequest("PrivacyPolicy is required.");

        var tenantId = context.GetTenantId();
        var actorUserId = GetUserId(context.User);

        var legalContent = await db.Set<TenantLegalContent>()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId);

        if (legalContent is null)
        {
            legalContent = new TenantLegalContent
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TermsOfUse = request.TermsOfUse.Trim(),
                PrivacyPolicy = request.PrivacyPolicy.Trim(),
                UpdatedByUserId = actorUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Set<TenantLegalContent>().Add(legalContent);
        }
        else
        {
            legalContent.TermsOfUse = request.TermsOfUse.Trim();
            legalContent.PrivacyPolicy = request.PrivacyPolicy.Trim();
            legalContent.UpdatedByUserId = actorUserId;
            legalContent.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        var response = await BuildResponseAsync(tenantId, db);
        return Results.Ok(response);
    }

    /// <summary>
    /// Reconstroi a resposta do modulo juntando o registro legal e o nome do administrador que o atualizou por ultimo.
    /// </summary>
    private static async Task<TenantLegalContentResponse?> BuildResponseAsync(Guid tenantId, AppDbContext db)
    {
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT lc.id,
                                    lc.tenant_id AS TenantId,
                                    lc.terms_of_use AS TermsOfUse,
                                    lc.privacy_policy AS PrivacyPolicy,
                                    lc.updated_by_user_id AS UpdatedByUserId,
                                    u.name AS UpdatedByUserName,
                                    lc.created_at AS CreatedAt,
                                    lc.updated_at AS UpdatedAt
                             FROM tenant_legal_contents lc
                             LEFT JOIN users u
                               ON u.id = lc.updated_by_user_id
                              AND u.tenant_id = lc.tenant_id
                             WHERE lc.tenant_id = @TenantId";

        return await connection.QueryFirstOrDefaultAsync<TenantLegalContentResponse>(sql, new
        {
            TenantId = tenantId
        });
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
}
