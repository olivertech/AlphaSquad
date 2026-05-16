namespace AlphaSquad.Backoffice.Services;

/// <summary>
/// Representa uma falha retornada pelas APIs master consumidas pelo backoffice.
/// Ela preserva o status code para tratamentos mais amigaveis nas paginas Razor.
/// </summary>
public sealed class BackofficeApiException : Exception
{
    public BackofficeApiException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}
