namespace AlphaSquad.Infrastructure.Auth;

/// <summary>
/// Classe BCryptPasswordHasher é responsável por fornecer funcionalidades de hash e verificação de senhas usando o algoritmo BCrypt.
/// </summary>
public class BCryptPasswordHasher : IBCryptPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}