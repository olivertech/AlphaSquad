using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface IUsersService
{
    Task DELETEApiUsersByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<UserResponseDto>?> GETApiUsersAsync(CancellationToken cancellationToken = default);

    Task<UserResponseDto?> GETApiUsersByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserResponseDto?> POSTApiUsersAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default);

    Task<UserResponseDto?> PUTApiUsersByIdAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default);
}