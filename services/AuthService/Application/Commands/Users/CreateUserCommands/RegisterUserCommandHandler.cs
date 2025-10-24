using AuthService.Application.Abstractions.Messaging;
using AuthService.Infrastructure.Database.Repositories;
using AuthService.Domain.Entities;
using Shared;

using UserModel = AuthService.Infrastructure.Database.Entities.User;

namespace AuthService.Application.Commands.Users.CreateUserCommands;

internal sealed class RegisterUserCommandHandler(IRepository<UserModel,User> repository) : ICommandHandler<RegisterUserCommand, Guid>
{
    public Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        // 1. Validamos usuario

        // 2. Creamos usuario

        // 3. Agregamos eventos de dominio (Notificaciones etc)

        // 4. Guardamos en base de datos

        // 5. Retornamos resultado

        return Task.FromResult(Result<Guid>.ValidationFailure(new Shared.Error("NotImplemented", "This command handler is not implemented yet.", ErrorType.Problem)));
    }
}

