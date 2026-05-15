using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Shared.Enums;
using AlphaSquad.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlphaSquad.Web.Pages.Dashboard.Events;

/// <summary>
/// Tela de criação de eventos com matrícula inicial de alunos.
/// O gestor já sai do cadastro com a senha gerada e os participantes-base definidos.
/// </summary>
public sealed class CreateModel(IEventsService eventsService,
                                IUsersService usersService,
                                IMediaService mediaService) : AdminDashboardPageModelBase
{
    [BindProperty]
    public EventFormInputModel Input { get; set; } = new();

    [BindProperty]
    public IFormFile? EventImage { get; set; }

    [BindProperty]
    public List<Guid> SelectedStudentIds { get; set; } = [];

    public string? LoadErrorMessage { get; private set; }
    public IReadOnlyList<EventParticipantOptionViewModel> Students { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        await LoadStudentsAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (SelectedStudentIds.Count > 0 && (!Input.IsOutdoorEvent || !Input.AllowParticipation))
            ModelState.AddModelError(string.Empty, "Para matricular alunos, o evento precisa ser outdoor e permitir participação.");

        if (EventImage is not null && !EventImage.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(EventImage), "Selecione um arquivo de imagem válido para a propaganda do evento.");

        if (!ModelState.IsValid)
        {
            await LoadStudentsAsync(cancellationToken);
            return Page();
        }

        try
        {
            Guid? uploadedMediaId = null;

            if (EventImage is not null && EventImage.Length > 0)
            {
                uploadedMediaId = await UploadEventImageAsync(EventImage, cancellationToken);
                if (!uploadedMediaId.HasValue)
                {
                    await LoadStudentsAsync(cancellationToken);
                    return Page();
                }
            }

            var request = new CreateAcademyEventRequestDto
            {
                AllowParticipation = Input.AllowParticipation,
                Description = Input.Description,
                EndsAt = Input.EndsAt,
                IsActive = Input.IsActive,
                IsOutdoorEvent = Input.IsOutdoorEvent,
                Location = Input.Location,
                MediaId = uploadedMediaId,
                StartsAt = Input.StartsAt,
                Title = Input.Title.Trim()
            };

            var createdEvent = await eventsService.POSTApiEventsAsync(request, cancellationToken);
            if (createdEvent?.Id is not Guid eventId)
            {
                ShowErrorToast("Não foi possível criar o evento agora.");
                await LoadStudentsAsync(cancellationToken);
                return Page();
            }

            foreach (var studentId in SelectedStudentIds.Distinct())
                await eventsService.POSTApiEventsByIdParticipantsByUserIdAsync(eventId, studentId, cancellationToken);

            ShowSuccessToast("Evento criado com sucesso. A senha do check-in já está disponível na edição.", persist: true);
            return RedirectToPage("/Dashboard/Events/Edit", new { id = eventId, created = true });
        }
        catch (Exception exception)
        {
            ShowErrorToast(BuildFriendlyErrorMessage(exception, "Não foi possível criar o evento agora. Revise os dados e tente novamente."));
            await LoadStudentsAsync(cancellationToken);
            return Page();
        }
    }

    private async Task LoadStudentsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await usersService.GETApiUsersAsync(cancellationToken);
            Students = response?
                .Where(x => x.Id.HasValue && x.IsActive == true && x.Role == (int)UserRole.Student)
                .Select(x => new EventParticipantOptionViewModel
                {
                    Email = x.Email ?? string.Empty,
                    Id = x.Id!.Value,
                    Name = x.Name ?? "Aluno"
                })
                .OrderBy(x => x.Name)
                .ToList() ?? [];
        }
        catch
        {
            LoadErrorMessage = "Não foi possível carregar a lista de alunos para matrícula inicial.";
            ShowWarningToast(LoadErrorMessage);
            Students = [];
        }
    }

    private async Task<Guid?> UploadEventImageAsync(IFormFile image, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = new MemoryStream();
            await image.CopyToAsync(stream, cancellationToken);

            var uploadResponse = await mediaService.POSTApiMediaUploadAsync(new MultipartBodyDto
            {
                Content = stream.ToArray(),
                ContentType = image.ContentType,
                FileName = image.FileName
            }, cancellationToken);

            if (uploadResponse?.Id is Guid mediaId)
                return mediaId;

            ShowErrorToast("A imagem do evento foi enviada, mas o vínculo não pôde ser concluído.");
            return null;
        }
        catch (Exception exception)
        {
            ShowErrorToast(BuildFriendlyErrorMessage(exception, "Não foi possível enviar a imagem de divulgação do evento."));
            return null;
        }
    }

    private static string BuildFriendlyErrorMessage(Exception exception, string fallbackMessage)
    {
        var message = exception.Message;

        if (string.IsNullOrWhiteSpace(message))
            return fallbackMessage;

        if (message.Contains("400"))
            return $"{fallbackMessage} A API rejeitou algum dado enviado.";

        if (message.Contains("403"))
            return $"{fallbackMessage} O usuário atual não tem permissão para concluir essa operação.";

        return fallbackMessage;
    }
}
