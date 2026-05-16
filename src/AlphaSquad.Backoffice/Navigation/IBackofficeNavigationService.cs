namespace AlphaSquad.Backoffice.Navigation;

public interface IBackofficeNavigationService
{
    IReadOnlyList<BackofficeMenuSection> Build();
}
