using System.Threading;
using System.Threading.Tasks;
using AlphaSquad.Lmt.Application.Contracts.Dtos;

namespace AlphaSquad.Lmt.Application.Contracts.Interfaces;

public interface IAuthService
{
    Task<AuthenticatedSessionResponseDto?> GETApiAuthMeAsync(CancellationToken cancellationToken = default);

    Task<string?> POSTApiAuthChangePasswordAsync(ChangePasswordRequestDto request, CancellationToken cancellationToken = default);

    Task<LoginResponseDto?> POSTApiAuthLoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    Task POSTApiAuthLogoutAsync(CancellationToken cancellationToken = default);

    Task<LoginResponseDto?> POSTApiAuthRefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken = default);
}