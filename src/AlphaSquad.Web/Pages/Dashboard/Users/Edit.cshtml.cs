using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Models;
using AlphaSquad.Web.Users;
using AlphaSquad.Web.Security;

namespace AlphaSquad.Web.Pages.Dashboard.Users;

/// <summary>
/// Tela de ediçao administrativa de usuários.
/// Aqui a gestão pode ajustar nome, perfil e status sem precisar voltar ao backend diretamente.
/// </summary>
public sealed class EditModel(IUsersService usersService) : AdminDashboardPageModelBase
{
    [BindProperty]
    public UserFormInputModel Input { get; set; } = new();

    public IReadOnlyList<UserRoleOptionViewModel> RoleOptions => UserPresentationMapper.RoleOptions;
    public Guid UserId { get; private set; }
    public string? LoadErrorMessage { get; private set; }
    public bool IsAdmin { get; private set; }
    public string CurrentRoleLabel => UserPresentationMapper.ToRoleLabel(Input.Role);

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        UserId = id;

        ResolvePermissions();

        return await LoadUserAsync(id, cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        UserId = id;

        ResolvePermissions();

        // Se a tela for reutilizada por outro perfil no futuro, a ausência do campo de role no
        // form não pode invalidar o submit nem abrir espaço para alteração indireta via DevTools.
        if (!IsAdmin)
            ModelState.Remove($"{nameof(Input)}.{nameof(Input.Role)}");

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var updateRequest = new UpdateUserRequestDto
            {
                Name = Input.Name.Trim(),
                IsActive = Input.IsActive
            };

            // A Role só é enviada na requisição se o usuário logado for administrador.
            if (IsAdmin)
                updateRequest.Role = Input.Role;

            var response = await usersService.PUTApiUsersByIdAsync(id, updateRequest, cancellationToken);

            if (response?.Id is Guid userId)
                return RedirectToPage("/Dashboard/Users/Details", new { id = userId, updated = true });

            ShowErrorToast("Não foi possível salvar as alterações agora. Tente novamente em instantes.");
        }
        catch
        {
            ShowErrorToast("Não foi possível salvar as alterações agora. Revise os dados e tente novamente.");
        }

        return Page();
    }

    private void ResolvePermissions()
    {
        var session = HttpContext.Session.GetDashboardSession();
        IsAdmin = string.Equals(session?.Role, DashboardRoles.Admin, StringComparison.OrdinalIgnoreCase);
    }

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
            ShowErrorToast(LoadErrorMessage);
        }

        return Page();
    }
}
