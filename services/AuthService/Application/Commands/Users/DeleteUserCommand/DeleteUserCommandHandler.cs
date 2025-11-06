using AuthService.Application.Abstractions.Messaging;
using AuthService.Domain.Events.Users;
using AuthService.Infrastructure.Database.Repositories;
using AuthService.Infrastructure.Database.Transactions;
using AutoMapper;
using Shared;

using User = AuthService.Domain.Entities.User;
using UserEntity = AuthService.Infrastructure.Database.Entities.User;

namespace AuthService.Application.Commands.Users.DeleteUserCommand;

internal sealed class DeleteUserCommandHandler(
    IRepository<UserEntity, User> repository,
    IUnitOfWork unitOfWork,
    IMapper mapper
    ) : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result> Handle(DeleteUserCommand command, CancellationToken cToken)
    {
        User user = mapper.Map<User>(
            repository
                .AsQueryable()
                .First(x => x.Email == command.email && x.TenantId == command.TenantId && x.IsActive)
            );

        
        user.Deactivate();
        user.Raise(new DeleteUserEvent(user.Id));
        
        await unitOfWork.BeginTransactionAsync(cToken);

        repository.Update(user, cToken);
        
        await unitOfWork.SaveChangesAsync(cToken);
        await unitOfWork.CommitAsync(cToken);
        
        return Result.Success();
    }
}