using AuthService.Domain.Entities;
using AuthService.Infrastructure.Database.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UserModel = AuthService.Infrastructure.Database.Entities.User;

namespace AuthService.Application.Commands.Users.DeleteUserCommand;

internal sealed class RegisterUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    private readonly IRepository<UserModel, User> _repository;

    public RegisterUserCommandValidator(IRepository<UserModel, User> repository)
    {
        _repository = repository;

        RuleFor(x => x.email).NotEmpty().WithMessage("UserId es obligatorio.");
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId es obligatorio.");

        RuleFor(x => x)
            .MustAsync(async (command, cancellation) => await FindUserById(command, cancellation))
            .WithMessage("Usuario no encontrado.");
    }

    private async Task<bool> FindUserById(
        DeleteUserCommand command,
        CancellationToken cToken
    )
    {
        return await _repository
            .AsQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == command.email && x.TenantId == command.TenantId, cToken) != null;
    }
}