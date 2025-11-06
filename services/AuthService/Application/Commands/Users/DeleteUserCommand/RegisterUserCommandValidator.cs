using AuthService.Domain.Entities;
using AuthService.Infrastructure.Database.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UserModel = AuthService.Infrastructure.Database.Entities.User;
using TenantModel = AuthService.Infrastructure.Database.Entities.Tenant;
using TenantEntity = AuthService.Domain.Entities.Tenant;

namespace AuthService.Application.Commands.Users.DeleteUserCommand;

internal sealed class RegisterUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    private readonly IRepository<UserModel, User> _userRepository;
    private readonly IRepository<TenantModel, TenantEntity> _tenantRepository;

    public RegisterUserCommandValidator(IRepository<UserModel, User> userRepository, IRepository<TenantModel, TenantEntity> tenantRepository)
    {
        _userRepository = userRepository;
        _tenantRepository = tenantRepository;

        RuleFor(x => x.email).NotEmpty().WithMessage("UserId es obligatorio.");
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId es obligatorio.");

        RuleFor(x => x)
            .MustAsync(async (command, cancellation) => await FindUserById(command, cancellation))
            .WithMessage("Usuario no encontrado.");

        RuleFor(x => x)
            .MustAsync(async (command, cancellation) => await FindTenantUsers(command, cancellation))
            .WithMessage("No se puede eliminar el único usuario de un tenant.");
    }

    private async Task<bool> FindUserById(
        DeleteUserCommand command,
        CancellationToken cToken
    )
    {
        return await _userRepository
            .AsQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == command.email && x.TenantId == command.TenantId, cToken) != null;
    }

    private async Task<bool> FindTenantUsers(DeleteUserCommand command, CancellationToken cToken)
    {
        TenantEntity? tenant = await _tenantRepository
            .GetByIdAsync(
                command.TenantId, 
                include: query => query.Include(t => t.Users),
                true,
                cancellationToken: cToken
            );

        if (tenant == null || tenant.Users == null || tenant.Users.Count <= 1)
            return false;

        return true;
    }
}