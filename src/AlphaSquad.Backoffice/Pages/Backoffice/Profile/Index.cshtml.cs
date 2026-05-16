using AlphaSquad.Backoffice.Security;
using AlphaSquad.Backoffice.Services;
using AlphaSquad.Shared.DTOs.PlatformProfile;

namespace AlphaSquad.Backoffice.Pages.Backoffice.Profile;

public sealed class IndexModel(IBackofficeOwnerProfileService profileService) : BackofficePageModelBase
{
    public BackofficeOwnerProfileViewModel? Profile { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = PageOrLogin();
        if (result is not PageResult)
            return result;

        try
        {
            var response = await profileService.GetAsync(cancellationToken);
            Profile = BackofficeOwnerProfileViewModel.FromResponse(response);
        }
        catch
        {
            ShowErrorToast("Nao foi possivel carregar o perfil do owner.");
        }

        return Page();
    }

    public sealed class BackofficeOwnerProfileViewModel
    {
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string RoleLabel { get; init; } = "Owner";
        public string? ProfilePhotoUrl { get; init; }
        public string Initials { get; init; } = "A";

        public static BackofficeOwnerProfileViewModel FromResponse(PlatformProfileResponse response)
        {
            var initials = string.Concat(response.Name
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(part => char.ToUpperInvariant(part[0])));

            return new BackofficeOwnerProfileViewModel
            {
                Name = response.Name,
                Email = response.Email,
                RoleLabel = response.Role,
                ProfilePhotoUrl = response.ProfilePhotoUrl,
                Initials = string.IsNullOrWhiteSpace(initials) ? "A" : initials
            };
        }
    }
}
