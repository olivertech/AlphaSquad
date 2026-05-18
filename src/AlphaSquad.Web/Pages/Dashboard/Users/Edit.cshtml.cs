using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Models;
using AlphaSquad.Web.Security;
using AlphaSquad.Web.Users;
using System.Globalization;

namespace AlphaSquad.Web.Pages.Dashboard.Users;

/// <summary>
/// Tela de edição administrativa de usuários.
/// Aqui a gestão pode ajustar nome, perfil, status e vínculo de plano sem precisar voltar ao backend diretamente.
/// </summary>
public sealed class EditModel(IUsersService usersService, IPlansService plansService) : AdminDashboardPageModelBase
{
    private const int StudentRoleValue = 3;

    [BindProperty]
    public UserFormInputModel Input { get; set; } = new();

    public IReadOnlyList<UserRoleOptionViewModel> RoleOptions => UserPresentationMapper.RoleOptions;
    public IReadOnlyList<MembershipPlanOptionViewModel> MembershipPlanOptions { get; private set; } = [];
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

        if (!IsAdmin)
            ModelState.Remove($"{nameof(Input)}.{nameof(Input.Role)}");

        ValidateMembershipInput();

        if (!ModelState.IsValid)
        {
            await LoadPlanOptionsAsync(Input.MembershipPlanId, cancellationToken);
            return Page();
        }

        PrepareMembershipInputForRole();

        try
        {
            var updateRequest = new UpdateUserRequestDto
            {
                Name = Input.Name.Trim(),
                IsActive = Input.IsActive,
                PhoneNumber = string.IsNullOrWhiteSpace(Input.PhoneNumber) ? null : Input.PhoneNumber.Trim(),
                BirthDate = string.IsNullOrWhiteSpace(Input.BirthDate) ? null : Input.BirthDate.Trim(),
                MembershipPlanId = Input.MembershipPlanId,
                MembershipBillingDueDay = Input.MembershipBillingDueDay
            };

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

        await LoadPlanOptionsAsync(Input.MembershipPlanId, cancellationToken);
        return Page();
    }

    private void ResolvePermissions()
    {
        var session = HttpContext.Session.GetDashboardSession();
        IsAdmin = string.Equals(session?.Role, DashboardRoles.Admin, StringComparison.OrdinalIgnoreCase);
    }

    private void ValidateMembershipInput()
    {
        var effectiveRole = IsAdmin ? Input.Role : StudentRoleValue;
        if (effectiveRole != StudentRoleValue)
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
        var effectiveRole = IsAdmin ? Input.Role : StudentRoleValue;
        if (effectiveRole == StudentRoleValue)
            return;

        Input.MembershipPlanId = null;
        Input.MembershipBillingDueDay = null;
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
                PhoneNumber = response.PhoneNumber ?? string.Empty,
                BirthDate = FormatBirthDateForDisplay(response.BirthDate),
                MembershipPlanId = response.ActiveMembershipPlanId,
                MembershipBillingDueDay = response.MembershipBillingDueDay,
                Role = response.Role,
                IsActive = response.IsActive == true
            };

            await LoadPlanOptionsAsync(Input.MembershipPlanId, cancellationToken);
        }
        catch
        {
            LoadErrorMessage = "Não foi possível carregar os dados desse usuário agora.";
            ShowErrorToast(LoadErrorMessage);
        }

        return Page();
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

    private static string FormatBirthDateForDisplay(string? birthDate)
    {
        if (string.IsNullOrWhiteSpace(birthDate))
            return string.Empty;

        var acceptedFormats = new[] { "yyyy-MM-dd", "dd/MM/yyyy" };
        if (!DateTime.TryParseExact(birthDate, acceptedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return birthDate;

        return parsed.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    }
}
