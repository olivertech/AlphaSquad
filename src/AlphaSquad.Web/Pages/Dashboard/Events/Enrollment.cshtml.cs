using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Shared.Enums;
using AlphaSquad.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlphaSquad.Web.Pages.Dashboard.Events;

/// <summary>
/// ViewModel de gestão de inscritos do evento.
/// Ele separa quem já está matriculado de quem ainda pode ser convidado para a ação.
/// </summary>
public sealed class EnrollmentViewModel
{
    public IReadOnlyList<EventParticipantOptionViewModel> AvailableStudents { get; set; } = [];
    public IReadOnlyList<EventParticipantOptionViewModel> EnrolledStudents { get; set; } = [];
    public Guid EventId { get; set; }
    public bool EventIsCompleted { get; set; }
    public string EventTitle { get; set; } = string.Empty;
}

/// <summary>
/// Tela administrativa de inscrições do evento.
/// Permite matricular e remover alunos, além de acompanhar quem já confirmou presença.
/// </summary>
public sealed class EnrollmentModel(IEventsService eventsService, IUsersService usersService) : AdminDashboardPageModelBase
{
    public EnrollmentViewModel Enrollment { get; private set; } = new();
    public string? LoadErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        var loaded = await LoadAsync(id, cancellationToken);
        if (!loaded)
            return RedirectToPage("/Dashboard/Events/Index");

        if (!string.IsNullOrWhiteSpace(LoadErrorMessage))
        {
            ShowWarningToast(LoadErrorMessage, persist: true);
            return RedirectToPage("/Dashboard/Events/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostEnrollAsync(Guid eventId, Guid userId, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        try
        {
            await eventsService.POSTApiEventsByIdParticipantsByUserIdAsync(eventId, userId, cancellationToken);
            ShowSuccessToast("Aluno matriculado no evento com sucesso.", persist: true);
        }
        catch
        {
            ShowErrorToast("Não foi possível matricular o aluno nesse evento.", persist: true);
        }

        return RedirectToPage(new { id = eventId });
    }

    public async Task<IActionResult> OnPostRemoveAsync(Guid eventId, Guid userId, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        try
        {
            await eventsService.DELETEApiEventsByIdParticipantsByUserIdAsync(eventId, userId, cancellationToken);
            ShowSuccessToast("Aluno removido da matrícula do evento.", persist: true);
        }
        catch
        {
            ShowErrorToast("Não foi possível remover o aluno deste evento.", persist: true);
        }

        return RedirectToPage(new { id = eventId });
    }

    private async Task<bool> LoadAsync(Guid eventId, CancellationToken cancellationToken)
    {
        try
        {
            var eventResponse = await eventsService.GETApiEventsByIdAsync(eventId, cancellationToken);
            if (eventResponse?.Id is not Guid)
            {
                ShowWarningToast("O evento solicitado não foi encontrado.", persist: true);
                return false;
            }

            var participants = await eventsService.GETApiEventsByIdParticipantsAsync(eventId, cancellationToken) ?? [];
            var allUsers = await usersService.GETApiUsersAsync(cancellationToken) ?? [];

            var enrolledStudents = participants
                .Where(x => x.UserId.HasValue)
                .Select(x => new EventParticipantOptionViewModel
                {
                    Email = x.UserEmail ?? string.Empty,
                    Id = x.UserId!.Value,
                    IsPresent = x.IsPresent == true,
                    Name = x.UserName ?? "Aluno",
                    ParticipatedAt = x.ParticipatedAt
                })
                .OrderBy(x => x.Name)
                .ToList();

            var enrolledIds = enrolledStudents.Select(x => x.Id).ToHashSet();

            var availableStudents = allUsers
                .Where(x => x.Id.HasValue && x.IsActive == true && x.Role == (int)UserRole.Student && !enrolledIds.Contains(x.Id.Value))
                .Select(x => new EventParticipantOptionViewModel
                {
                    Email = x.Email ?? string.Empty,
                    Id = x.Id!.Value,
                    Name = x.Name ?? "Aluno"
                })
                .OrderBy(x => x.Name)
                .ToList();

            Enrollment = new EnrollmentViewModel
            {
                AvailableStudents = availableStudents,
                EnrolledStudents = enrolledStudents,
                EventId = eventId,
                EventIsCompleted = eventResponse.IsCompleted == true,
                EventTitle = eventResponse.Title ?? "Evento"
            };

            return true;
        }
        catch
        {
            LoadErrorMessage = "Não foi possível carregar a gestão de inscrições deste evento.";
            return true;
        }
    }
}
