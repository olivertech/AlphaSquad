using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using AlphaSquad.Lmt.Application.Contracts.Dtos;
using AlphaSquad.Lmt.Application.Contracts.Interfaces;

namespace AlphaSquad.Lmt.Application.Http.Services;

public sealed class UsersService : IUsersService
{
    private readonly IApiFacade _apiFacade;

    public UsersService(IApiFacade apiFacade)
    {
        _apiFacade = apiFacade ?? throw new ArgumentNullException(nameof(apiFacade));
    }

    public Task DELETEApiUsersByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.DELETEApiUsersByIdAsync(id, cancellationToken);
    }

    public Task<List<UserResponseDto>?> GETApiUsersAsync(CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiUsersAsync(cancellationToken);
    }

    public Task<UserResponseDto?> GETApiUsersByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiFacade.GETApiUsersByIdAsync(id, cancellationToken);
    }

    public Task<UserResponseDto?> POSTApiUsersAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.POSTApiUsersAsync(request, cancellationToken);
    }

    public Task<UserResponseDto?> PUTApiUsersByIdAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        return _apiFacade.PUTApiUsersByIdAsync(id, request, cancellationToken);
    }
}