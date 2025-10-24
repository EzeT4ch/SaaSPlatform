using AuthService.Application.Abstractions.Messaging;
using AuthService.Application.Commands.Users.CreateUserCommands;
using AuthService.Web.API.Extensions;
using AuthService.Web.API.Infrastructure;
using Shared;

namespace AuthService.Web.API.Endpoints.User;

public class CreatedUserEndpoint : IEndpoint
{
    public sealed record Request(string username, string password, Guid tenant, string email);
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/register", async (
            Request req,
            ICommandHandler<RegisterUserCommand, Guid> handler,
            CancellationToken cToken) =>
        {
            var command = new RegisterUserCommand(req.email, req.username, req.password);

            Result<Guid> result =  await handler.Handle(command, cToken);

            return result.Match(
                success => Results.Created($"/users/{success}", success),
                failure => CustomResults.Problem(result));
        }).WithTags("User");
    }
}

