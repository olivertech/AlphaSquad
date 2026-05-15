using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlphaSquad.Web.Pages.Dashboard.Profile;

/// <summary>
/// ViewModel para representação visual do perfil do usuário no Dashboard.
/// </summary>
public class ProfileViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? BirthDate { get; set; }
    public string? PhotoUrl { get; set; }
    public string Initials => string.IsNullOrWhiteSpace(Name) ? "??" : new string(Name.ToUpper().Take(2).ToArray());
}

/// <summary>
/// Tela principal do Perfil do Usuário.
/// Permite ao usuário visualizar seus dados atuais e navegar para as telas de edição e segurança.
/// </summary>
public sealed class IndexModel(IProfileService profileService) : AdminDashboardPageModelBase
{
    public ProfileViewModel? Profile { get; private set; }
    public string? LoadErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        try
        {
            var response = await profileService.GETApiProfileMeAsync(cancellationToken);
            
            if (response == null)
            {
                LoadErrorMessage = "Não foi possível localizar as informações do seu perfil.";
                ShowErrorToast(LoadErrorMessage);
                return Page();
            }

            Profile = new ProfileViewModel
            {
                Name = response.Name ?? "Usuário",
                Email = response.Email ?? string.Empty,
                PhoneNumber = response.PhoneNumber,
                BirthDate = response.BirthDate,
                PhotoUrl = response.ProfilePhotoUrl
            };
        }
        catch (Exception)
        {
            LoadErrorMessage = "Ocorreu um erro ao carregar os dados do seu perfil. Tente novamente em instantes.";
            ShowErrorToast(LoadErrorMessage);
        }

        return result;
    }
}
