using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Infrastructure.Database.Entities;
using AuthService.Infrastructure.Segurity.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Segurity.Jwt;

public class JwtProvider : IJwtProvider
{
    private readonly JwtSettings _settings;

    public JwtProvider(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string Generate(User user, IEnumerable<string>? roles = null)
    {
        SymmetricSecurityKey key = new (Encoding.UTF8.GetBytes(_settings.Key));
        SigningCredentials creds = new (key, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("tenantId", user.TenantId.ToString())
        ];

        if (roles != null)
        {
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        }

        JwtSecurityToken token = new (
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(
        User user,
        IEnumerable<string>? roles,
        IRefreshTokenService refreshService,
        CancellationToken ct = default)
    {
        //TODO: Pendiente implementar feature flag para refresh Token
        string accessToken = Generate(user, roles);
        string refreshToken = await refreshService.CreateAsync(user, ct);
        return (accessToken, refreshToken);
    }

    public bool Validate(string token)
    {
        JwtSecurityTokenHandler tokenHandler = new ();
        byte[] key = Encoding.UTF8.GetBytes(_settings.Key);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _settings.Issuer,
                ValidAudience = _settings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return validatedToken is JwtSecurityToken;
        }
        catch
        {
            return false;
        }
    }
}