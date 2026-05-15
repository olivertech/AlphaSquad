using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlphaSquad.Web.Pages.Dashboard.Events;

/// <summary>
/// Tela de edição e operação de um evento.
/// Além dos dados básicos, ela expõe a senha do check-in e o estado de conclusão.
/// </summary>
public sealed class EditModel(IEventsService eventsService) : AdminDashboardPageModelBase
{
    [BindProperty]
    public EventFormInputModel Input { get; set; } = new();

    public bool CanComplete { get; private set; }
    public bool CanDeletePhysically { get; private set; }
    public string? CheckInPassword { get; private set; }
    public string? CurrentMediaUrl { get; private set; }
    public Guid EventId { get; private set; }
    public bool IsCompleted { get; private set; }
    public string? LoadErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken, bool created = false)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        EventId = id;
        await LoadEventAsync(id, cancellationToken);

        if (!string.IsNullOrWhiteSpace(LoadErrorMessage))
        {
            ShowWarningToast(LoadErrorMessage, persist: true);
            return RedirectToPage("/Dashboard/Events/Index");
        }

        if (created && string.IsNullOrWhiteSpace(LoadErrorMessage))
            ShowSuccessToast("Evento criado com sucesso. Use a senha abaixo para orientar o check-in no dia da ação.");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        EventId = id;
        if (!ModelState.IsValid)
        {
            await LoadEventAsync(id, cancellationToken, preserveInput: true);
            return Page();
        }

        try
        {
            var request = new UpdateAcademyEventRequestDto
            {
                AllowParticipation = Input.AllowParticipation,
                Description = Input.Description,
                EndsAt = Input.EndsAt,
                IsActive = Input.IsActive,
                IsOutdoorEvent = Input.IsOutdoorEvent,
                Location = Input.Location,
                MediaId = Input.MediaId,
                StartsAt = Input.StartsAt,
                Title = Input.Title.Trim()
            };

            await eventsService.PUTApiEventsByIdAsync(id, request, cancellationToken);
            ShowSuccessToast("Evento atualizado com sucesso.", persist: true);
            return RedirectToPage(new { id });
        }
        catch
        {
            ShowErrorToast("Não foi possível salvar as alterações do evento.");
            await LoadEventAsync(id, cancellationToken, preserveInput: true);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        try
        {
            await eventsService.DELETEApiEventsByIdAsync(id, cancellationToken);
            ShowSuccessToast("Evento removido com sucesso.", persist: true);
            return RedirectToPage("/Dashboard/Events/Index");
        }
        catch
        {
            ShowErrorToast("Não foi possível remover o evento agora.", persist: true);
            return RedirectToPage(new { id });
        }
    }

    public async Task<IActionResult> OnPostCompleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        try
        {
            var response = await eventsService.POSTApiEventsByIdCompleteAsync(id, cancellationToken);
            var awardedCount = response?.AwardedParticipantsCount ?? 0;
            ShowSuccessToast($"Evento concluído com sucesso. {awardedCount} aluno(s) receberam pontos.", persist: true);
            return RedirectToPage(new { id });
        }
        catch
        {
            ShowErrorToast("Não foi possível concluir o evento agora.", persist: true);
            return RedirectToPage(new { id });
        }
    }

    private async Task LoadEventAsync(Guid id, CancellationToken cancellationToken, bool preserveInput = false)
    {
        try
        {
            var response = await eventsService.GETApiEventsByIdAsync(id, cancellationToken);
            if (response?.Id is not Guid)
            {
                LoadErrorMessage = "Não foi possível encontrar o evento solicitado.";
                return;
            }

            if (!preserveInput)
            {
                Input = new EventFormInputModel
                {
                    AllowParticipation = response.AllowParticipation == true,
                    Description = response.Description,
                    EndsAt = response.EndsAt,
                    IsActive = response.IsActive == true,
                    IsOutdoorEvent = response.IsOutdoorEvent == true,
                    Location = response.Location,
                    MediaId = response.MediaId,
                    StartsAt = response.StartsAt,
                    Title = response.Title ?? string.Empty
                };
            }

            CurrentMediaUrl = response.MediaUrl;
            CheckInPassword = response.CheckInPassword;
            IsCompleted = response.IsCompleted == true;

            var hasStarted = response.StartsAt.HasValue && response.StartsAt.Value <= DateTimeOffset.Now;
            CanDeletePhysically = !hasStarted;
            CanComplete = response.IsActive == true && !IsCompleted && hasStarted;
        }
        catch
        {
            LoadErrorMessage = "Não foi possível carregar os dados do evento agora.";
        }
    }
}
