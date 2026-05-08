namespace AlphaSquad.Shared.DTOs.Media;

public record MediaResponse(
    Guid Id, 
    string FileName, 
    string ContentType, 
    string Url
);

public record TenantMediaResponse(
    Guid Id, 
    Guid TenantId, 
    string FileName, 
    string ContentType, 
    long Size, 
    string StorageKey, 
    string Url, 
    DateTime CreatedAt
);

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
