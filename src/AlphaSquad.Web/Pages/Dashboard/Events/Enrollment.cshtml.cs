using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AlphaSquad.Web.Users;
using AlphaSquad.Web.Models;

namespace AlphaSquad.Web.Pages.Dashboard.Events;

/// <summary>
/// ViewModel para a gestão de participantes de um evento.
/// </summary>
public class EnrollmentViewModel
{
    public Guid EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public IReadOnlyList<UserListItemViewModel> AvailableUsers { get; set; } = [];
    public IReadOnlyList<UserListItemViewModel> EnrolledUsers { get; set; } = [];
}

/// <summary>
/// Tela de inscrições de participantes em um evento.
/// Permite adicionar e remover alunos e professores do evento.
/// </summary>
public sealed class EnrollmentModel(IEventsService eventsService, IUsersService usersService) : AdminDashboardPageModelBase
{
    public EnrollmentViewModel Enrollment { get; set; } = new();
    public string? LoadErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult) return result;

        try
        {
            var eventResponse = await eventsService.GETApiEventsByIdAsync(id, cancellationToken);
            if (eventResponse == null) return RedirectToPage("/Dashboard/Events/Index");

            var allUsersResponse = await usersService.GETApiUsersAsync(cancellationToken);
            var allUsers = allUsersResponse?.Select(UserPresentationMapper.ToListItem).ToList() ?? [];

            // O endpoint de listagem de participantes não existe na API no momento.
            // Retornando lista vazia temporariamente para permitir compilação.
            var enrolledIds = new List<Guid>();

            Enrollment = new EnrollmentViewModel
            {
                EventId = id,
                EventTitle = eventResponse.Title ?? "Evento",
                EnrolledUsers = allUsers.Where(u => enrolledIds.Contains(u.Id)).ToList(),
                AvailableUsers = allUsers.Where(u => !enrolledIds.Contains(u.Id)).ToList()
            };
        }
        catch
        {
            LoadErrorMessage = "Erro ao carregar a lista de inscrições.";
            ShowErrorToast(LoadErrorMessage);
        }

        return result;
    }

    public async Task<IActionResult> OnPostEnrollAsync(Guid eventId, Guid userId, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult) return result;

        try
        {
            await eventsService.POSTApiEventsByIdParticipateAsync(eventId.ToString(), cancellationToken);
            ShowSuccessToast("Participante inscrito com sucesso!");
        }
        catch
        {
            ShowErrorToast("Erro ao inscrever participante.");
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveAsync(Guid eventId, Guid userId, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult) return result;

        try
        {
            await eventsService.DELETEApiEventsByIdAsync(eventId, cancellationToken); 
            ShowSuccessToast("Participante removido com sucesso!");
        }
        catch
        {
            ShowErrorToast("Erro ao remover participante.");
        }

        return RedirectToPage();
    }
}
