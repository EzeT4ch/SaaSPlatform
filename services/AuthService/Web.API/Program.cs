using System.Reflection;
using AuthService.Application;
using AuthService.Infrastructure;
using AuthService.Web.API;
using AuthService.Web.API.Extensions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, logConfig)
    => logConfig.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddSwaggerGenWithAuth();

builder.Services.AddOpenApi();

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

WebApplication app = builder.Build();

app.MapEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.UseSwaggerWithUi();
    app.ApplyMigrations();
}

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    
});


app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

app.UseHttpsRedirection();

await app.RunAsync();

namespace Web.Api
{
    public partial class Program;
}
