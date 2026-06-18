namespace AlphaSquad.Shared.DTOs.Common;

using System.Text.RegularExpressions;

/// <summary>
/// Contrato resumido e padronizado para cards mobile.
/// Ele permite que o app renderize feeds diferentes com a mesma estrutura base,
/// reduzindo regras especificas por modulo no cliente.
/// </summary>
public record MobileCardItemResponse(
    string CardType,
    string Title,
    string? Subtitle,
    string? Summary,
    string? ImageUrl,
    string? ThumbnailUrl,
    string? Badge,
    string StatusCode,
    DateTime? ReferenceDate,
    string TargetModule,
    Guid? TargetEntityId,
    string TargetRouteHint
);

/// <summary>
/// Helper central para gerar codigos estaveis de contrato mobile.
/// Esses codigos sao voltados ao consumo por app e evitam que o cliente dependa de textos de UI.
/// </summary>
public static class MobileContractCodes
{
    private static readonly Regex PascalCaseBoundary = new("([a-z0-9])([A-Z])", RegexOptions.Compiled);

    public static string ToCode(Enum value)
    {
        var raw = value.ToString();
        return PascalCaseBoundary.Replace(raw, "$1-$2").ToLowerInvariant();
    }

    public static string FromBoolean(bool value, string trueCode, string falseCode)
    {
        return value ? trueCode : falseCode;
    }

    public static string BuildRouteHint(string module, Guid? entityId)
    {
        return entityId.HasValue
            ? $"/{module}/{entityId.Value}"
            : $"/{module}";
    }

    public static string ResolveTargetModule(string? relatedEntityType, string fallbackModule)
    {
        if (string.IsNullOrWhiteSpace(relatedEntityType))
            return fallbackModule;

        var normalized = relatedEntityType.Trim().ToLowerInvariant();

        return normalized switch
        {
            "event" or "events" or "academy_event" or "academy-event" => "events",
            "class" or "classes" or "gym_class" or "gym-class" => "classes",
            "product" or "products" or "store" or "store_product" or "store-product" => "store/products",
            "social" or "social_post" or "social-post" or "post" => "social/posts",
            "workout" or "workouts" => "workouts",
            "notification" or "notifications" => "notifications",
            _ => fallbackModule
        };
    }
}
