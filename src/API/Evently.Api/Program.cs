using Evently.Api.Extensions;
using Evently.Api.Midleware;
using Evently.Common.Application;
using Evently.Common.Infrastructure;
using Evently.Common.Presentation.Endpoints;
using Evently.Modules.Events.Infrastructure;
using Evently.Modules.Ticketing.Infrastructure;
using Evently.Modules.Users.Infrastructure;
using MassTransit;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, logCfg) => logCfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(configure =>
{
    configure.Title = "Evently";
    configure.Version = "v1";
    configure.Description = "Evently API";
});

builder.Configuration.AddModuleConfiguration(["events", "users", "ticketing"]);

builder.Services.AddApplication([
    Evently.Modules.Events.Application.AssemblyReference.Assembly,
    Evently.Modules.Users.Application.AssemblyReference.Assembly,
    Evently.Modules.Ticketing.Application.AssemblyReference.Assembly
]);

builder.Services.AddInfrastructure(
    [TicketingModule.ConfigureConsumers],
    builder.Configuration.GetConnectionString("Database")!,
    builder.Configuration.GetConnectionString("Cache")!);

builder.Services.AddEventsModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddTicketingModule(builder.Configuration);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
    app.ApplyMigrations();

    app.MapGet("/", (context) =>
    {
        context.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}

app.MapEndpoints();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.Run();
