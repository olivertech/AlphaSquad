using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlphaSquad.Web.Pages.Dashboard.Events;

/// <summary>
/// ViewModel para representação de um evento no mural.
/// </summary>
public class EventCardViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MediaUrl { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsOutdoor { get; set; }
}

/// <summary>
/// Tela do Mural de Eventos.
/// Apresenta os eventos da academia em formato de cards para fácil visualização.
/// </summary>
public sealed class IndexModel(IEventsService eventsService) : AdminDashboardPageModelBase
{
    public IReadOnlyList<EventCardViewModel> Events { get; private set; } = [];
    public string? LoadErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        try
        {
            // Corrigido: Passando parâmetros obrigatórios de paginação para satisfazer a assinatura do método.
            var response = await eventsService.GETApiEventsAsync(
                isActive: true, 
                onlyOutdoor: true, 
                page: 1, 
                pageSize: 50, 
                cancellationToken: cancellationToken);
            
            Events = response?.Select(e => new EventCardViewModel
            {
                Id = e.Id ?? Guid.Empty,
                Title = e.Title ?? "Evento Sem Título",
                Description = e.Description,
                MediaUrl = e.MediaUrl,
                StartsAt = e.StartsAt,
                EndsAt = e.EndsAt,
                IsActive = e.IsActive == true,
                IsOutdoor = e.IsOutdoorEvent == true
            }).OrderByDescending(e => e.StartsAt).ToList() ?? [];
        }
        catch
        {
            LoadErrorMessage = "Não foi possível carregar os eventos do mural agora.";
            ShowErrorToast(LoadErrorMessage);
        }

        return result;
    }
}
