using AuthService.Application.Abstractions.Messaging;
using AuthService.Domain.Entities;
using AuthService.Domain.Events.Users;
using AuthService.Infrastructure.Database.Repositories;
using AuthService.Infrastructure.Database.Transactions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Shared;
using UserModel = AuthService.Infrastructure.Database.Entities.User;

namespace AuthService.Application.Commands.Users.CreateUserCommands;

public sealed class RegisterUserCommandHandler(
    UserManager<UserModel> repository,
    IMapper mapper,
    IPasswordHasher<UserModel> passwordHasher,
    IUnitOfWork unitOfWork) : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cToken)
    {
        User user = CreateDomainUser(command);

        await PersistUserAsync(user, cToken);

        return Result.Success(user.Id);
    }

    private User CreateDomainUser(RegisterUserCommand command)
    {
        User user = mapper.Map<User>(command);

        UserModel userModelForHashing = mapper.Map<UserModel>(user);
        user.PasswordHash = passwordHasher.HashPassword(userModelForHashing, command.password);
        user.AssignRole(command.role);
        user.Raise(new RegisterUserEvent(user.Id));

        return user;
    }

    private async Task PersistUserAsync(User user, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        UserModel? toModel = mapper.Map<UserModel>(user);

        await repository.CreateAsync(toModel);
        await repository.AddToRoleAsync(toModel, user.Role);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
    }
}