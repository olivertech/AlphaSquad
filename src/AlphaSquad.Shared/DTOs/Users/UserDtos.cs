namespace AlphaSquad.Shared.DTOs.Users;

public record CreateUserRequest(
    string Name, 
    string Email, 
    string Password, 
    UserRole Role
);

public record UpdateUserRequest(
    string Name, 
    UserRole Role, 
    bool IsActive
);

public record UserResponse(
    Guid Id, 
    Guid TenantId, 
    string Name, 
    string Email, 
    UserRole Role, 
    bool IsActive, 
    DateTime CreatedAt
);
