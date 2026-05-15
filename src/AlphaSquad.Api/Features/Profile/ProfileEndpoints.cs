namespace AlphaSquad.Api.Features.Profile;

using System.Net.Mail;
using System.Text.RegularExpressions;

/// <summary>
/// Registra os endpoints do modulo de profile do usuario autenticado.
/// Este modulo concentra os dados pessoais usados diretamente na experiencia do app.
/// </summary>
public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profile")
            .WithTags("Profile")
            .RequireAuthorization();

        group.MapGet("/me", GetMeAsync)
            .WithName("GetMyProfile")
            .WithSummary("Retorna o profile do usuario autenticado.")
            .WithDescription("Consolida nome, e-mail, username, foto de profile e plano ativo do usuario da sessao atual.")
            .Produces<ProfileResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/me", UpdateMeAsync)
            .WithName("UpdateMyProfile")
            .WithSummary("Atualiza os dados basicos do profile do usuario autenticado.")
            .WithDescription("Permite alterar nome e username do proprio usuario, respeitando o tenant da sessao e a unicidade do username.")
            .Produces<ProfileResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/me/email", UpdateEmailAsync)
            .WithName("UpdateMyProfileEmail")
            .WithSummary("Atualiza o e-mail do usuario autenticado.")
            .WithDescription("Exige o e-mail atual e a senha atual para confirmar a identidade do usuario antes de aplicar a troca para um novo e-mail unico no tenant.")
            .Produces<ProfileResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/me/password", UpdatePasswordAsync)
            .WithName("UpdateMyProfilePassword")
            .WithSummary("Atualiza a senha do usuario autenticado pela area de profile.")
            .WithDescription("Permite trocar a propria senha dentro do modulo de profile, exigindo a senha atual e revogando as sessoes ativas do usuario.")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/me/photo", UpdatePhotoAsync)
            .DisableAntiforgery()
            .WithName("UpdateMyProfilePhoto")
            .WithSummary("Atualiza a foto de profile do usuario autenticado.")
            .WithDescription("Faz upload da nova foto para o storage, substitui o vinculo atual do profile e remove a imagem anterior quando existir.")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ProfileResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapDelete("/me/photo", DeletePhotoAsync)
            .WithName("DeleteMyProfilePhoto")
            .WithSummary("Remove a foto de profile do usuario autenticado.")
            .WithDescription("Limpa o vinculo da foto de profile e remove o arquivo associado do storage quando existir.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }

    /// <summary>
    /// Retorna o profile do usuario da sessao atual.
    /// Usa uma unica consulta para unir os dados nucleares do usuario com os dados complementares do profile.
    /// </summary>
    private static async Task<IResult> GetMeAsync(AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        var profile = await BuildResponseAsync(userId, tenantId, db);
        if (profile is null)
            return Results.Unauthorized();

        return Results.Ok(profile);
    }

    /// <summary>
    /// Atualiza os dados basicos do profile.
    /// Neste recorte, o proprio usuario pode editar nome, username, celular e data de nascimento.
    /// </summary>
    private static async Task<IResult> UpdateMeAsync(UpdateProfileRequest request, AppDbContext db, HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);
        var normalizedUsername = NormalizeUsername(request.Username);
        var normalizedPhoneNumber = NormalizePhoneNumber(request.PhoneNumber);
        var normalizedBirthDate = NormalizeBirthDate(request.BirthDate);
        var validation = ValidateProfileUpdateRequest(request.Name, normalizedUsername, normalizedPhoneNumber, normalizedBirthDate);
        if (validation is not null)
            return validation;

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.IsActive);
        if (user is null)
            return Results.Unauthorized();

        if (!string.IsNullOrWhiteSpace(normalizedUsername))
        {
            var usernameInUse = await db.Set<UserProfile>().AnyAsync(x =>
                x.TenantId == tenantId &&
                x.UserId != userId &&
                x.Username == normalizedUsername);

            if (usernameInUse)
                return Results.Conflict("Username is already in use in this tenant.");
        }

        user.Name = request.Name.Trim();

        var profile = await db.Set<UserProfile>().FirstOrDefaultAsync(x => x.UserId == userId && x.TenantId == tenantId);
        if (profile is null)
        {
            profile = new UserProfile
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            db.Set<UserProfile>().Add(profile);
        }

        profile.Username = normalizedUsername;
        profile.PhoneNumber = normalizedPhoneNumber;
        profile.BirthDate = normalizedBirthDate;
        profile.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        var response = await BuildResponseAsync(userId, tenantId, db);
        return Results.Ok(response!);
    }

    /// <summary>
    /// Atualiza o e-mail do proprio usuario com validacoes de identidade e unicidade.
    /// </summary>
    private static async Task<IResult> UpdateEmailAsync(UpdateProfileEmailRequest request,
                                                        AppDbContext db,
                                                        IBCryptPasswordHasher passwordHasher,
                                                        HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);
        var currentEmail = request.CurrentEmail.Trim().ToLowerInvariant();
        var newEmail = request.NewEmail.Trim().ToLowerInvariant();
        var validation = ValidateEmailChangeRequest(request.CurrentEmail, request.NewEmail, request.CurrentPassword);
        if (validation is not null)
            return validation;

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.IsActive);
        if (user is null)
            return Results.Unauthorized();

        if (user.Email != currentEmail)
            return Results.BadRequest("Current email does not match the authenticated user.");

        if (currentEmail == newEmail)
            return Results.BadRequest("New email must be different from current email.");

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return Results.BadRequest("Current password is incorrect.");

        var emailInUse = await db.Users.AnyAsync(x => x.TenantId == tenantId && x.Id != userId && x.Email == newEmail);
        if (emailInUse)
            return Results.Conflict("A user with this e-mail already exists in this tenant.");

        user.Email = newEmail;
        await db.SaveChangesAsync();

        var response = await BuildResponseAsync(userId, tenantId, db);
        return Results.Ok(response!);
    }

    /// <summary>
    /// Atualiza a senha pela propria area de profile.
    /// A regra e equivalente ao endpoint de auth, mas fica exposta onde o app espera esse ajuste de conta.
    /// </summary>
    private static async Task<IResult> UpdatePasswordAsync(ChangePasswordRequest request,
                                                           AppDbContext db,
                                                           IBCryptPasswordHasher passwordHasher,
                                                           HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            return Results.BadRequest("Current and new passwords are required.");

        if (request.NewPassword.Trim().Length < 6)
            return Results.BadRequest("New password must have at least 6 characters.");

        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.IsActive);
        if (user is null)
            return Results.Unauthorized();

        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return Results.BadRequest("Current password is incorrect.");

        if (passwordHasher.Verify(request.NewPassword, user.PasswordHash))
            return Results.BadRequest("New password must be different from current password.");

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);

        var tokens = await db.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await db.SaveChangesAsync();

        return Results.Ok("Password changed successfully and all active sessions were terminated.");
    }

    /// <summary>
    /// Atualiza a foto de profile do usuario autenticado.
    /// O arquivo anterior e removido apenas depois de o novo vinculo estar salvo com sucesso.
    /// </summary>
    private static async Task<IResult> UpdatePhotoAsync(IFormFile file,
                                                        AppDbContext db,
                                                        IObjectStorageService storage,
                                                        HttpContext context)
    {
        if (file is null || file.Length == 0)
            return Results.BadRequest("Profile photo file is required.");

        var tenantId = context.GetTenantId();
        var tenantSlug = context.GetTenantSlug();
        var userId = GetUserId(context.User);

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId && x.IsActive);
        if (user is null)
            return Results.Unauthorized();

        var profile = await db.Set<UserProfile>().FirstOrDefaultAsync(x => x.UserId == userId && x.TenantId == tenantId);
        if (profile is null)
        {
            profile = new UserProfile
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            db.Set<UserProfile>().Add(profile);
        }

        TenantMedia? oldMedia = null;
        if (profile.ProfileMediaId.HasValue)
        {
            oldMedia = await db.TenantMedias
                .FirstOrDefaultAsync(x => x.Id == profile.ProfileMediaId.Value && x.TenantId == tenantId);
        }

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"profile_{userId}{extension}";
        var path = $"tenants/{tenantSlug}/profiles";

        using var stream = file.OpenReadStream();
        var uploadResult = await storage.UploadAsync(stream, fileName, file.ContentType, path);

        var media = new TenantMedia
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FileName = fileName,
            ContentType = file.ContentType,
            Size = file.Length,
            StorageKey = uploadResult.Key,
            Url = uploadResult.Url,
            CreatedAt = DateTime.UtcNow
        };

        db.TenantMedias.Add(media);

        profile.ProfileMediaId = media.Id;
        profile.ProfilePhotoUrl = media.Url;
        profile.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        if (oldMedia is not null)
        {
            await storage.DeleteAsync(oldMedia.StorageKey);
            db.TenantMedias.Remove(oldMedia);
            await db.SaveChangesAsync();
        }

        var response = await BuildResponseAsync(userId, tenantId, db);
        return Results.Ok(response!);
    }

    /// <summary>
    /// Remove a foto de profile atual do usuario autenticado.
    /// </summary>
    private static async Task<IResult> DeletePhotoAsync(AppDbContext db,
                                                        IObjectStorageService storage,
                                                        HttpContext context)
    {
        var tenantId = context.GetTenantId();
        var userId = GetUserId(context.User);

        var profile = await db.Set<UserProfile>().FirstOrDefaultAsync(x => x.UserId == userId && x.TenantId == tenantId);
        if (profile is null || !profile.ProfileMediaId.HasValue)
            return Results.NoContent();

        var oldMedia = await db.TenantMedias
            .FirstOrDefaultAsync(x => x.Id == profile.ProfileMediaId.Value && x.TenantId == tenantId);

        profile.ProfileMediaId = null;
        profile.ProfilePhotoUrl = null;
        profile.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        if (oldMedia is not null)
        {
            await storage.DeleteAsync(oldMedia.StorageKey);
            db.TenantMedias.Remove(oldMedia);
            await db.SaveChangesAsync();
        }

        return Results.NoContent();
    }

    /// <summary>
    /// Recarrega a projeção completa do profile usada pelo app.
    /// </summary>
    private static async Task<ProfileResponse?> BuildResponseAsync(Guid userId, Guid tenantId, AppDbContext db)
    {
        var connection = db.Database.GetDbConnection();

        const string sql = @"SELECT u.id AS UserId,
                                    u.name,
                                    u.email,
                                    up.username,
                                    up.phone_number AS PhoneNumber,
                                    CASE
                                        WHEN up.birth_date IS NULL THEN NULL
                                        ELSE TO_CHAR(up.birth_date, 'YYYY-MM-DD')
                                    END AS BirthDate,
                                    u.role,
                                    u.is_active AS IsActive,
                                    up.profile_photo_url AS ProfilePhotoUrl,
                                    active_plan.membership_plan_id AS ActivePlanId,
                                    active_plan.name AS ActivePlan,
                                    active_plan.price AS ActivePlanPrice,
                                    active_plan.duration_days AS ActivePlanDurationDays,
                                    u.created_at AS CreatedAt
                             FROM users u
                             LEFT JOIN user_profiles up
                               ON up.user_id = u.id
                              AND up.tenant_id = u.tenant_id
                             LEFT JOIN LATERAL (
                                 SELECT um.membership_plan_id,
                                        mp.name,
                                        mp.price,
                                        mp.duration_days
                                 FROM user_memberships um
                                 JOIN membership_plans mp
                                   ON mp.id = um.membership_plan_id
                                  AND mp.tenant_id = um.tenant_id
                                 WHERE um.user_id = u.id
                                   AND um.tenant_id = u.tenant_id
                                   AND um.is_active = true
                                   AND mp.is_active = true
                                   AND um.starts_at <= @Now
                                   AND (um.ends_at IS NULL OR um.ends_at > @Now)
                                 ORDER BY um.starts_at DESC, um.created_at DESC
                                 LIMIT 1
                             ) active_plan ON true
                             WHERE u.id = @UserId
                               AND u.tenant_id = @TenantId
                               AND u.is_active = true";

        return await connection.QueryFirstOrDefaultAsync<ProfileResponse>(sql, new
        {
            UserId = userId,
            TenantId = tenantId,
            Now = DateTime.UtcNow
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

    /// <summary>
    /// Normaliza o username para comparacoes e persistencia.
    /// Username vazio e tratado como ausente.
    /// </summary>
    private static string? NormalizeUsername(string? username)
    {
        return string.IsNullOrWhiteSpace(username)
            ? null
            : username.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Valida nome, username, celular e data de nascimento do profile.
    /// </summary>
    private static IResult? ValidateProfileUpdateRequest(string name, string? username, string? phoneNumber, DateTime? birthDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Results.BadRequest("Name is required.");

        if (name.Trim().Length < 3)
            return Results.BadRequest("Name must have at least 3 characters.");

        if (name.Trim().Length > 150)
            return Results.BadRequest("Name must have at most 150 characters.");

        if (!string.IsNullOrWhiteSpace(username))
        {
            if (username.Length < 3 || username.Length > 30)
                return Results.BadRequest("Username must have between 3 and 30 characters.");

            if (!Regex.IsMatch(username, "^[a-z0-9._]+$"))
                return Results.BadRequest("Username may contain only lowercase letters, numbers, dot and underscore.");
        }

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var digits = GetPhoneDigits(phoneNumber);
            if (digits.Length < 10 || digits.Length > 11)
                return Results.BadRequest("Phone number must include area code and a valid Brazilian mobile or landline number.");
        }

        if (birthDate.HasValue)
        {
            if (birthDate.Value.Date > DateTime.UtcNow.Date)
                return Results.BadRequest("Birth date cannot be in the future.");

            if (birthDate.Value.Date < new DateTime(1900, 1, 1))
                return Results.BadRequest("Birth date is invalid.");
        }

        return null;
    }

    /// <summary>
    /// Normaliza o celular do usuario para um formato legivel padrao com DDD.
    /// </summary>
    private static string? NormalizePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        var digits = GetPhoneDigits(phoneNumber);
        if (digits.Length == 11)
            return $"({digits[..2]}) {digits.Substring(2, 5)}-{digits.Substring(7, 4)}";

        if (digits.Length == 10)
            return $"({digits[..2]}) {digits.Substring(2, 4)}-{digits.Substring(6, 4)}";

        return phoneNumber.Trim();
    }

    /// <summary>
    /// Converte a data textual do frontend para uma data persistivel.
    /// </summary>
    private static DateTime? NormalizeBirthDate(string? birthDate)
    {
        if (string.IsNullOrWhiteSpace(birthDate))
            return null;

        if (!DateTime.TryParse(birthDate.Trim(), out var parsed))
            return DateTime.MinValue;

        return DateTime.SpecifyKind(parsed.Date, DateTimeKind.Utc);
    }

    /// <summary>
    /// Extrai apenas os digitos do telefone para validacao e formatacao.
    /// </summary>
    private static string GetPhoneDigits(string phoneNumber)
    {
        return Regex.Replace(phoneNumber, "[^0-9]", string.Empty);
    }

    /// <summary>
    /// Valida o payload de troca de e-mail antes das verificacoes de identidade e unicidade.
    /// </summary>
    private static IResult? ValidateEmailChangeRequest(string currentEmail, string newEmail, string currentPassword)
    {
        if (string.IsNullOrWhiteSpace(currentEmail))
            return Results.BadRequest("Current email is required.");

        if (string.IsNullOrWhiteSpace(newEmail))
            return Results.BadRequest("New email is required.");

        if (string.IsNullOrWhiteSpace(currentPassword))
            return Results.BadRequest("Current password is required.");

        if (!IsValidEmail(currentEmail))
            return Results.BadRequest("Current email format is invalid.");

        if (!IsValidEmail(newEmail))
            return Results.BadRequest("New email format is invalid.");

        return null;
    }

    /// <summary>
    /// Valida o formato do e-mail usando o parser nativo da plataforma.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var parsed = new MailAddress(email.Trim());
            return parsed.Address.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
