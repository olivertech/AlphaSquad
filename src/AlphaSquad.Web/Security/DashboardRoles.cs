namespace AlphaSquad.Web.Security;

/// <summary>
/// Representa os perfis entendidos pelo dashboard web.
/// Os valores precisam continuar alinhados com o enum UserRole do backend.
/// </summary>
public static class DashboardRoles
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";

    public static string FromApiValue(int? role) =>
        role switch
        {
            1 => Admin,
            2 => Teacher,
            3 => Student,
            _ => string.Empty
        };
}
