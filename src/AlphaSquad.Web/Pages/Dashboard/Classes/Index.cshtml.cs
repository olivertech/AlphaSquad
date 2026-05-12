using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Classes;
using AlphaSquad.Web.Models;
using AlphaSquad.Web.Security;

namespace AlphaSquad.Web.Pages.Dashboard.Classes;

/// <summary>
/// Visão operacional do módulo de aulas.
/// Ela reúne agenda, ocupação e reservas recentes para ajudar a academia na rotina de turmas e professores.
/// </summary>
public sealed class IndexModel(IClassesService classesService) : DashboardPageModelBase
{
    [BindProperty(SupportsGet = true)]
    public DateTime? DateFrom { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? DateTo { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; } = true;

    [BindProperty(SupportsGet = true)]
    public bool OnlySpecialClasses { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public IReadOnlyList<ClassListItemViewModel> Classes { get; private set; } = [];
    public IReadOnlyList<ClassBookingListItemViewModel> RecentBookings { get; private set; } = [];
    public IReadOnlyList<ClassListItemViewModel> TopOccupiedClasses { get; private set; } = [];
    public bool CanManageClasses => SessionState?.Role == DashboardRoles.Admin;
    public int TotalClasses => Classes.Count;
    public int ActiveClasses => Classes.Count(item => item.IsActive);
    public int SpecialClasses => Classes.Count(item => item.IsSpecialClass);
    public int TotalReservations => Classes.Sum(item => item.BookingCount);
    public decimal AverageOccupancy => Classes.Count == 0 ? 0m : Math.Round(Classes.Average(item => item.OccupancyRate), 1);
    public string? LoadErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(bool? deleted, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (deleted == true)
            SuccessMessage = "A aula foi removida com sucesso.";

        if (DateTo.HasValue && DateFrom.HasValue && DateTo.Value < DateFrom.Value)
        {
            (DateFrom, DateTo) = (DateTo, DateFrom);
        }

        try
        {
            var classResponses = await classesService.GETApiClassesAsync(
                DateFrom?.Date,
                DateTo?.Date,
                IsActive,
                1,
                100,
                cancellationToken) ?? [];

            var filteredClasses = ApplyLocalFilters(classResponses);
            var bookingMap = await LoadBookingsByClassAsync(filteredClasses, cancellationToken);

            var classItems = filteredClasses
                .Where(item => item.Id.HasValue)
                .Select(item => ClassPresentationMapper.ToListItem(
                    item,
                    bookingMap.TryGetValue(item.Id!.Value, out var bookings) ? bookings.Count : 0))
                .OrderBy(item => item.StartsAt ?? DateTimeOffset.MaxValue)
                .ToList();

            Classes = classItems;
            TopOccupiedClasses = classItems
                .OrderByDescending(item => item.OccupancyRate)
                .ThenBy(item => item.StartsAt ?? DateTimeOffset.MaxValue)
                .Take(5)
                .ToList();

            RecentBookings = bookingMap.Values
                .SelectMany(list => list)
                .Where(item => item.BookingId.HasValue && item.GymClassId.HasValue && item.UserId.HasValue)
                .Select(ClassPresentationMapper.ToBookingItem)
                .OrderByDescending(item => item.BookedAt ?? DateTimeOffset.MinValue)
                .Take(10)
                .ToList();
        }
        catch
        {
            LoadErrorMessage = "Não foi possível carregar os dados de aulas agora. Tente novamente em instantes.";
        }

        return result;
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (!CanManageClasses)
            return Forbid();

        try
        {
            await classesService.DELETEApiClassesByIdAsync(id, cancellationToken);
            return RedirectToPage(new
            {
                Search,
                DateFrom,
                DateTo,
                IsActive,
                OnlySpecialClasses,
                deleted = true
            });
        }
        catch
        {
            LoadErrorMessage = "Nao foi possivel remover a aula agora.";
            return await OnGetAsync(false, cancellationToken);
        }
    }

    /// <summary>
    /// Aplica filtros que ainda não existem prontos na API, como busca textual e destaque apenas de aulões.
    /// </summary>
    private IReadOnlyList<GymClassResponseDto> ApplyLocalFilters(IReadOnlyList<GymClassResponseDto> classes)
    {
        var query = classes.AsEnumerable();

        if (OnlySpecialClasses)
        {
            query = query.Where(item => item.IsSpecialClass == true);
        }

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim();
            query = query.Where(item =>
                Contains(item.Name, term) ||
                Contains(item.Description, term) ||
                Contains(item.InstructorName, term) ||
                Contains(item.Location, term));
        }

        return query.ToList();
    }

    /// <summary>
    /// Carrega as reservas por aula para permitir indicadores de ocupação e uma visão recente de bookings.
    /// </summary>
    private async Task<Dictionary<Guid, List<ClassBookingManagementResponseDto>>> LoadBookingsByClassAsync(
        IReadOnlyList<GymClassResponseDto> classes,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, List<ClassBookingManagementResponseDto>>();

        foreach (var gymClass in classes.Where(item => item.Id.HasValue))
        {
            var classId = gymClass.Id!.Value;

            try
            {
                var bookings = await classesService.GETApiClassesByIdBookingsAsync(
                    classId.ToString(),
                    null,
                    cancellationToken) ?? [];

                result[classId] = bookings.ToList();
            }
            catch
            {
                result[classId] = [];
            }
        }

        return result;
    }

    private static bool Contains(string? source, string term) =>
        !string.IsNullOrWhiteSpace(source) &&
        source.Contains(term, StringComparison.OrdinalIgnoreCase);
}
