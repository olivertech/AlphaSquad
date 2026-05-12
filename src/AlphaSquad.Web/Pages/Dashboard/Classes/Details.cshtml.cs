using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Classes;
using AlphaSquad.Web.Models;
using AlphaSquad.Web.Security;

namespace AlphaSquad.Web.Pages.Dashboard.Classes;

/// <summary>
/// Tela detalhada de uma aula.
/// Ela junta o resumo da turma com a lista de reservas para facilitar o acompanhamento administrativo.
/// </summary>
public sealed class DetailsModel(
    IClassesService classesService,
    IUsersService usersService) : DashboardPageModelBase
{
    [BindProperty]
    public Guid? SelectedStudentId { get; set; }

    public ClassDetailsViewModel? ClassDetails { get; private set; }
    public IReadOnlyList<ClassBookingListItemViewModel> Bookings { get; private set; } = [];
    public IReadOnlyList<StudentOptionViewModel> StudentOptions { get; private set; } = [];
    public bool CanManageClasses => SessionState?.Role == DashboardRoles.Admin;
    public string? LoadErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        Guid id,
        bool? created,
        bool? updated,
        bool? deleted,
        bool? booked,
        bool? unbooked,
        CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (created == true)
            ShowSuccessToast("Aula cadastrada com sucesso.");
        else if (updated == true)
            ShowSuccessToast("Aula atualizada com sucesso.");
        else if (deleted == true)
            ShowSuccessToast("A aula foi removida com sucesso.");
        else if (booked == true)
            ShowSuccessToast("Reserva criada com sucesso para o aluno selecionado.");
        else if (unbooked == true)
            ShowSuccessToast("Reserva removida com sucesso.");

        await LoadStudentOptionsAsync(cancellationToken);
        await LoadClassDataAsync(id, cancellationToken);
        return result;
    }

    public async Task<IActionResult> OnPostAddBookingAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (!CanManageClasses)
            return Forbid();

        if (!SelectedStudentId.HasValue || SelectedStudentId == Guid.Empty)
        {
            ShowWarningToast("Selecione um aluno para confirmar a reserva.");
            await LoadStudentOptionsAsync(cancellationToken);
            await LoadClassDataAsync(id, cancellationToken);
            return Page();
        }

        try
        {
            await classesService.POSTApiClassesByIdBookingsAsync(id.ToString(), new CreateClassBookingForUserRequestDto
            {
                UserId = SelectedStudentId
            }, cancellationToken);

            return RedirectToPage(new { id, booked = true });
        }
        catch
        {
            ShowErrorToast("Não foi possível confirmar a reserva agora. Verifique se o aluno já está reservado ou se ainda existem vagas.");
            await LoadStudentOptionsAsync(cancellationToken);
            await LoadClassDataAsync(id, cancellationToken);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostRemoveBookingAsync(Guid id, Guid bookingId, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (!CanManageClasses)
            return Forbid();

        try
        {
            await classesService.DELETEApiClassesByIdBookingsByBookingIdAsync(id.ToString(), bookingId, cancellationToken);
            return RedirectToPage(new { id, unbooked = true });
        }
        catch
        {
            ShowErrorToast("Não foi possível remover a reserva agora. Tente novamente em instantes.");
            await LoadStudentOptionsAsync(cancellationToken);
            await LoadClassDataAsync(id, cancellationToken);
            return Page();
        }
    }

    /// <summary>
    /// Carrega os alunos ativos do tenant para uso na reserva administrativa.
    /// </summary>
    private async Task LoadStudentOptionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var users = await usersService.GETApiUsersAsync(cancellationToken) ?? [];
            StudentOptions = users
                .Where(user => user.Id.HasValue && user.IsActive == true && user.Role == 3)
                .OrderBy(user => user.Name ?? string.Empty)
                .Select(user => new StudentOptionViewModel
                {
                    Value = user.Id!.Value,
                    Name = string.IsNullOrWhiteSpace(user.Name) ? "Aluno sem nome" : user.Name.Trim(),
                    Email = string.IsNullOrWhiteSpace(user.Email) ? "E-mail não informado" : user.Email.Trim()
                })
                .ToList();
        }
        catch
        {
            StudentOptions = [];
        }
    }

    /// <summary>
    /// Recarrega a aula e as reservas para a tela de detalhes.
    /// Esse mesmo fluxo é reaproveitado tanto no GET quanto após tentativas de reserva administrativa.
    /// </summary>
    private async Task LoadClassDataAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var response = await classesService.GETApiClassesByIdAsync(id, cancellationToken);
            if (response?.Id is not Guid)
            {
                LoadErrorMessage = "A aula informada não foi encontrada.";
                ShowWarningToast(LoadErrorMessage);
                return;
            }

            var bookings = await classesService.GETApiClassesByIdBookingsAsync(id.ToString(), null, cancellationToken) ?? [];
            ClassDetails = ClassPresentationMapper.ToDetails(response, bookings.Count);
            Bookings = bookings
                .Where(item => item.BookingId.HasValue && item.GymClassId.HasValue && item.UserId.HasValue)
                .Select(ClassPresentationMapper.ToBookingItem)
                .OrderByDescending(item => item.BookedAt ?? DateTimeOffset.MinValue)
                .ToList();
        }
        catch
        {
            LoadErrorMessage = "Não foi possível carregar os detalhes desta aula agora.";
            ShowErrorToast(LoadErrorMessage);
        }
    }
}
