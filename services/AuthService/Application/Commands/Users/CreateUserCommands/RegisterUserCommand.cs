﻿using AuthService.Application.Abstractions.Messaging;
using AuthService.Domain.Enums;

namespace AuthService.Application.Commands.Tenant.CreateUserCommands;

public sealed record RegisterUserCommand(
    string email,
    string username,
    string password,
    string fullName,
    Guid tenantId,
    UserRole role,
    string? tenantName = null
) : ICommand<Guid>;

