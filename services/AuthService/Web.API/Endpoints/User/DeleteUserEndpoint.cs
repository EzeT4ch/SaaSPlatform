using AuthService.Application.Abstractions.Messaging;
using AuthService.Application.Commands.Users.DeleteUserCommand;
using AuthService.Web.API.Extensions;
using AuthService.Web.API.Infrastructure;
using Shared;

namespace AuthService.Web.API.Endpoints.User;

public class DeleteUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        
        app.MapDelete("users/delete/{email}/tenant/{tenantId:guid}", async Task<IResult> (
            string email,
            Guid tenantId,
            ICommandHandler<DeleteUserCommand> handler,
            CancellationToken cToken) =>
        {
            DeleteUserCommand command = new(tenantId, email);

            Result result = await handler.Handle(command, cToken);
            
            return result.Match<IResult>(
                () => Results.NotFound("User deleted successfully."),
                CustomResults.Problem);
        })
        .RequireAuthorization("users.delete")
        .WithTags("Users");
    }
}