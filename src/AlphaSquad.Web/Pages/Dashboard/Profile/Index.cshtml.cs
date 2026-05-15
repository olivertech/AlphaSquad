using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

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
                BirthDate = FormatBirthDateForDisplay(response.BirthDate),
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

    /// <summary>
    /// Padroniza a data de nascimento para exibicao visual no formato brasileiro.
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
}
