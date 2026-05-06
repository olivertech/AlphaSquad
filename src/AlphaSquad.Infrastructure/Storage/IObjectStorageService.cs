namespace AlphaSquad.Infrastructure.Storage;

public interface IObjectStorageService
{
    Task<UploadResult> UploadAsync(Stream fileStream, string fileName, string contentType, string path);
    Task DeleteAsync(string key);
}
