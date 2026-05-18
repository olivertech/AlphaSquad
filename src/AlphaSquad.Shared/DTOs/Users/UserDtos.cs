namespace AlphaSquad.Shared.DTOs.Users;

public record CreateUserRequest(
    string Name, 
    string Email, 
    string Password, 
    UserRole Role,
    string? PhoneNumber,
    string? BirthDate,
    Guid? MembershipPlanId,
    int? MembershipBillingDueDay
);

public record UpdateUserRequest(
    string Name, 
    UserRole Role, 
    bool IsActive,
    string? PhoneNumber,
    string? BirthDate,
    Guid? MembershipPlanId,
    int? MembershipBillingDueDay
);

public record UserResponse(
    Guid Id, 
    Guid TenantId, 
    string Name, 
    string Email, 
    UserRole Role, 
    bool IsActive, 
    DateTime CreatedAt,
    string? PhoneNumber,
    string? BirthDate,
    decimal TotalAccumulatedPoints,
    bool IsGamificationParticipant,
    Guid? ActiveMembershipPlanId,
    string? ActiveMembershipPlanName,
    int? MembershipBillingDueDay,
    bool? IsMembershipInGoodStanding
);

public record UserGamificationHistoryEntryResponse(
    Guid Id,
    GamificationEventType EventType,
    string RuleName,
    decimal PointsApplied,
    string SourceEntity,
    string SourceLabel,
    Guid? SourceEntityId,
    string? SourceTitle,
    DateTime OccurredAt,
    string? Notes
);

public record UserGamificationHistoryResponse(
    Guid UserId,
    string UserName,
    bool IsGamificationParticipant,
    decimal TotalAccumulatedPoints,
    List<UserGamificationHistoryEntryResponse> Entries
);
