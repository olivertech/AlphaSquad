using AlphaSquad.Lmt.Application.Contracts.Interfaces;
using AlphaSquad.Web.Models;
using AlphaSquad.Web.Security;
using AlphaSquad.Web.Users;

namespace AlphaSquad.Web.Pages.Dashboard.Users;

/// <summary>
/// Tela principal do módulo de usuários.
/// Ela resume a base atual, permite filtros rápidos e cria a porta de entrada para cadastro, edição e detalhes.
/// </summary>
public sealed class IndexModel(IUsersService usersService) : AdminDashboardPageModelBase
{
    public IReadOnlyList<UserListItemViewModel> Users { get; private set; } = [];
    public IReadOnlyList<UserListItemViewModel> FilteredUsers { get; private set; } = [];
    public PagedResultViewModel<UserListItemViewModel>? PagedUsers { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string Search { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public int? Role { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Status { get; set; } = "todos";

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public int TotalUsers => Users.Count;
    public int ActiveUsers => Users.Count(user => user.IsActive);
    public int Students => Users.Count(user => user.RoleValue == 3);
    public int Teachers => Users.Count(user => user.RoleValue == 2);
    public int InactiveUsers => Users.Count(user => !user.IsActive);
    public string? LoadErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }
    public IReadOnlyList<UserRoleOptionViewModel> RoleOptions => UserPresentationMapper.RoleOptions;
    public IReadOnlyList<int> PageSizeOptions => [10, 20, 30];

    public async Task<IActionResult> OnGetAsync(bool? deactivated, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        if (deactivated == true)
            SuccessMessage = "Usuário desativado com sucesso.";

        try
        {
            var response = await usersService.GETApiUsersAsync(cancellationToken);
            Users = response?
                .Where(user => user.Id.HasValue)
                .Select(UserPresentationMapper.ToListItem)
                .OrderBy(user => user.Name)
                .ToList() ?? [];

            FilteredUsers = ApplyFilters(Users);
            PagedUsers = ApplyPagination(FilteredUsers);
        }
        catch
        {
            LoadErrorMessage = "Não foi possível carregar os usuários da academia agora. Tente novamente em instantes.";
        }

        return result;
    }

    /// <summary>
    /// Aplica filtros locais para manter a experiência da gestão rápida, sem depender de novos parâmetros na API.
    /// </summary>
    private IReadOnlyList<UserListItemViewModel> ApplyFilters(IReadOnlyList<UserListItemViewModel> users)
    {
        var query = users.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var normalizedSearch = Search.Trim();
            query = query.Where(user =>
                user.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                user.Email.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
        }

        if (Role.HasValue)
            query = query.Where(user => user.RoleValue == Role.Value);

        query = Status?.ToLowerInvariant() switch
        {
            "ativos" => query.Where(user => user.IsActive),
            "inativos" => query.Where(user => !user.IsActive),
            _ => query
        };

        return query.ToList();
    }

    /// <summary>
    /// Pagina os resultados já filtrados para manter a listagem leve e agradável no dashboard.
    /// </summary>
    private PagedResultViewModel<UserListItemViewModel> ApplyPagination(IReadOnlyList<UserListItemViewModel> users)
    {
        var normalizedPageSize = PageSizeOptions.Contains(PageSize) ? PageSize : 10;
        var totalItems = users.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling((double)Math.Max(totalItems, 1) / normalizedPageSize));
        var normalizedPageNumber = Math.Min(Math.Max(PageNumber, 1), totalPages);

        var pageItems = users
            .Skip((normalizedPageNumber - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToList();

        PageSize = normalizedPageSize;
        PageNumber = normalizedPageNumber;

        return new PagedResultViewModel<UserListItemViewModel>
        {
            Items = pageItems,
            PageNumber = normalizedPageNumber,
            PageSize = normalizedPageSize,
            TotalItems = totalItems
        };
    }

    /// <summary>
    /// Desativa rapidamente um usuário a partir da listagem principal, sem exigir ida à tela de detalhes.
    /// </summary>
    public async Task<IActionResult> OnPostDeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        try
        {
            await usersService.DELETEApiUsersByIdAsync(id, cancellationToken);

            return RedirectToPage(new
            {
                Search,
                Role,
                Status,
                PageNumber,
                PageSize,
                deactivated = true
            });
        }
        catch
        {
            LoadErrorMessage = "Não foi possível desativar esse usuário agora. Tente novamente em instantes.";
            return await OnGetAsync(false, cancellationToken);
        }
    }
}
