namespace AlphaSquad.Infrastructure.Storage;

public interface IObjectStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string path);
}
