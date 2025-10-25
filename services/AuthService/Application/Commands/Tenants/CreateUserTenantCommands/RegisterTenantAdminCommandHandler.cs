using AuthService.Application.Abstractions.Messaging;
using AuthService.Domain.Entities;
using AuthService.Domain.Enums;
using AuthService.Domain.Events.Tenants;
using AuthService.Infrastructure.Database.Repositories;
using AuthService.Infrastructure.Database.Transactions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Shared;

using TenantEntity = AuthService.Infrastructure.Database.Entities.Tenant;
using UserEntity = AuthService.Infrastructure.Database.Entities.User;

namespace AuthService.Application.Commands.Tenants.CreateUserTenantCommands;

internal sealed class RegisterTenantAdminCommandHandler(
    IRepository<TenantEntity, Domain.Entities.Tenant> tenantRepository,
    IRepository<UserEntity, User> userRepository,
    IPasswordHasher<UserEntity> passwordHasher,
    IUnitOfWork unitOfWork,
    IMapper mapper
    ) : ICommandHandler<RegisterTenantAdminCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterTenantAdminCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        Domain.Entities.Tenant tenant = await CreateTenant(command, cancellationToken);
        User user = await CreateUser(command, tenant, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        tenant.Raise(new TenantCreatedEvent(tenant.Id, user.Id));

        await unitOfWork.CommitAsync(cancellationToken);

        return Result<Guid>.Success(user.Id);
    }

    private async Task<User> CreateUser(RegisterTenantAdminCommand command, Domain.Entities.Tenant tenant, CancellationToken cancellationToken)
    {
        User user = new()
        {
            Email = command.Email,
            TenantId = tenant.Id,
            UserName = command.Username,
            FullName = command.FullName,
            CreatedAt = DateTime.UtcNow
        };

        UserEntity userEntity = mapper.Map<UserEntity>(user);
        user.PasswordHash = passwordHasher.HashPassword(userEntity, command.Password);

        user.AssignRole("Admin");

        await userRepository.AddAsync(user, cancellationToken);
        return user;
    }

    private async Task<Domain.Entities.Tenant> CreateTenant(RegisterTenantAdminCommand command, CancellationToken cancellationToken)
    {
        Domain.Entities.Tenant tenant = new()
        {
            Name = command.TenantName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Status = TenantStatus.Active
        };
        await tenantRepository.AddAsync(tenant, cancellationToken);
        return tenant;
    }
}

