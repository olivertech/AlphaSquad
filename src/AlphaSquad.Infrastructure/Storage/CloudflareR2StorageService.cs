using AlphaSquad.Shared.Contracts;

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

    public async Task<UploadResult> UploadAsync(Stream fileStream, string fileName, string contentType, string path)
    {
        var sanitizedFileName = fileName.Replace(" ", "_");

        var key = $"{path}/{Guid.NewGuid()}_{sanitizedFileName}";

        // Esse é o formato recomendado pela Cloudflare para garantir compatibilidade total com o R2,
        // especialmente em relação à assinatura de payloads e validação de checksums.
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType,

            // ESSENCIAL PARA R2
            DisablePayloadSigning = true,
            DisableDefaultChecksumValidation = true
        };

        // Aqui estamos usando o método PutObjectAsync, que é compatível com o R2 e respeita as configurações de assinatura e checksum.
        // O R2 tem requisitos específicos para a assinatura de payloads e validação de checksums, e o uso do PutObjectAsync com as
        // opções corretas garante que os arquivos sejam armazenados corretamente sem erros relacionados a assinaturas ou validações.
        await _client.PutObjectAsync(request);

        return new UploadResult
        {
            Key = key,
            Url = $"{_options.PublicBaseUrl}/{key}"
        };
    }
}