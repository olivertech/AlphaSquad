namespace AlphaSquad.Shared.DTOs.Checkins;

/// <summary>
/// DTO utilizado para registrar um novo check-in do usuário autenticado.
/// </summary>
public record CreateCheckInRequest(
    string? Notes
);

/// <summary>
/// DTO de resposta utilizado nas consultas de check-in.
/// </summary>
public record CheckInResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    UserRole UserRole,
    DateTime CheckedInAt,
    string? Notes
);
