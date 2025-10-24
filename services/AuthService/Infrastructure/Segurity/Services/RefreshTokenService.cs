using System.Security.Cryptography;
using AuthService.Infrastructure.Database;
using AuthService.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Segurity.Services;

public class RefreshTokenService(DbContainer context) : IRefreshTokenService
{
    public async Task<string> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
        string token = Convert.ToBase64String(randomBytes);

        RefreshToken refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(cancellationToken);

        return token;
    }

    public async Task<(bool IsValid, User? User)> ValidateAsync(string token, CancellationToken cancellationToken = default)
    {
        RefreshToken? stored = await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (stored is null || stored.IsRevoked || stored.IsUsed || stored.ExpiresAt < DateTime.UtcNow)
        {
            return (false, null);
        }

        stored.IsUsed = true;
        context.RefreshTokens.Update(stored);
        await context.SaveChangesAsync(cancellationToken);

        // TODO : Modificar el mappeo
        User domainUser = new ()
        {
            Id = stored.User.Id,
            Email = stored.User.Email,
            UserName = stored.User.UserName,
            TenantId = stored.User.TenantId,
        };

        return (true, domainUser);
    }

    public async Task InvalidateAsync(string token, CancellationToken cancellationToken = default)
    {
        RefreshToken? stored = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (stored is null)
        {
            return;
        }

        stored.IsRevoked = true;
        context.RefreshTokens.Update(stored);
        await context.SaveChangesAsync(cancellationToken);
    }
}