namespace AlphaSquad.Shared.Helpers;

/// <summary>
/// Centraliza os codigos das features opcionais do produto.
/// Isso evita strings soltas e facilita o reuso entre seed, endpoints e validacoes.
/// </summary>
public static class FeatureCodes
{
    public const string Workouts = "WORKOUTS";
    public const string CheckIn = "CHECKIN";
    public const string Schedule = "SCHEDULE";
    public const string Media = "MEDIA";
    public const string UserManagement = "USER_MGMT";
    public const string Store = "STORE";
    public const string Social = "SOCIAL";
    public const string Gamification = "GAMIFICATION";
    public const string Events = "EVENTS";
}
