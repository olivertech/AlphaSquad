using System.ComponentModel.DataAnnotations;
using AlphaSquad.Backoffice.Pages.Backoffice;
using AlphaSquad.Backoffice.Services;

namespace AlphaSquad.Backoffice.Pages.Backoffice.Tenants;

/// <summary>
/// Permite ao sponsor gerenciar administradores extras de uma academia pelo backoffice master.
/// </summary>
public sealed class AdminsModel(IBackofficeTenantWorkspaceService tenantWorkspaceService) : BackofficePageModelBase
{
    [BindProperty]
    public TenantAdminInputModel Input { get; set; } = new();

    public Guid TenantId { get; private set; }
    public string TenantName { get; private set; } = string.Empty;
    public IReadOnlyList<BackofficeTenantAdminItem> AdminUsers { get; private set; } = [];
    public string? LatestTemporaryPassword { get; private set; }
    public string? LatestTemporaryPasswordEmail { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        return await LoadPageAsync(id, cancellationToken);
    }

    public async Task<IActionResult> OnPostCreateAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (!ModelState.IsValid)
            return await LoadPageAsync(id, cancellationToken);

        try
        {
            var provisioning = await tenantWorkspaceService.CreateAdminAsync(id, new BackofficeTenantAdminCreateCommand
            {
                Name = Input.Name,
                Email = Input.Email
            }, cancellationToken);

            TempData["Backoffice.TenantAdminTemporaryPassword"] = provisioning.TemporaryPassword;
            TempData["Backoffice.TenantAdminTemporaryPasswordEmail"] = provisioning.Admin.Email;
            ShowSuccessToast("Administrador adicional criado com sucesso.", persist: true, title: "Novo admin provisionado");
            return RedirectToPage(new { id });
        }
        catch (BackofficeApiException ex)
        {
            ShowWarningToast(ex.Message, title: "Não foi possível criar o admin");
            return await LoadPageAsync(id, cancellationToken);
        }
        catch
        {
            ShowErrorToast("Não foi possível criar este administrador agora. Tente novamente em instantes.", title: "Falha no provisionamento");
            return await LoadPageAsync(id, cancellationToken);
        }
    }

    public async Task<IActionResult> OnPostResetAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        try
        {
            var provisioning = await tenantWorkspaceService.ResetAdminPasswordAsync(id, userId, cancellationToken);
            TempData["Backoffice.TenantAdminTemporaryPassword"] = provisioning.TemporaryPassword;
            TempData["Backoffice.TenantAdminTemporaryPasswordEmail"] = provisioning.Admin.Email;
            ShowSuccessToast("Senha provisória regenerada com sucesso.", persist: true, title: "Credencial atualizada");
            return RedirectToPage(new { id });
        }
        catch (BackofficeApiException ex)
        {
            ShowWarningToast(ex.Message, persist: true, title: "Não foi possível resetar");
            return RedirectToPage(new { id });
        }
        catch
        {
            ShowErrorToast("Não foi possível gerar uma nova senha provisória agora.", persist: true, title: "Falha na operação");
            return RedirectToPage(new { id });
        }
    }

    private async Task<IActionResult> LoadPageAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await tenantWorkspaceService.GetAsync(id, cancellationToken);
        if (tenant is null)
        {
            ShowWarningToast("A academia solicitada não foi encontrada.", persist: true, title: "Cadastro inexistente");
            return RedirectToPage("/Backoffice/Tenants/Index");
        }

        TenantId = tenant.Id;
        TenantName = tenant.Name;
        AdminUsers = tenant.AdminUsers;
        LatestTemporaryPassword = TempData["Backoffice.TenantAdminTemporaryPassword"]?.ToString();
        LatestTemporaryPasswordEmail = TempData["Backoffice.TenantAdminTemporaryPasswordEmail"]?.ToString();
        return Page();
    }

    public sealed class TenantAdminInputModel
    {
        [Required(ErrorMessage = "Informe o nome do administrador.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o e-mail do administrador.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        public string Email { get; set; } = string.Empty;
    }
}
