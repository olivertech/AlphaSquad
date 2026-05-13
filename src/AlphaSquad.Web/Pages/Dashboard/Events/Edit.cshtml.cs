using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AlphaSquad.Web.Pages.Dashboard.Events;

/// <summary>
/// ViewModel para edição de eventos.
/// </summary>
public class EventEditInputModel
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public string? Location { get; set; }
    public Guid? MediaId { get; set; }
}

public sealed class EditModel(IEventsService eventsService) : AdminDashboardPageModelBase
{
    [BindProperty]
    public EventEditInputModel Input { get; set; } = new();
    public Guid EventId { get; private set; }
    public string? CurrentMediaUrl { get; private set; }
    public bool CanDeletePhysically { get; private set; }
    public string? LoadErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult) return result;

        EventId = id;
        try
        {
            var response = await eventsService.GETApiEventsByIdAsync(id, cancellationToken);
            if (response == null) return RedirectToPage("/Dashboard/Events/Index");

            Input = new EventEditInputModel
            {
                Title = response.Title ?? string.Empty,
                Description = response.Description,
                StartsAt = response.StartsAt,
                EndsAt = response.EndsAt,
                Location = response.Location,
                MediaId = response.MediaId
            };
            CurrentMediaUrl = response.MediaUrl;
            
            // Regra: Deleção física permitida apenas se o evento ainda não começou.
            CanDeletePhysically = response.StartsAt > DateTimeOffset.Now;
        }
        catch
        {
            LoadErrorMessage = "Erro ao carregar os dados do evento.";
            ShowErrorToast(LoadErrorMessage);
        }

        return result;
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult) return result;

        EventId = id;
        if (!ModelState.IsValid) return Page();

        try
        {
            var request = new UpdateAcademyEventRequestDto
            {
                Title = Input.Title.Trim(),
                Description = Input.Description,
                StartsAt = Input.StartsAt,
                EndsAt = Input.EndsAt,
                Location = Input.Location,
                MediaId = Input.MediaId
            };

            var response = await eventsService.PUTApiEventsByIdAsync(id, request, cancellationToken);
            if (response != null)
            {
                ShowSuccessToast("Evento atualizado com sucesso!");
                return RedirectToPage("/Dashboard/Events/Index");
            }
            ShowErrorToast("Erro ao salvar as alterações.");
        }
        catch
        {
            ShowErrorToast("Erro inesperado ao atualizar o evento.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult) return result;

        try
        {
            // Aqui a API deve validar a regra de negócio: 
            // Se StartsAt < Now -> Deleta Lógico (Inativa).
            // Se StartsAt > Now -> Deleta Físico.
            await eventsService.DELETEApiEventsByIdAsync(id, cancellationToken);
            ShowSuccessToast("Evento removido com sucesso!");
        }
        catch
        {
            ShowErrorToast("Erro ao remover o evento.");
        }

        return RedirectToPage("/Dashboard/Events/Index");
    }
}
