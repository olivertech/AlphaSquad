namespace AlphaSquad.Api.Features.Configurations;

using System.Text.Json;

/// <summary>
/// Registra os endpoints de configuracoes pessoais do dashboard.
/// Nesta V1, o foco e salvar os medidores escolhidos por cada usuario para a home do painel.
/// </summary>
public static class ConfigurationEndpoints
{
    private const int MaxDashboardMetrics = 5;

    public static IEndpointRouteBuilder MapConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/configurations")
            .WithTags("Configurations")
            .RequireAuthorization(AuthorizationPolicies.AdminOrTeacher);

        group.MapGet("/me/dashboard", GetMyDashboardConfigurationAsync)
            .WithName("GetMyDashboardConfiguration")
            .WithSummary("Retorna a configuracao pessoal dos medidores do dashboard.")
            .WithDescription("Devolve os medidores que o usuario autenticado escolheu para visualizar na home do painel administrativo.")
            .Produces<DashboardConfigurationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/me/dashboard", UpsertMyDashboardConfigurationAsync)
            .WithName("UpsertMyDashboardConfiguration")
            .WithSummary("Cria ou atualiza a configuracao pessoal dos medidores do dashboard.")
            .WithDescription("Permite que administradores e professores salvem quais medidores desejam ver na tela inicial do painel.")
            .Produces<DashboardConfigurationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    /// <summary>
    /// Retorna a selecao de medidores persistida para o usuario autenticado.
    /// Quando ainda nao existe configuracao salva, a API responde com lista vazia para o front aplicar o fallback padrao.
    /// </summary>
    private static async Task<IResult> GetMyDashboardConfigurationAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        var configuration = await db.Configurations
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.TenantId == tenantId &&
                x.UserId == userId &&
                x.Key == ConfigurationKeys.DashboardMetrics);

        return Results.Ok(BuildDashboardResponse(configuration, tenantId, userId));
    }

    /// <summary>
    /// Grava a escolha de medidores da home do dashboard para o usuario autenticado.
    /// A selecao e validada para evitar listas vazias, duplicadas ou com quantidade acima do limite da tela inicial.
    /// </summary>
    private static async Task<IResult> UpsertMyDashboardConfigurationAsync(UpdateDashboardConfigurationRequest request,
                                                                           AppDbContext db,
                                                                           HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);
        var normalizedMetricKeys = NormalizeMetricKeys(request.SelectedMetricKeys);

        if (normalizedMetricKeys.Count == 0)
            return Results.BadRequest("Selecione pelo menos um medidor para a tela inicial.");

        if (normalizedMetricKeys.Count > MaxDashboardMetrics)
            return Results.BadRequest($"Selecione no maximo {MaxDashboardMetrics} medidores para a tela inicial.");

        var configuration = await db.Configurations
            .FirstOrDefaultAsync(x =>
                x.TenantId == tenantId &&
                x.UserId == userId &&
                x.Key == ConfigurationKeys.DashboardMetrics);

        if (configuration is null)
        {
            configuration = new Configuration
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                Key = ConfigurationKeys.DashboardMetrics,
                ValueJson = JsonSerializer.Serialize(normalizedMetricKeys),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Configurations.Add(configuration);
        }
        else
        {
            configuration.ValueJson = JsonSerializer.Serialize(normalizedMetricKeys);
            configuration.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        return Results.Ok(BuildDashboardResponse(configuration, tenantId, userId));
    }

    /// <summary>
    /// Converte a configuracao persistida para o contrato de resposta usado pelo dashboard.
    /// </summary>
    private static DashboardConfigurationResponse BuildDashboardResponse(Configuration? configuration, Guid tenantId, Guid userId)
    {
        return new DashboardConfigurationResponse(
            configuration?.Id,
            tenantId,
            userId,
            ReadMetricKeys(configuration?.ValueJson),
            configuration?.UpdatedAt
        );
    }

    /// <summary>
    /// Normaliza as chaves de medidores para evitar espacos, vazios e duplicidade entre maiusculas e minusculas.
    /// </summary>
    private static List<string> NormalizeMetricKeys(IReadOnlyList<string>? metricKeys)
    {
        return metricKeys?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
            ?? [];
    }

    /// <summary>
    /// Desserializa o JSON armazenado em banco para a lista de chaves de medidores.
    /// Se o conteudo estiver invalido, a API responde com lista vazia para nao quebrar o dashboard.
    /// </summary>
    private static IReadOnlyList<string> ReadMetricKeys(string? valueJson)
    {
        if (string.IsNullOrWhiteSpace(valueJson))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<string>>(valueJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
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
}
