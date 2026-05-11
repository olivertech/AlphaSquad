using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Models;
using AlphaSquad.Web.Users;

namespace AlphaSquad.Web.Pages.Dashboard.Users;

/// <summary>
/// Tela de cadastro de novos usuários da academia.
/// Ela traduz as regras da API para uma experiência mais amigável para a gestão.
/// </summary>
public sealed class CreateModel(IUsersService usersService) : AdminDashboardPageModelBase
{
    [BindProperty]
    public UserFormInputModel Input { get; set; } = new();

    public IReadOnlyList<UserRoleOptionViewModel> RoleOptions => UserPresentationMapper.RoleOptions;
    public string? SubmitErrorMessage { get; private set; }

    public IActionResult OnGet()
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        Input.IsActive = true;
        return result;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (string.IsNullOrWhiteSpace(Input.Password))
            ModelState.AddModelError("Input.Password", "Informe a senha inicial do usuário.");

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var response = await usersService.POSTApiUsersAsync(new CreateUserRequestDto
            {
                Name = Input.Name.Trim(),
                Email = Input.Email.Trim(),
                Password = Input.Password,
                Role = Input.Role
            }, cancellationToken);

            if (response?.Id is Guid userId)
                return RedirectToPage("/Dashboard/Users/Details", new { id = userId, created = true });

            SubmitErrorMessage = "Não foi possível concluir o cadastro agora. Tente novamente em instantes.";
        }
        catch
        {
            SubmitErrorMessage = "Não foi possível concluir o cadastro agora. Revise os dados e tente novamente.";
        }

        return Page();
    }
}
