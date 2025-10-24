using AuthService.Infrastructure.Database.Entities;
using AuthService.Infrastructure.Segurity.Services;

namespace AuthService.Infrastructure.Segurity.Jwt;

public interface IJwtProvider
{
    string Generate(User user, IEnumerable<string>? roles = null);

    Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(
        User user,
        IEnumerable<string>? roles,
        IRefreshTokenService refreshService,
        CancellationToken ct = default);
    
    bool Validate(string token);
}