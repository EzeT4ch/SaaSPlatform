using AuthService.Application.Abstractions.Messaging;
using AuthService.Application.Commands.Tenants.CreateUserTenantCommands;
using AuthService.Web.API.Extensions;
using AuthService.Web.API.Infrastructure;
using Shared;

namespace AuthService.Web.API.Endpoints.Tenant
{
    public class CreateTenantEndpoint : IEndpoint
    {
        private sealed record Request(
            string TenantName,
            string Email,
            string Password,
            string FullName,
            string Username);

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("tenants/register", async (
                Request req,
                ICommandHandler<RegisterTenantAdminCommand, Guid> handler,
                CancellationToken cToken) =>
            {
                RegisterTenantAdminCommand command = new(req.TenantName, req.Email, req.Password, req.FullName,
                    req.Username);

                Result<Guid> result = await handler.Handle(command, cToken);

                return result.Match(
                    success => Results.Created($"/tenants/{success}", success),
                    failure => CustomResults.Problem(result));
            }).WithTags("Tenants");
        }
    }
}