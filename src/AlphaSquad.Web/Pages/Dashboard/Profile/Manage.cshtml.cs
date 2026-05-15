using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Security;

namespace AlphaSquad.Web.Pages.Dashboard.Profile;

/// <summary>
/// ViewModel consolidado para a gestão completa do perfil do usuário.
/// </summary>
public class ProfileManageViewModel
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Celular")]
    [RegularExpression(@"^[0-9()\-\s+]{10,20}$", ErrorMessage = "Informe um celular válido com DDD.")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Data de nascimento")]
    [RegularExpression(@"^\d{2}/\d{2}/\d{4}$", ErrorMessage = "Informe a data no formato dd/mm/aaaa.")]
    public string? BirthDate { get; set; }

    public string CurrentPassword { get; set; } = string.Empty;
    
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres.")]
    public string NewPassword { get; set; } = string.Empty;
    
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }
}

/// <summary>
/// Tela unificada de gerenciamento de perfil.
/// Centraliza a edição de dados pessoais, segurança e foto em um único local.
/// </summary>
public sealed class ManageModel(IProfileService profileService) : AdminDashboardPageModelBase
{
    [BindProperty]
    public ProfileManageViewModel Input { get; set; } = new();
    
    public string? LoadErrorMessage { get; private set; }

    private async Task LoadProfileDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await profileService.GETApiProfileMeAsync(cancellationToken);
            if (response != null)
            {
                Input.Name = response.Name ?? string.Empty;
                Input.Email = response.Email ?? string.Empty;
                Input.PhoneNumber = response.PhoneNumber;
                Input.BirthDate = FormatBirthDateForDisplay(response.BirthDate);
                Input.PhotoUrl = response.ProfilePhotoUrl;
            }
        }
        catch
        {
            // Erros de carregamento são tratados no OnGet.
        }
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        try
        {
            await LoadProfileDataAsync(cancellationToken);
            if (string.IsNullOrEmpty(Input.Name))
            {
                LoadErrorMessage = "Não foi possível carregar as informações do seu perfil.";
                ShowErrorToast(LoadErrorMessage);
            }
        }
        catch
        {
            LoadErrorMessage = "Erro ao carregar dados do perfil.";
            ShowErrorToast(LoadErrorMessage);
        }

        return result;
    }

    public async Task<IActionResult> OnPostUpdateAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult) return result;

        // O card de dados pessoais nao deve depender das validacoes do card de senha.
        ModelState.Remove($"{nameof(Input)}.{nameof(Input.CurrentPassword)}");
        ModelState.Remove($"{nameof(Input)}.{nameof(Input.NewPassword)}");
        ModelState.Remove($"{nameof(Input)}.{nameof(Input.ConfirmPassword)}");

        if (!ModelState.IsValid) 
        {
            await LoadProfileDataAsync(cancellationToken);
            return Page();
        }

        try
        {
            await profileService.PUTApiProfileMeAsync(new UpdateProfileRequestDto
            {
                Name = Input.Name.Trim(),
                PhoneNumber = Input.PhoneNumber?.Trim(),
                BirthDate = Input.BirthDate?.Trim()
            }, cancellationToken);

            ShowSuccessToast("Perfil atualizado com sucesso!", persist: true);
            return RedirectToPage("/Dashboard/Profile/Index");
        }
        catch
        {
            await LoadProfileDataAsync(cancellationToken);
            ShowErrorToast("Erro ao atualizar os dados do perfil.");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostChangePasswordAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult) return result;

        // O card de senha nao deve depender das validacoes do card de dados pessoais.
        ModelState.Remove($"{nameof(Input)}.{nameof(Input.Name)}");
        ModelState.Remove($"{nameof(Input)}.{nameof(Input.Email)}");
        ModelState.Remove($"{nameof(Input)}.{nameof(Input.PhoneNumber)}");
        ModelState.Remove($"{nameof(Input)}.{nameof(Input.BirthDate)}");

        if (string.IsNullOrWhiteSpace(Input.CurrentPassword))
        {
            await LoadProfileDataAsync(cancellationToken);
            ShowWarningToast("Informe sua senha atual para confirmar a alteração.");
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
            ShowWarningToast("A confirmação da nova senha não confere.");
            return Page();
        }

        if (!ModelState.IsValid)
        {
            await LoadProfileDataAsync(cancellationToken);
            var validationMessage = ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage)
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            ShowWarningToast(validationMessage ?? "Revise os dados informados para a nova senha.");
            return Page();
        }

        try
        {
            var request = new ChangePasswordRequestDto
            {
                CurrentPassword = Input.CurrentPassword,
                NewPassword = Input.NewPassword
            };

            var response = await profileService.PUTApiProfileMePasswordAsync(request, cancellationToken);
            if (!string.IsNullOrEmpty(response))
            {
                ShowSuccessToast("Senha alterada com sucesso!", persist: true);
                return RedirectToPage();
            }
            
            await LoadProfileDataAsync(cancellationToken);
            ShowErrorToast("Não foi possível alterar a senha. Verifique os dados informados e tente novamente.");
            return Page();
        }
        catch (Exception exception)
        {
            await LoadProfileDataAsync(cancellationToken);
            ShowErrorToast(BuildFriendlyPasswordErrorMessage(exception));
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUploadPhotoAsync(IFormFile photo, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult) return result;

        if (photo == null || photo.Length == 0)
        {
            await LoadProfileDataAsync(cancellationToken);
            ShowErrorToast("Por favor, selecione uma imagem válida.");
            return Page();
        }

        try
        {
            using var stream = new MemoryStream();
            await photo.CopyToAsync(stream);
            
            var request = new MultipartBodyDto
            {
                Content = stream.ToArray(),
                ContentType = photo.ContentType,
                FileName = photo.FileName
            };

            var response = await profileService.PUTApiProfileMePhotoAsync(request, cancellationToken);
            if (response != null)
            {
                var session = HttpContext.Session.GetDashboardSession();
                if (session != null)
                {
                    session.ProfilePhotoUrl = response.ProfilePhotoUrl;
                    HttpContext.Session.SetDashboardSession(session);
                }

                ShowSuccessToast("Foto atualizada com sucesso!", persist: true);
                return RedirectToPage();
            }
            
            await LoadProfileDataAsync(cancellationToken);
            ShowErrorToast("Não foi possível atualizar a foto.");
            return Page();
        }
        catch (Exception ex)
        {
            await LoadProfileDataAsync(cancellationToken);
            ShowErrorToast($"Erro no upload: {ex.Message}");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeletePhotoAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult) return result;

        try
        {
            await profileService.DELETEApiProfileMePhotoAsync(cancellationToken);
            
            var session = HttpContext.Session.GetDashboardSession();
            if (session != null)
            {
                session.ProfilePhotoUrl = null;
                HttpContext.Session.SetDashboardSession(session);
            }

            ShowSuccessToast("Foto removida com sucesso!", persist: true);
        }
        catch
        {
            ShowErrorToast("Erro ao remover a foto de perfil.");
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Converte a data textual retornada pela API para o formato amigavel do dashboard.
    /// </summary>
    private static string? FormatBirthDateForDisplay(string? birthDate)
    {
        if (string.IsNullOrWhiteSpace(birthDate))
            return null;

        var acceptedFormats = new[] { "yyyy-MM-dd", "dd/MM/yyyy" };
        if (!DateTime.TryParseExact(birthDate, acceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return birthDate;

        return parsed.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Traduz os erros tecnicos da API de profile para mensagens claras no dashboard.
    /// </summary>
    private static string BuildFriendlyPasswordErrorMessage(Exception exception)
    {
        var fullMessage = $"{exception.Message} {exception.InnerException?.Message}".Trim();
        if (string.IsNullOrWhiteSpace(fullMessage))
            return "Ocorreu um erro ao processar a alteração de senha.";

        if (fullMessage.Contains("Current password is incorrect", StringComparison.OrdinalIgnoreCase))
            return "A senha atual informada está incorreta.";

        if (fullMessage.Contains("New password must be different", StringComparison.OrdinalIgnoreCase))
            return "A nova senha precisa ser diferente da senha atual.";

        if (fullMessage.Contains("at least 6 characters", StringComparison.OrdinalIgnoreCase))
            return "A nova senha deve ter pelo menos 6 caracteres.";

        if (fullMessage.Contains("Current and new passwords are required", StringComparison.OrdinalIgnoreCase))
            return "Informe sua senha atual e a nova senha para concluir a alteração.";

        if (fullMessage.Contains("400", StringComparison.OrdinalIgnoreCase))
            return "Não foi possível alterar a senha porque algum dado enviado é inválido.";

        if (fullMessage.Contains("401", StringComparison.OrdinalIgnoreCase) || fullMessage.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
            return "Sua sessão não é mais válida. Entre novamente e tente alterar a senha.";

        return "Ocorreu um erro ao processar a alteração de senha. Tente novamente em instantes.";
    }
}
