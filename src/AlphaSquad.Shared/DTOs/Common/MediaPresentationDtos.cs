namespace AlphaSquad.Shared.DTOs.Common;

/// <summary>
/// Descreve como o cliente deve apresentar uma imagem no app.
/// O objetivo e separar a URL bruta da semantica visual esperada para cada contexto.
/// </summary>
public record MediaPresentationResponse(
    string? ImageUrl,
    string? ThumbnailUrl,
    string UsageCode,
    string AspectRatioCode,
    string FallbackStyleCode,
    string? FallbackLabel,
    bool HasImage
);

/// <summary>
/// Fabrica central dos presets de midia usados pelo app.
/// Nesta fase as thumbnails reaproveitam a mesma URL principal, mas o contrato ja nasce pronto
/// para futuro resize server-side ou CDN sem quebrar o cliente.
/// </summary>
public static class MediaPresentationFactory
{
    public static MediaPresentationResponse ForEvent(string title, string? mediaUrl)
    {
        return Build(mediaUrl, "event-cover", "16:9", "banner", CreateInitials(title));
    }

    public static MediaPresentationResponse ForSocialPost(string userName, string? mediaUrl)
    {
        return Build(mediaUrl, "social-post", "4:5", "content", CreateInitials(userName));
    }

    public static MediaPresentationResponse ForStoreProduct(string productName, string? mediaUrl)
    {
        return Build(mediaUrl, "store-product", "1:1", "product", CreateInitials(productName));
    }

    public static MediaPresentationResponse ForNotification(string title, string? mediaUrl)
    {
        return Build(mediaUrl, "notification-cover", "16:9", "banner", CreateInitials(title));
    }

    public static MediaPresentationResponse ForAvatar(string displayName, string? mediaUrl)
    {
        return Build(mediaUrl, "avatar", "1:1", "avatar", CreateInitials(displayName));
    }

    public static MediaPresentationResponse ForLogo(string displayName, string? mediaUrl)
    {
        return Build(mediaUrl, "tenant-logo", "1:1", "logo", CreateInitials(displayName));
    }

    public static MediaPresentationResponse ForLibraryAsset(string fileName, string? mediaUrl)
    {
        return Build(mediaUrl, "library-asset", "1:1", "file", CreateInitials(fileName));
    }

    private static MediaPresentationResponse Build(string? mediaUrl,
                                                   string usageCode,
                                                   string aspectRatioCode,
                                                   string fallbackStyleCode,
                                                   string? fallbackLabel)
    {
        var normalizedUrl = string.IsNullOrWhiteSpace(mediaUrl) ? null : mediaUrl.Trim();

        return new MediaPresentationResponse(
            normalizedUrl,
            normalizedUrl,
            usageCode,
            aspectRatioCode,
            fallbackStyleCode,
            fallbackLabel,
            normalizedUrl is not null);
    }

    private static string? CreateInitials(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var parts = text.Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
            return null;

        if (parts.Length == 1)
            return parts[0][0].ToString().ToUpperInvariant();

        return string.Concat(parts[0][0], parts[^1][0]).ToUpperInvariant();
    }
}
