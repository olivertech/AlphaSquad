namespace AlphaSquad.Shared.DTOs.Plans;

/// <summary>
/// Representa um plano da academia em respostas administrativas e de apoio ao profile.
/// </summary>
public record MembershipPlanResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int DurationDays,
    bool IsActive,
    DateTime CreatedAt
);

/// <summary>
/// Payload para criar um novo plano.
/// </summary>
public record CreateMembershipPlanRequest(
    string Name,
    string? Description,
    decimal Price,
    int DurationDays
);

/// <summary>
/// Payload para atualizar um plano existente.
/// </summary>
public record UpdateMembershipPlanRequest(
    string Name,
    string? Description,
    decimal Price,
    int DurationDays,
    bool IsActive
);

/// <summary>
/// Payload para atribuir um plano a um usuario.
/// </summary>
public record AssignMembershipPlanRequest(
    Guid UserId,
    DateTime StartsAt,
    DateTime? EndsAt,
    string? StatusReason
);

/// <summary>
/// Representa um item do historico de planos de um usuario.
/// </summary>
public record UserMembershipHistoryResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid MembershipPlanId,
    string PlanName,
    decimal PlanPrice,
    int DurationDays,
    DateTime StartsAt,
    DateTime? EndsAt,
    bool IsActive,
    string? StatusReason,
    Guid? ChangedByUserId,
    string? ChangedByUserName,
    DateTime CreatedAt
);

/// <summary>
/// Representa um usuario que esta sem plano ativo ha um determinado periodo.
/// </summary>
public record UserWithoutActivePlanResponse(
    Guid UserId,
    string UserName,
    string Email,
    UserRole Role,
    Guid? LastPlanId,
    string? LastPlanName,
    DateTime? LastPlanEndedAt,
    int DaysWithoutActivePlan
);

/// <summary>
/// Payload para registrar administrativamente o pagamento de uma mensalidade.
/// </summary>
public record RecordMembershipPaymentRequest(
    Guid UserId,
    DateTime DueDate,
    DateTime PaidAt,
    decimal AmountPaid,
    string? Notes
);

/// <summary>
/// Resposta administrativa de pagamento de mensalidade registrado.
/// </summary>
public record MembershipPaymentResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid MembershipPlanId,
    string MembershipPlanName,
    decimal AmountPaid,
    DateTime DueDate,
    DateTime PaidAt,
    bool IsPaidOnTime,
    Guid RecordedByUserId,
    string RecordedByUserName,
    string? Notes,
    DateTime CreatedAt
);
