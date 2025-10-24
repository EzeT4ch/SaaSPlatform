using AuthService.Infrastructure.Database.Entities;

namespace AuthService.Infrastructure.Segurity.Services;

public interface IRefreshTokenService
{
    Task<string> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<(bool IsValid, User? User)> ValidateAsync(string token, CancellationToken cancellationToken = default);
    Task InvalidateAsync(string token, CancellationToken cancellationToken = default);
}