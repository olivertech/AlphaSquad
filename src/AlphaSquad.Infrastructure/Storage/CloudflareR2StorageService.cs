public class CloudflareR2StorageService : IObjectStorageService
{
    private readonly StorageOptions _options;
    private readonly IAmazonS3 _client;

    public CloudflareR2StorageService(IOptions<StorageOptions> options)
    {
        _options = options.Value;

        var config = new AmazonS3Config
        {
            ServiceURL = $"https://{_options.Endpoint}",
            ForcePathStyle = true
        };

        _client = new AmazonS3Client(
            _options.AccessKey,
            _options.SecretKey,
            config
        );
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string path)
    {
        var key = $"{path}/{Guid.NewGuid()}_{fileName}";

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType,
            DisablePayloadSigning = true,
        };

        await _client.PutObjectAsync(request);

        return $"{_options.PublicBaseUrl}/{key}";
    }
}