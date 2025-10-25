using AuthService.Application.Abstractions.Messaging;
using AuthService.Domain.Entities;
using AuthService.Domain.Events.Users;
using AuthService.Infrastructure.Database.Repositories;
using AuthService.Infrastructure.Database.Transactions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Shared;
using UserModel = AuthService.Infrastructure.Database.Entities.User;

namespace AuthService.Application.Commands.Tenant.CreateUserCommands;

public sealed class RegisterUserCommandHandler(
 IRepository<UserModel, User> repository,
 IMapper mapper,
 IPasswordHasher<UserModel> passwordHasher,
 IUnitOfWork unitOfWork) : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        User user = CreateDomainUser(command);

        await PersistUserAsync(user, cancellationToken);

        return Result<Guid>.Success(user.Id);
    }

    private User CreateDomainUser(RegisterUserCommand command)
    {
        User user = mapper.Map<User>(command);
     user.Id = Guid.NewGuid();

        UserModel userModelForHashing = mapper.Map<UserModel>(user);
        user.PasswordHash = passwordHasher.HashPassword(userModelForHashing, command.password);

        user.Raise(new RegisterUserEvent(user.Id));

        return user;
    }

    private async Task PersistUserAsync(User user, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        await repository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
