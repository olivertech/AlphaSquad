using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Models;
using AlphaSquad.Web.Users;

namespace AlphaSquad.Web.Pages.Dashboard.Classes;

/// <summary>
/// Tela de cadastro de novas aulas da academia.
/// Ela traduz o contrato da API para um formulario mais claro para a operacao administrativa.
/// </summary>
public sealed class CreateModel(
    IClassesService classesService,
    IUsersService usersService) : AdminDashboardPageModelBase
{
    [BindProperty]
    public ClassFormInputModel Input { get; set; } = new();

    public IReadOnlyList<InstructorOptionViewModel> InstructorOptions { get; private set; } = [];
    public string? SubmitErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        Input.IsActive = true;
        await LoadInstructorOptionsAsync(cancellationToken);
        return result;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        await LoadInstructorOptionsAsync(cancellationToken);

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var response = await classesService.POSTApiClassesAsync(new CreateGymClassRequestDto
            {
                Name = Input.Name.Trim(),
                Description = NormalizeOptional(Input.Description),
                Location = NormalizeOptional(Input.Location),
                Capacity = Input.Capacity,
                StartsAt = ToLocalOffset(Input.StartsAt),
                EndsAt = ToLocalOffset(Input.EndsAt),
                InstructorUserId = Input.InstructorUserId,
                IsSpecialClass = Input.IsSpecialClass
            }, cancellationToken);

            if (response?.Id is Guid classId)
                return RedirectToPage("/Dashboard/Classes/Details", new { id = classId, created = true });

            SubmitErrorMessage = "Nao foi possivel cadastrar a aula agora. Tente novamente em instantes.";
        }
        catch
        {
            SubmitErrorMessage = "Nao foi possivel cadastrar a aula agora. Revise os dados e tente novamente.";
        }

        return Page();
    }

    /// <summary>
    /// Carrega somente administradores e professores ativos como candidatos a instrutor.
    /// Isso mantem o select coerente com a regra do backend.
    /// </summary>
    private async Task LoadInstructorOptionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var users = await usersService.GETApiUsersAsync(cancellationToken) ?? [];
            InstructorOptions = users
                .Where(user => user.Id.HasValue && user.IsActive == true && (user.Role == 1 || user.Role == 2))
                .OrderBy(user => user.Name ?? string.Empty)
                .Select(user => new InstructorOptionViewModel
                {
                    Value = user.Id!.Value,
                    Name = string.IsNullOrWhiteSpace(user.Name) ? "Usuario sem nome" : user.Name.Trim(),
                    RoleLabel = UserPresentationMapper.ToRoleLabel(user.Role)
                })
                .ToList();
        }
        catch
        {
            InstructorOptions = [];
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTimeOffset? ToLocalOffset(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        var local = DateTime.SpecifyKind(value.Value, DateTimeKind.Local);
        return new DateTimeOffset(local);
    }
}
