using System.ComponentModel.DataAnnotations;
using AlphaSquad.Backoffice.Security;
using AlphaSquad.Backoffice.Services;
using AlphaSquad.Shared.DTOs.PlatformProfile;

namespace AlphaSquad.Backoffice.Pages.Backoffice.Profile;

public sealed class ManageModel(IBackofficeOwnerProfileService profileService) : BackofficePageModelBase
{
    [BindProperty]
    public BackofficeOwnerProfileManageInputModel Input { get; set; } = new();

    public string? LoadErrorMessage { get; private set; }
    public string Initials { get; private set; } = "A";

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        try
        {
            await LoadProfileDataAsync(cancellationToken);
        }
        catch
        {
            LoadErrorMessage = "Nao foi possivel carregar o perfil do owner.";
            ShowErrorToast(LoadErrorMessage);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        ModelState.Remove($"{nameof(Input)}.{nameof(Input.CurrentPassword)}");
        ModelState.Remove($"{nameof(Input)}.{nameof(Input.NewPassword)}");
        ModelState.Remove($"{nameof(Input)}.{nameof(Input.ConfirmPassword)}");

        if (!ModelState.IsValid)
        {
            await LoadProfileDataAsync(cancellationToken, preserveName: true, preserveEmail: true);
            return Page();
        }

        try
        {
            var response = await profileService.UpdateAsync(Input.Name.Trim(), cancellationToken);
            UpdateSession(response);
            ShowSuccessToast("Perfil atualizado com sucesso.", persist: true);
            return RedirectToPage("/Backoffice/Profile/Index");
        }
        catch (BackofficeApiException exception)
        {
            await LoadProfileDataAsync(cancellationToken, preserveName: true, preserveEmail: true);
            ShowErrorToast(exception.Message);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostChangePasswordAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        ModelState.Remove($"{nameof(Input)}.{nameof(Input.Name)}");
        ModelState.Remove($"{nameof(Input)}.{nameof(Input.Email)}");

        if (string.IsNullOrWhiteSpace(Input.CurrentPassword))
        {
            await LoadProfileDataAsync(cancellationToken);
            ShowWarningToast("Informe sua senha atual para confirmar a alteracao.");
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Input.NewPassword))
        {
            await LoadProfileDataAsync(cancellationToken);
            ShowWarningToast("Informe a nova senha.");
            return Page();
        }

        if (Input.NewPassword != Input.ConfirmPassword)
        {
            await LoadProfileDataAsync(cancellationToken);
            ShowWarningToast("A confirmacao da nova senha nao confere.");
            return Page();
        }

        if (!ModelState.IsValid)
        {
            await LoadProfileDataAsync(cancellationToken);
            var validationMessage = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).FirstOrDefault();
            ShowWarningToast(validationMessage ?? "Revise os dados informados para a nova senha.");
            return Page();
        }

        try
        {
            await profileService.ChangePasswordAsync(Input.CurrentPassword, Input.NewPassword, cancellationToken);
            ShowSuccessToast("Senha alterada com sucesso.", persist: true);
            return RedirectToPage();
        }
        catch (BackofficeApiException exception)
        {
            await LoadProfileDataAsync(cancellationToken);
            ShowErrorToast(BuildFriendlyPasswordErrorMessage(exception));
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUploadPhotoAsync(IFormFile photo, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (photo == null || photo.Length == 0)
        {
            await LoadProfileDataAsync(cancellationToken);
            ShowErrorToast("Selecione uma imagem valida.");
            return Page();
        }

        try
        {
            var response = await profileService.UploadPhotoAsync(photo, cancellationToken);
            UpdateSession(response);
            ShowSuccessToast("Foto atualizada com sucesso.", persist: true);
            return RedirectToPage();
        }
        catch (BackofficeApiException exception)
        {
            await LoadProfileDataAsync(cancellationToken);
            ShowErrorToast(exception.Message);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeletePhotoAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        try
        {
            var response = await profileService.DeletePhotoAsync(cancellationToken);
            UpdateSession(response);
            ShowSuccessToast("Foto removida com sucesso.", persist: true);
        }
        catch (BackofficeApiException exception)
        {
            ShowErrorToast(exception.Message);
        }

        return RedirectToPage();
    }

    private async Task LoadProfileDataAsync(CancellationToken cancellationToken, bool preserveName = false, bool preserveEmail = false)
    {
        var response = await profileService.GetAsync(cancellationToken);

        if (!preserveName)
            Input.Name = response.Name;

        if (!preserveEmail)
            Input.Email = response.Email;

        Input.PhotoUrl = response.ProfilePhotoUrl;
        Initials = string.Concat(response.Name
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(part => char.ToUpperInvariant(part[0])));

        if (string.IsNullOrWhiteSpace(Initials))
            Initials = "A";
    }

    private void UpdateSession(PlatformProfileResponse response)
    {
        var session = HttpContext.Session.GetBackofficeSession();
        if (session is null)
            return;

        session.Name = response.Name;
        session.Email = response.Email;
        session.MustChangePassword = response.MustChangePassword;
        session.ProfilePhotoUrl = response.ProfilePhotoUrl;
        HttpContext.Session.SetBackofficeSession(session);
    }

    private static string BuildFriendlyPasswordErrorMessage(BackofficeApiException exception)
    {
        var message = exception.Message;

        if (message.Contains("Current password is incorrect", StringComparison.OrdinalIgnoreCase))
            return "A senha atual informada esta incorreta.";

        if (message.Contains("New password must be different", StringComparison.OrdinalIgnoreCase))
            return "A nova senha precisa ser diferente da senha atual.";

        if (message.Contains("at least 6 characters", StringComparison.OrdinalIgnoreCase))
            return "A nova senha deve ter pelo menos 6 caracteres.";

        if (message.Contains("Current and new passwords are required", StringComparison.OrdinalIgnoreCase))
            return "Informe sua senha atual e a nova senha para concluir a alteracao.";

        if (exception.StatusCode == 401)
            return "Sua sessao nao e mais valida. Entre novamente e tente alterar a senha.";

        return string.IsNullOrWhiteSpace(message)
            ? "Ocorreu um erro ao processar a alteracao de senha."
            : message;
    }

    public sealed class BackofficeOwnerProfileManageInputModel
    {
        [Required(ErrorMessage = "O nome e obrigatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail e obrigatorio.")]
        [EmailAddress(ErrorMessage = "E-mail invalido.")]
        public string Email { get; set; } = string.Empty;

        public string CurrentPassword { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;

        public string? PhotoUrl { get; set; }
    }
}
