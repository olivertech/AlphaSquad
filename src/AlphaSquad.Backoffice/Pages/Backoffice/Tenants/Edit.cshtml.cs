using AlphaSquad.Backoffice.Models;
using AlphaSquad.Backoffice.Services;

namespace AlphaSquad.Backoffice.Pages.Backoffice.Tenants;

/// <summary>
/// Ajusta dados basicos da academia enquanto a API master ainda nao foi aberta.
/// </summary>
public sealed class EditModel(IBackofficeTenantWorkspaceService tenantWorkspaceService) : BackofficePageModelBase
{
    [BindProperty]
    public BackofficeTenantFormInputModel Input { get; set; } = new();

    [BindProperty]
    public IFormFile? LogoFile { get; set; }

    public Guid TenantId { get; private set; }
    public IReadOnlyList<BackofficeFeatureOptionViewModel> FeatureOptions => BackofficeTenantCatalog.FeatureOptions;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        var tenant = await tenantWorkspaceService.GetAsync(id, cancellationToken);
        if (tenant is null)
        {
            ShowWarningToast("A academia solicitada nao foi encontrada.", persist: true, title: "Cadastro inexistente");
            return RedirectToPage("/Backoffice/Tenants/Index");
        }

        TenantId = tenant.Id;
        Input = new BackofficeTenantFormInputModel
        {
            Name = tenant.Name,
            Slug = tenant.Slug,
            PrimaryColor = tenant.PrimaryColor,
            SecondaryColor = tenant.SecondaryColor,
            IsActive = tenant.IsActive,
            FeatureCodes = [.. tenant.FeatureCodes],
            AdminName = tenant.PrimaryAdminName,
            AdminEmail = tenant.PrimaryAdminEmail
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        TenantId = id;

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var logoUrl = await ConvertLogoToDataUrlAsync(LogoFile, cancellationToken);

            var tenant = await tenantWorkspaceService.UpdateAsync(id, new BackofficeTenantUpdateCommand
            {
                Name = Input.Name,
                Slug = Input.Slug,
                PrimaryColor = NormalizeColor(Input.PrimaryColor),
                SecondaryColor = NormalizeColor(Input.SecondaryColor),
                IsActive = Input.IsActive,
                FeatureCodes = Input.FeatureCodes,
                LogoUrl = logoUrl
            }, cancellationToken);

            if (tenant is null)
            {
                ShowWarningToast("A academia solicitada nao foi encontrada.", persist: true, title: "Cadastro inexistente");
                return RedirectToPage("/Backoffice/Tenants/Index");
            }

            ShowSuccessToast("Academia atualizada com sucesso.", persist: true, title: "Dados salvos");
            return RedirectToPage("/Backoffice/Tenants/Details", new { id = tenant.Id, updated = true });
        }
        catch (InvalidOperationException ex)
        {
            ShowWarningToast(ex.Message, title: "Nao foi possivel salvar");
            return Page();
        }
        catch
        {
            ShowErrorToast("Nao foi possivel atualizar esta academia agora. Tente novamente em instantes.", title: "Falha na atualizacao");
            return Page();
        }
    }

    private static string NormalizeColor(string color)
    {
        var normalized = color.Trim();
        return normalized.StartsWith('#') ? normalized : $"#{normalized}";
    }

    private static async Task<string?> ConvertLogoToDataUrlAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return null;

        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        return $"data:{file.ContentType};base64,{Convert.ToBase64String(bytes)}";
    }
}
