using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Models;
using AlphaSquad.Web.Users;

namespace AlphaSquad.Web.Pages.Dashboard.Users;

/// <summary>
/// Tela de edição administrativa de usuários.
/// Aqui a gestão pode ajustar nome, perfil e status sem precisar voltar ao backend diretamente.
/// </summary>
public sealed class EditModel(IUsersService usersService) : AdminDashboardPageModelBase
{
    [BindProperty]
    public UserFormInputModel Input { get; set; } = new();

    public IReadOnlyList<UserRoleOptionViewModel> RoleOptions => UserPresentationMapper.RoleOptions;
    public Guid UserId { get; private set; }
    public string? LoadErrorMessage { get; private set; }
    public string? SubmitErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        UserId = id;
        return await LoadUserAsync(id, cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        UserId = id;

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var response = await usersService.PUTApiUsersByIdAsync(id, new UpdateUserRequestDto
            {
                Name = Input.Name.Trim(),
                Role = Input.Role,
                IsActive = Input.IsActive
            }, cancellationToken);

            if (response?.Id is Guid userId)
                return RedirectToPage("/Dashboard/Users/Details", new { id = userId, updated = true });

            SubmitErrorMessage = "Não foi possível salvar as alterações agora. Tente novamente em instantes.";
        }
        catch
        {
            SubmitErrorMessage = "Não foi possível salvar as alterações agora. Revise os dados e tente novamente.";
        }

        return Page();
    }

    /// <summary>
    /// Carrega o usuário para a tela de edição, convertendo o retorno da API em um formulário amigável.
    /// </summary>
    private async Task<IActionResult> LoadUserAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await usersService.GETApiUsersByIdAsync(id, cancellationToken);
            if (response?.Id is not Guid)
                return RedirectToPage("/Dashboard/Users/Index");

            Input = new UserFormInputModel
            {
                Name = response.Name ?? string.Empty,
                Email = response.Email ?? string.Empty,
                Role = response.Role,
                IsActive = response.IsActive == true
            };
        }
        catch
        {
            LoadErrorMessage = "Não foi possível carregar os dados desse usuário agora.";
        }

        return Page();
    }
}
