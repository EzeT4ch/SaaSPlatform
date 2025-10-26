namespace AuthService.Infrastructure.Segurity.Authentication;

public interface IUserContext
{
    Guid UserId { get; }
}