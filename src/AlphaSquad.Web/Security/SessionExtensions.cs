using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace AlphaSquad.Web.Security;

/// <summary>
/// Encapsula a leitura e escrita do contexto autenticado do dashboard na sessao HTTP.
/// </summary>
public static class SessionExtensions
{
    private const string DashboardSessionKey = "dashboard_session";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static void SetDashboardSession(this ISession session, DashboardSessionState state)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(state);

        session.SetString(DashboardSessionKey, JsonSerializer.Serialize(state, Options));
    }

    public static DashboardSessionState? GetDashboardSession(this ISession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var json = session.GetString(DashboardSessionKey);

        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<DashboardSessionState>(json, Options);
    }

    public static void ClearDashboardSession(this ISession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.Remove(DashboardSessionKey);
    }
}
