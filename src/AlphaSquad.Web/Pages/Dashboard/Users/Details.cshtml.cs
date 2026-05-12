using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Models;
using AlphaSquad.Web.Users;

namespace AlphaSquad.Web.Pages.Dashboard.Users;

/// <summary>
/// Exibe a visão detalhada de um usuário da academia.
/// Ela ajuda a gestão a validar rapidamente o perfil, o status e os próximos passos administrativos.
/// </summary>
public sealed class DetailsModel(IUsersService usersService) : AdminDashboardPageModelBase
{
    public UserDetailsViewModel? UserDetails { get; private set; }
    public string? LoadErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, bool? created, bool? updated, bool? deactivated, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (created == true)
            ShowSuccessToast("Usuário cadastrado com sucesso.");
        else if (updated == true)
            ShowSuccessToast("Usuário atualizado com sucesso.");
        else if (deactivated == true)
            ShowSuccessToast("Usuário desativado com sucesso.");

        try
        {
            var response = await usersService.GETApiUsersByIdAsync(id, cancellationToken);
            if (response?.Id is not Guid)
                return RedirectToPage("/Dashboard/Users/Index");

            UserDetails = UserPresentationMapper.ToDetails(response);
        }
        catch
        {
            LoadErrorMessage = "Não foi possível carregar os detalhes desse usuário agora.";
            ShowErrorToast(LoadErrorMessage);
        }

        return result;
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        try
        {
            await usersService.DELETEApiUsersByIdAsync(id, cancellationToken);
            return RedirectToPage(new { id, deactivated = true });
        }
        catch
        {
            LoadErrorMessage = "Não foi possível desativar esse usuário agora.";
            ShowErrorToast(LoadErrorMessage);
            return await OnGetAsync(id, false, false, false, cancellationToken);
        }
    }
}
