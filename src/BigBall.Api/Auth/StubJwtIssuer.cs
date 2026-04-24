using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BigBall.Api.Auth;

/// <summary>
/// Issues HMAC SHA-256 signed JWTs for the skeleton.
/// STUB — replace with Supabase JWT validation per TechSpec §4.3.
/// The signing key lives in appsettings and is NOT a production secret.
/// </summary>
public sealed class StubJwtIssuer
{
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly TimeSpan _lifetime;

    public StubJwtIssuer(IConfiguration config)
    {
        var keyText = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));
        _issuer = config["Jwt:Issuer"] ?? "bigball-stub";
        _audience = config["Jwt:Audience"] ?? "bigball-web";
        _lifetime = TimeSpan.FromHours(24);
    }

    public string Issue(Guid userId, string email, string displayName)
    {
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("name", displayName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            },
            expires: DateTime.UtcNow.Add(_lifetime),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public SymmetricSecurityKey SigningKey => _key;
    public string Issuer => _issuer;
    public string Audience => _audience;
}
