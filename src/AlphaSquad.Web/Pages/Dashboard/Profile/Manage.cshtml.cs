using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

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
public sealed class ManageModel(IProfileService profileService) : AdminDashboardPageModelBase {
    [BindProperty]
    public ProfileManageViewModel Input { get; set; } = new();
    
    public string? LoadErrorMessage { get; private set; }

    // Método auxiliar para garantir que os dados do perfil sejam sempre recarregados
    // evitando que o formulário fique em branco em caso de erro ou postback.
    private async Task LoadProfileDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await profileService.GETApiProfileMeAsync(cancellationToken);
            if (response != null)
            {
                Input.Name = response.Name ?? string.Empty;
                Input.Email = response.Email ?? string.Empty;
                Input.PhotoUrl = response.ProfilePhotoUrl;
            }
        }
        catch
        {
            // Erros de carregamento são tratados no OnGet, aqui apenas garantimos a tentativa de preencher.
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

        if (!ModelState.IsValid) 
        {
            await LoadProfileDataAsync(cancellationToken);
            return Page();
        }

        try
        {
            await profileService.PUTApiProfileMeAsync(new UpdateProfileRequestDto { Name = Input.Name.Trim() }, cancellationToken);
            await profileService.PUTApiProfileMeEmailAsync(new UpdateProfileEmailRequestDto { Email = Input.Email.Trim() }, cancellationToken);

            ShowSuccessToast("Perfil atualizado com sucesso!");
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

        if (string.IsNullOrWhiteSpace(Input.NewPassword) || Input.NewPassword != Input.ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "As senhas novas não coincidem ou estão vazias.");
            await LoadProfileDataAsync(cancellationToken);
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
                ShowSuccessToast("Senha alterada com sucesso!");
                return RedirectToPage();
            }
            
            await LoadProfileDataAsync(cancellationToken);
            ShowErrorToast("Falha ao alterar senha. Verifique as credenciais.");
            return Page();
        }
        catch
        {
            await LoadProfileDataAsync(cancellationToken);
            ShowErrorToast("Ocorreu um erro ao processar a alteração de senha.");
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
                ShowSuccessToast("Foto atualizada com sucesso!");
                // Redireciona para forçar o recarregamento completo dos dados via OnGet
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
            ShowSuccessToast("Foto removida com sucesso!");
        }
        catch
        {
            ShowErrorToast("Erro ao remover a foto de perfil.");
        }

        return RedirectToPage();
    }
}
