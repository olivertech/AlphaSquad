using System.Text.Json;

namespace AlphaSquad.Backoffice.Security;

/// <summary>
/// Encapsula a leitura e escrita do contexto autenticado do backoffice na sessao HTTP.
/// </summary>
public static class SessionExtensions
{
    private const string BackofficeSessionKey = "backoffice_session";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static void SetBackofficeSession(this ISession session, BackofficeSessionState state)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(state);

        session.SetString(BackofficeSessionKey, JsonSerializer.Serialize(state, Options));
    }

    public static BackofficeSessionState? GetBackofficeSession(this ISession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var json = session.GetString(BackofficeSessionKey);

        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<BackofficeSessionState>(json, Options);
    }

    public static void ClearBackofficeSession(this ISession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.Remove(BackofficeSessionKey);
    }
}
