using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Models;
using AlphaSquad.Web.Users;

namespace AlphaSquad.Web.Pages.Dashboard.Users;

/// <summary>
/// Tela de cadastro de novos usuários da academia.
/// Ela traduz as regras da API para uma experiência mais amigável para a gestão.
/// </summary>
public sealed class CreateModel(IUsersService usersService, IPlansService plansService) : AdminDashboardPageModelBase
{
    private const int StudentRoleValue = 3;

    [BindProperty]
    public UserFormInputModel Input { get; set; } = new();

    public IReadOnlyList<UserRoleOptionViewModel> RoleOptions => UserPresentationMapper.RoleOptions;
    public IReadOnlyList<MembershipPlanOptionViewModel> MembershipPlanOptions { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        Input.IsActive = true;
        await LoadPlanOptionsAsync(null, cancellationToken);
        return result;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        ValidateMembershipInput();

        if (string.IsNullOrWhiteSpace(Input.Password))
            ModelState.AddModelError("Input.Password", "Informe a senha inicial do usuário.");

        if (!ModelState.IsValid)
        {
            await LoadPlanOptionsAsync(Input.MembershipPlanId, cancellationToken);
            return Page();
        }

        PrepareMembershipInputForRole();

        try
        {
            var response = await usersService.POSTApiUsersAsync(new CreateUserRequestDto
            {
                Name = Input.Name.Trim(),
                Email = Input.Email.Trim(),
                Password = Input.Password,
                Role = Input.Role,
                PhoneNumber = string.IsNullOrWhiteSpace(Input.PhoneNumber) ? null : Input.PhoneNumber.Trim(),
                BirthDate = string.IsNullOrWhiteSpace(Input.BirthDate) ? null : Input.BirthDate.Trim(),
                MembershipPlanId = Input.MembershipPlanId,
                MembershipBillingDueDay = Input.MembershipBillingDueDay
            }, cancellationToken);

            if (response?.Id is Guid userId)
                return RedirectToPage("/Dashboard/Users/Details", new { id = userId, created = true });

            ShowErrorToast("Não foi possível concluir o cadastro agora. Tente novamente em instantes.");
        }
        catch
        {
            ShowErrorToast("Não foi possível concluir o cadastro agora. Revise os dados e tente novamente.");
        }

        await LoadPlanOptionsAsync(Input.MembershipPlanId, cancellationToken);
        return Page();
    }

    private void ValidateMembershipInput()
    {
        if (Input.Role != StudentRoleValue)
            return;

        if (!Input.MembershipPlanId.HasValue && !Input.MembershipBillingDueDay.HasValue)
            return;

        if (!Input.MembershipPlanId.HasValue)
            ModelState.AddModelError("Input.MembershipPlanId", "Selecione um plano antes de informar o vencimento.");

        if (!Input.MembershipBillingDueDay.HasValue)
            ModelState.AddModelError("Input.MembershipBillingDueDay", "Informe o dia de vencimento do plano.");
        else if (Input.MembershipBillingDueDay.Value is < 1 or > 31)
            ModelState.AddModelError("Input.MembershipBillingDueDay", "Informe um dia de vencimento entre 1 e 31.");
    }

    private void PrepareMembershipInputForRole()
    {
        if (Input.Role == StudentRoleValue)
            return;

        Input.MembershipPlanId = null;
        Input.MembershipBillingDueDay = null;
    }

    private async Task LoadPlanOptionsAsync(Guid? selectedPlanId, CancellationToken cancellationToken)
    {
        try
        {
            var plans = await plansService.GETApiPlansAsync(cancellationToken);
            MembershipPlanOptions = plans?
                .Where(plan => plan.Id.HasValue && (plan.IsActive == true || plan.Id == selectedPlanId))
                .OrderBy(plan => plan.Name)
                .Select(UserPresentationMapper.ToMembershipPlanOption)
                .ToList() ?? [];
        }
        catch
        {
            MembershipPlanOptions = [];
            ShowWarningToast("Os planos da academia não puderam ser carregados agora.");
        }
    }
}
