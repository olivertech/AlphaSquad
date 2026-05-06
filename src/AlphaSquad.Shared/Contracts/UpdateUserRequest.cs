namespace AlphaSquad.Shared.Contracts;

public class UpdateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
}