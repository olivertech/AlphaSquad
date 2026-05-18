using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Models;
using AlphaSquad.Web.Users;

namespace AlphaSquad.Web.Pages.Dashboard.Users;

/// <summary>
/// Exibe a trilha de pontuação do aluno para que a gestão entenda de onde veio cada crédito da gamificação.
/// </summary>
public sealed class GamificationModel(IUsersService usersService) : AdminDashboardPageModelBase
{
    public UserGamificationHistoryViewModel? History { get; private set; }
    public string? LoadErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        try
        {
            var response = await usersService.GETApiUsersByIdGamificationHistoryAsync(id, 150, cancellationToken);
            if (response?.UserId is not Guid)
                return RedirectToPage("/Dashboard/Users/Index");

            History = UserPresentationMapper.ToGamificationHistory(response);
        }
        catch
        {
            LoadErrorMessage = "Não foi possível carregar o histórico de pontuação desse usuário agora.";
            ShowErrorToast(LoadErrorMessage);
        }

        return result;
    }
}
