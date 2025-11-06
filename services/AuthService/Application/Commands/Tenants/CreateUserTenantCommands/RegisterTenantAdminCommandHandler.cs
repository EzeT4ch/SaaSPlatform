using AuthService.Application.Abstractions.Messaging;
using AuthService.Domain.Enums;
using AuthService.Domain.Events.Tenants;
using AuthService.Infrastructure.Database.Entities;
using AuthService.Infrastructure.Database.Repositories;
using AuthService.Infrastructure.Database.Transactions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using TenantEntity = AuthService.Infrastructure.Database.Entities.Tenant;
using User = AuthService.Domain.Entities.User;
using UserEntity = AuthService.Infrastructure.Database.Entities.User;

namespace AuthService.Application.Commands.Tenants.CreateUserTenantCommands;

internal sealed class RegisterTenantAdminCommandHandler(
    IRepository<TenantEntity, Domain.Entities.Tenant> tenantRepository,
    UserManager<UserEntity> userRepository,
    RoleManager<Role> roleManager,
    IUnitOfWork unitOfWork,
    ILogger logger
) : ICommandHandler<RegisterTenantAdminCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterTenantAdminCommand command, CancellationToken cToken)
    {
        await unitOfWork.BeginTransactionAsync(cToken);

        Domain.Entities.Tenant tenant = await CreateTenant(command, cToken);
        User user = await CreateUser(command, tenant);

        await EnsureRoleAsync(UserRole.User, tenant.Id,[
            "users.read-self"
        ]);
        await EnsureRoleAsync(UserRole.Admin, tenant.Id, [
            "users.create",
            "users.delete",
            "users.update",
            "tenants.manage",
            "*"
        ]);
        
        await userRepository.AddToRoleAsync(user, UserRole.Admin);

        await unitOfWork.SaveChangesAsync(cToken);

        tenant.Raise(new TenantCreatedEvent(tenant.Id, user.Id));

        await unitOfWork.CommitAsync(cToken);

        return Result.Success(user.Id);
    }

    private async Task<UserEntity> CreateUser(RegisterTenantAdminCommand command, Domain.Entities.Tenant tenant)
    {
        UserEntity user = new()
        {
            Email = command.Email,
            TenantId = tenant.Id,
            UserName = command.Username,
            NormalizedUserName = command.FullName
        };
        
        await userRepository.CreateAsync(user, command.Password);
        
        return user;
    }

    private async Task<Domain.Entities.Tenant> CreateTenant(RegisterTenantAdminCommand command,
        CancellationToken cancellationToken)
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

    private async Task EnsureRoleAsync(
        string roleName,
        Guid tenantId,
        IEnumerable<string> permissions)
    {
        Role? role = await roleManager.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == tenantId);

        if (role is null)
        {
            role = new Role { Name = roleName, NormalizedName = roleName.ToUpperInvariant(), TenantId = tenantId };
            IdentityResult result = await roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                logger.LogError("Error creating role {Role}: {Errors}", roleName,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Created role {Role} for tenant {TenantId}", roleName, tenantId);

        }


        foreach (string perm in permissions)
        {
            if (role.RolePermissions.All(p => p.Permission != perm))
                role.RolePermissions.Add(new RolePermission { Permission = perm, RoleId = role.Id });
        }

        await roleManager.UpdateAsync(role);
        logger.LogInformation("Updated role {Role} with {Count} permissions for tenant {TenantId}", roleName, role.RolePermissions.Count, tenantId);
    }
}