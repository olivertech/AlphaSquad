namespace AlphaSquad.Shared.DTOs.Media;

using AlphaSquad.Shared.DTOs.Common;

public record MediaResponse(
    Guid Id,
    string FileName,
    string ContentType,
    string Url
)
{
    public MediaPresentationResponse Presentation => MediaPresentationFactory.ForLibraryAsset(FileName, Url);
}

public record TenantMediaResponse(
    Guid Id,
    Guid TenantId,
    string FileName,
    string ContentType,
    long Size,
    string Url,
    DateTime CreatedAt
)
{
    public MediaPresentationResponse Presentation => MediaPresentationFactory.ForLibraryAsset(FileName, Url);
}

public record UploadMediaResponse(
    string Url
);

public record UpdateTenantMediaRequest(
    string FileName
);

public record UploadResult(
    string Key,
    string Url
);
