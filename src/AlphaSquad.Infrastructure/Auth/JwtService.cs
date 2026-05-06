using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace AlphaSquad.Infrastructure.Auth;

/// <summary>
/// Serviço responsável pela geração e validação de tokens JWT (JSON Web Tokens).
/// Este serviço provê a autenticação stateless da API, permitindo que o servidor valide a identidade
/// do usuário através de claims assinadas digitalmente.
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtOptions _options;

    public JwtService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Gera um Access Token (JWT) com base nas informações do usuário e do tenant.
    /// O token contém claims essenciais para a identificação do usuário e o isolamento do tenant.
    /// </summary>
    /// <param name="user">O usuário autenticado.</param>
    /// <param name="tenant">O tenant ao qual o usuário pertence.</param>
    /// <param name="expiresAt">Data de expiração do token gerada pelo serviço.</param>
    /// <returns>Uma string representando o JWT assinado.</returns>
    public string GenerateAccessToken(AppUser user, Tenant tenant, out DateTime expiresAt)
    {
        expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("tenant_id", tenant.Id.ToString()),
            new("tenant_slug", tenant.Slug),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Gera um Refresh Token utilizando um gerador de números aleatórios criptograficamente forte.
    /// O Refresh Token é uma string opaca que não contém informações, servindo apenas como 
    /// uma chave para recuperar a sessão no banco de dados.
    /// </summary>
    /// <returns>Uma string Base64 representando o token de atualização.</returns>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
