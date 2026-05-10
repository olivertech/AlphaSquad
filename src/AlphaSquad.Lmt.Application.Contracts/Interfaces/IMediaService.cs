using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface IMediaService
{
    Task DELETEApiMediaByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<TenantMediaResponseDto>?> GETApiMediaAsync(int? page, int? pageSize, CancellationToken cancellationToken = default);

    Task<MediaResponseDto?> GETApiMediaByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UploadMediaResponseDto?> POSTApiMediaUploadAsync(MultipartBodyDto request, CancellationToken cancellationToken = default);

    Task<TenantMediaResponseDto?> PUTApiMediaByIdFileAsync(string id, MultipartBodyDto request, CancellationToken cancellationToken = default);
}