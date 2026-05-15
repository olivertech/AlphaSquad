using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlphaSquad.Web.Pages.Dashboard.Events;

/// <summary>
/// Representa um evento no painel administrativo, destacando o estado operacional para a gestão.
/// </summary>
public sealed class EventListItemViewModel
{
    public bool AllowParticipation { get; set; }
    public bool CanComplete { get; set; }
    public bool CanDeletePhysically { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsOutdoorEvent { get; set; }
    public string? Location { get; set; }
    public string? MediaUrl { get; set; }
    public int ParticipantCount { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public string StatusToneClass { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// Central administrativa do mural de eventos.
/// Ela permite filtrar, concluir, remover e navegar para criação ou gestão de participantes.
/// </summary>
public sealed class IndexModel(IEventsService eventsService) : AdminDashboardPageModelBase
{
    [BindProperty(SupportsGet = true)]
    public bool IncludeInactive { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public IReadOnlyList<EventListItemViewModel> Events { get; private set; } = [];
    public string? LoadErrorMessage { get; private set; }
    public int TotalCompleted => Events.Count(x => x.IsCompleted);
    public int TotalEnrolled => Events.Sum(x => x.ParticipantCount);
    public int TotalEvents => Events.Count;
    public int TotalOutdoor => Events.Count(x => x.IsOutdoorEvent);

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        await LoadEventsAsync(cancellationToken);
        return Page();
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
        }
        catch
        {
            ShowErrorToast("Não foi possível remover o evento agora.", persist: true);
        }

        return RedirectToPage(new { Search, IncludeInactive });
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
        }
        catch
        {
            ShowErrorToast("Não foi possível concluir o evento agora.", persist: true);
        }

        return RedirectToPage(new { Search, IncludeInactive });
    }

    private async Task LoadEventsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await eventsService.GETApiEventsAsync(
                isActive: IncludeInactive ? null : true,
                onlyOutdoor: null,
                page: 1,
                pageSize: 100,
                cancellationToken: cancellationToken);

            var normalizedSearch = Search?.Trim();
            Events = response?
                .Select(MapEvent)
                .Where(x => string.IsNullOrWhiteSpace(normalizedSearch) ||
                            x.Title.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                            (!string.IsNullOrWhiteSpace(x.Location) && x.Location.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(x => x.StartsAt ?? DateTimeOffset.MinValue)
                .ThenByDescending(x => x.Id)
                .ToList() ?? [];
        }
        catch
        {
            LoadErrorMessage = "Não foi possível carregar o mural de eventos agora.";
            ShowErrorToast(LoadErrorMessage);
            Events = [];
        }
    }

    private static EventListItemViewModel MapEvent(AcademyEventResponseDto response)
    {
        var startsAt = response.StartsAt;
        var isCompleted = response.IsCompleted == true;
        var isActive = response.IsActive == true;
        var hasStarted = startsAt.HasValue && startsAt.Value <= DateTimeOffset.Now;

        var (statusLabel, statusToneClass) = ResolveStatus(isActive, isCompleted, hasStarted);

        return new EventListItemViewModel
        {
            AllowParticipation = response.AllowParticipation == true,
            CanComplete = isActive && !isCompleted && hasStarted,
            CanDeletePhysically = !hasStarted,
            Description = response.Description,
            EndsAt = response.EndsAt,
            Id = response.Id ?? Guid.Empty,
            IsActive = isActive,
            IsCompleted = isCompleted,
            IsOutdoorEvent = response.IsOutdoorEvent == true,
            Location = response.Location,
            MediaUrl = response.MediaUrl,
            ParticipantCount = response.ParticipantCount ?? 0,
            StartsAt = startsAt,
            StatusLabel = statusLabel,
            StatusToneClass = statusToneClass,
            Title = response.Title ?? "Evento sem título"
        };
    }

    private static (string Label, string ToneClass) ResolveStatus(bool isActive, bool isCompleted, bool hasStarted)
    {
        if (isCompleted)
            return ("Concluído", "bg-emerald-50 text-emerald-700");

        if (!isActive)
            return ("Inativo", "bg-slate-100 text-slate-600");

        if (hasStarted)
            return ("Em andamento", "bg-amber-50 text-amber-700");

        return ("Agendado", "bg-sky-50 text-sky-700");
    }
}
