using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AlphaSquad.Backoffice.Models;
using AlphaSquad.Backoffice.Security;
using AlphaSquad.Shared.DTOs.PlatformAuth;
using AlphaSquad.Shared.DTOs.PlatformProfile;
using AlphaSquad.Shared.DTOs.PlatformTenants;

namespace AlphaSquad.Backoffice.Services;

/// <summary>
/// Cliente HTTP do backoffice para consumir as APIs master da AlphaSquad.
/// Ele centraliza autenticacao, gestao de academias e perfil do owner sem depender do dashboard das academias.
/// </summary>
public sealed class BackofficePlatformApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    : IBackofficeAuthService, IBackofficeTenantWorkspaceService, IBackofficeOwnerProfileService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<PlatformLoginResponse> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/platform-auth/login", new PlatformLoginRequest(email, password), cancellationToken);
        return await ReadAsync<PlatformLoginResponse>(response, cancellationToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, "/api/platform-auth/logout");
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
            await ThrowAsync(response, cancellationToken);
    }

    public async Task<PlatformAuthenticatedSessionResponse> GetSessionAsync(CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, "/api/platform-auth/me");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        return await ReadAsync<PlatformAuthenticatedSessionResponse>(response, cancellationToken);
    }

    public async Task<PlatformProfileResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, "/api/platform-profile/me");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        return await ReadAsync<PlatformProfileResponse>(response, cancellationToken);
    }

    public async Task<PlatformProfileResponse> UpdateAsync(string name, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Put, "/api/platform-profile/me", JsonContent.Create(new UpdatePlatformProfileRequest(name)));
        using var response = await httpClient.SendAsync(request, cancellationToken);
        return await ReadAsync<PlatformProfileResponse>(response, cancellationToken);
    }

    public async Task<string> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, "/api/platform-auth/change-password", JsonContent.Create(new PlatformChangePasswordRequest(currentPassword, newPassword)));
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            await ThrowAsync(response, cancellationToken);

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<PlatformProfileResponse> UploadPhotoAsync(IFormFile photo, CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        using var stream = photo.OpenReadStream();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(photo.ContentType);
        form.Add(fileContent, "file", photo.FileName);

        using var request = CreateRequest(HttpMethod.Put, "/api/platform-profile/me/photo", form);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        return await ReadAsync<PlatformProfileResponse>(response, cancellationToken);
    }

    public async Task<PlatformProfileResponse> DeletePhotoAsync(CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, "/api/platform-profile/me/photo");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        return await ReadAsync<PlatformProfileResponse>(response, cancellationToken);
    }

    public async Task<IReadOnlyList<BackofficeTenantWorkspaceItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, "/api/platform-tenants");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var payload = await ReadAsync<List<PlatformTenantListItemResponse>>(response, cancellationToken);

        return payload
            .Select(MapListItem)
            .ToList();
    }

    public async Task<BackofficeTenantWorkspaceItem?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, $"/api/platform-tenants/{id}");
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        var payload = await ReadAsync<PlatformTenantDetailsResponse>(response, cancellationToken);
        return MapDetailsItem(payload);
    }

    public async Task<BackofficeTenantProvisioningResult> CreateAsync(BackofficeTenantCreateCommand command, CancellationToken cancellationToken = default)
    {
        var requestBody = new CreatePlatformTenantRequest(
            command.Name,
            command.Slug,
            command.LogoUrl,
            command.PrimaryColor,
            command.SecondaryColor,
            command.IsActive,
            command.PrimaryAdminName,
            command.PrimaryAdminEmail,
            [.. command.FeatureCodes]);

        using var request = CreateRequest(HttpMethod.Post, "/api/platform-tenants", JsonContent.Create(requestBody));
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var payload = await ReadAsync<PlatformTenantProvisioningResponse>(response, cancellationToken);

        return new BackofficeTenantProvisioningResult
        {
            Tenant = MapDetailsItem(payload.Tenant),
            TemporaryPassword = payload.TemporaryPassword,
            MustChangePassword = payload.MustChangePassword
        };
    }

    public async Task<BackofficeTenantWorkspaceItem?> UpdateAsync(Guid id, BackofficeTenantUpdateCommand command, CancellationToken cancellationToken = default)
    {
        var requestBody = new UpdatePlatformTenantRequest(
            command.Name,
            command.Slug,
            command.LogoUrl,
            command.PrimaryColor,
            command.SecondaryColor,
            command.IsActive,
            command.AdminName,
            command.AdminEmail,
            [.. command.FeatureCodes]);

        using var request = CreateRequest(HttpMethod.Put, $"/api/platform-tenants/{id}", JsonContent.Create(requestBody));
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        var payload = await ReadAsync<PlatformTenantDetailsResponse>(response, cancellationToken);
        return MapDetailsItem(payload);
    }

    public async Task<BackofficeTenantWorkspaceItem> UploadLogoAsync(Guid id, IFormFile file, CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        form.Add(fileContent, "file", file.FileName);

        using var request = CreateRequest(HttpMethod.Put, $"/api/platform-tenants/{id}/logo", form);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var payload = await ReadAsync<PlatformTenantDetailsResponse>(response, cancellationToken);
        return MapDetailsItem(payload);
    }

    public async Task<IReadOnlyList<BackofficeFeatureOptionViewModel>> GetFeatureCatalogAsync(CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, "/api/platform-tenants/features/catalog");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var payload = await ReadAsync<List<PlatformFeatureCatalogItemResponse>>(response, cancellationToken);

        return payload
            .Select(feature =>
            {
                var knownOption = BackofficeTenantCatalog.FeatureOptions
                    .FirstOrDefault(option => string.Equals(option.Code, feature.Code, StringComparison.OrdinalIgnoreCase));

                return knownOption ?? new BackofficeFeatureOptionViewModel
                {
                    Code = feature.Code,
                    Label = feature.Code,
                    Description = feature.Description
                };
            })
            .OrderBy(option => option.Label)
            .ToList();
    }

    public async Task<IReadOnlyList<BackofficeTenantAdminItem>> GetAdminsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, $"/api/platform-tenants/{tenantId}/admins");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var payload = await ReadAsync<List<PlatformTenantAdminResponse>>(response, cancellationToken);
        return payload.Select(MapAdminItem).ToList();
    }

    public async Task<BackofficeTenantAdminProvisioningResult> CreateAdminAsync(Guid tenantId, BackofficeTenantAdminCreateCommand command, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, $"/api/platform-tenants/{tenantId}/admins", JsonContent.Create(new CreatePlatformTenantAdminRequest(command.Name, command.Email)));
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var payload = await ReadAsync<PlatformTenantAdminProvisioningResponse>(response, cancellationToken);
        return MapAdminProvisioningResult(payload);
    }

    public async Task<BackofficeTenantAdminProvisioningResult> ResetAdminPasswordAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, $"/api/platform-tenants/{tenantId}/admins/{userId}/reset-password");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var payload = await ReadAsync<PlatformTenantAdminPasswordResetResponse>(response, cancellationToken);
        return new BackofficeTenantAdminProvisioningResult
        {
            TenantId = payload.TenantId,
            Admin = new BackofficeTenantAdminItem
            {
                UserId = payload.UserId,
                Email = payload.Email,
                MustChangePassword = payload.MustChangePassword
            },
            TemporaryPassword = payload.TemporaryPassword,
            MustChangePassword = payload.MustChangePassword
        };
    }

    public async Task<IReadOnlyList<BackofficeTenantAuditLogItem>> GetAuditAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, $"/api/platform-tenants/{tenantId}/audit");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var payload = await ReadAsync<List<PlatformTenantAuditLogResponse>>(response, cancellationToken);
        return payload.Select(MapAuditItem).ToList();
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string relativeUrl, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, relativeUrl);
        if (content is not null)
            request.Content = content;

        var accessToken = httpContextAccessor.HttpContext?.Session.GetBackofficeSession()?.AccessToken;
        if (!string.IsNullOrWhiteSpace(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return request;
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            await ThrowAsync(response, cancellationToken);

        var payload = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        if (payload is null)
            throw new BackofficeApiException("A API retornou uma resposta vazia.", (int)response.StatusCode);

        return payload;
    }

    private static async Task ThrowAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = ExtractMessage(raw);

        if (string.IsNullOrWhiteSpace(message))
            message = $"A API retornou o status {(int)response.StatusCode}.";

        throw new BackofficeApiException(message, (int)response.StatusCode);
    }

    private static string? ExtractMessage(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var trimmed = raw.Trim();
        if (!trimmed.StartsWith("{"))
            return trimmed.Trim('"');

        try
        {
            using var document = JsonDocument.Parse(trimmed);
            if (document.RootElement.TryGetProperty("message", out var messageElement))
                return messageElement.GetString();

            if (document.RootElement.TryGetProperty("title", out var titleElement))
                return titleElement.GetString();
        }
        catch
        {
            // Ignora parse failure e devolve o bruto.
        }

        return trimmed;
    }

    private static BackofficeTenantWorkspaceItem MapListItem(PlatformTenantListItemResponse item)
    {
        return new BackofficeTenantWorkspaceItem
        {
            Id = item.Id,
            Name = item.Name,
            Slug = item.Slug,
            LogoUrl = item.LogoUrl,
            PrimaryColor = item.PrimaryColor,
            SecondaryColor = item.SecondaryColor,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            FeatureCount = item.FeatureCount,
            FeatureCodes = [],
            Features = [],
            PrimaryAdminName = item.PrimaryAdminName,
            PrimaryAdminEmail = item.PrimaryAdminEmail,
            MustChangePassword = item.PrimaryAdminMustChangePassword,
            AdminUsers = [],
            AuditLogs = []
        };
    }

    private static BackofficeTenantWorkspaceItem MapDetailsItem(PlatformTenantDetailsResponse item)
    {
        return new BackofficeTenantWorkspaceItem
        {
            Id = item.Id,
            Name = item.Name,
            Slug = item.Slug,
            LogoUrl = item.LogoUrl,
            PrimaryColor = item.PrimaryColor,
            SecondaryColor = item.SecondaryColor,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            FeatureCount = item.Features.Count,
            FeatureCodes = item.Features.Select(feature => feature.Code).ToList(),
            Features = item.Features.Select(feature => new BackofficeFeatureOptionViewModel
            {
                Code = feature.Code,
                Label = BackofficeTenantCatalog.FeatureOptions
                    .FirstOrDefault(option => string.Equals(option.Code, feature.Code, StringComparison.OrdinalIgnoreCase))
                    ?.Label ?? feature.Code,
                Description = feature.Description
            }).ToList(),
            PrimaryAdminName = item.PrimaryAdmin?.Name ?? string.Empty,
            PrimaryAdminEmail = item.PrimaryAdmin?.Email ?? string.Empty,
            MustChangePassword = item.PrimaryAdmin?.MustChangePassword ?? false,
            AdminUsers = item.AdminUsers.Select(MapAdminItem).ToList(),
            AuditLogs = item.AuditLogs.Select(MapAuditItem).ToList()
        };
    }

    private static BackofficeTenantAdminItem MapAdminItem(PlatformTenantAdminResponse item)
    {
        return new BackofficeTenantAdminItem
        {
            UserId = item.UserId,
            Name = item.Name,
            Email = item.Email,
            IsActive = item.IsActive,
            MustChangePassword = item.MustChangePassword,
            CreatedAt = item.CreatedAt
        };
    }

    private static BackofficeTenantAdminProvisioningResult MapAdminProvisioningResult(PlatformTenantAdminProvisioningResponse payload)
    {
        return new BackofficeTenantAdminProvisioningResult
        {
            TenantId = payload.TenantId,
            Admin = MapAdminItem(payload.Admin),
            TemporaryPassword = payload.TemporaryPassword,
            MustChangePassword = payload.MustChangePassword
        };
    }

    private static BackofficeTenantAuditLogItem MapAuditItem(PlatformTenantAuditLogResponse item)
    {
        return new BackofficeTenantAuditLogItem
        {
            Id = item.Id,
            PlatformUserId = item.PlatformUserId,
            PlatformUserName = item.PlatformUserName,
            TenantId = item.TenantId,
            Action = item.Action,
            EntityType = item.EntityType,
            EntityId = item.EntityId,
            Summary = item.Summary,
            MetadataJson = item.MetadataJson,
            CreatedAt = item.CreatedAt
        };
    }
}
