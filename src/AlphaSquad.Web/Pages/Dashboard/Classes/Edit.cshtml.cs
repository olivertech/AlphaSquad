using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Classes;
using AlphaSquad.Web.Models;
using AlphaSquad.Web.Users;

namespace AlphaSquad.Web.Pages.Dashboard.Classes;

/// <summary>
/// Tela de edicao de aulas.
/// Ela ajuda a academia a ajustar agenda, instrutor, lotacao e status operacional sem sair do painel.
/// </summary>
public sealed class EditModel(
    IClassesService classesService,
    IUsersService usersService) : AdminDashboardPageModelBase
{
    [BindProperty]
    public ClassFormInputModel Input { get; set; } = new();

    public Guid ClassId { get; private set; }
    public IReadOnlyList<InstructorOptionViewModel> InstructorOptions { get; private set; } = [];
    public string? LoadErrorMessage { get; private set; }
    public string? SubmitErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        ClassId = id;
        await LoadInstructorOptionsAsync(cancellationToken);
        return await LoadClassAsync(id, cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        ClassId = id;
        await LoadInstructorOptionsAsync(cancellationToken);

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var response = await classesService.PUTApiClassesByIdAsync(id, new UpdateGymClassRequestDto
            {
                Name = Input.Name.Trim(),
                Description = NormalizeOptional(Input.Description),
                Location = NormalizeOptional(Input.Location),
                Capacity = Input.Capacity,
                StartsAt = ToLocalOffset(Input.StartsAt),
                EndsAt = ToLocalOffset(Input.EndsAt),
                InstructorUserId = Input.InstructorUserId,
                IsSpecialClass = Input.IsSpecialClass,
                IsActive = Input.IsActive
            }, cancellationToken);

            if (response?.Id is Guid classId)
                return RedirectToPage("/Dashboard/Classes/Details", new { id = classId, updated = true });

            SubmitErrorMessage = "Nao foi possivel salvar as alteracoes da aula agora. Tente novamente em instantes.";
        }
        catch
        {
            SubmitErrorMessage = "Nao foi possivel salvar as alteracoes da aula agora. Revise os dados e tente novamente.";
        }

        return Page();
    }

    /// <summary>
    /// Busca os dados da aula e converte para o formato mais amigavel do formulario.
    /// </summary>
    private async Task<IActionResult> LoadClassAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await classesService.GETApiClassesByIdAsync(id, cancellationToken);
            if (response?.Id is not Guid)
                return RedirectToPage("/Dashboard/Classes/Index");

            Input = ClassPresentationMapper.ToFormInput(response);
        }
        catch
        {
            LoadErrorMessage = "Nao foi possivel carregar os dados desta aula agora.";
        }

        return Page();
    }

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
