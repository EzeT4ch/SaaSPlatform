using AuthService.Infrastructure.Database.Entities;

namespace AuthService.Infrastructure.Segurity.Jwt;

public interface IJwtProvider
{
    string Generate(User user, IEnumerable<string>? roles = null);
}