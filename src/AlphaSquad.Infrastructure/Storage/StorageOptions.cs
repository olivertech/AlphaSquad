namespace AlphaSquad.Infrastructure.Storage;

public class StorageOptions
{
    public string Provider { get; set; } = default!;
    public string Endpoint { get; set; } = default!;
    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string BucketName { get; set; } = default!;
    public string PublicBaseUrl { get; set; } = default!;
}
