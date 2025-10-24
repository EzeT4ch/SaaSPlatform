using AuthService.Application.Abstractions.Messaging;
using AuthService.Application.Commands.Users.CreateUserCommands;
using AuthService.Domain.Enums;
using AuthService.Web.API.Extensions;
using AuthService.Web.API.Infrastructure;
using Shared;

namespace AuthService.Web.API.Endpoints.User;

public class CreatedUserEndpoint : IEndpoint
{
    public sealed record Request(string username, string password, Guid tenantId, string email, string fullName, UserRole role);
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/register", async (
            Request req,
            ICommandHandler<RegisterUserCommand, Guid> handler,
            CancellationToken cToken) =>
        {
            var command = new RegisterUserCommand(req.email, req.username, req.password, req.fullName, req.tenantId, req.role);

            Result<Guid> result =  await handler.Handle(command, cToken);

            return result.Match(
                success => Results.Created($"/users/{success}", success),
                failure => CustomResults.Problem(result));
        }).WithTags("User");
    }
}

