using System.Reflection;
using ConfigService.Api.Data;
using ConfigService.Api.Endpoints;
using ConfigService.Api.Infrastructure;
using ConfigService.Api.Options;
using ConfigService.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var isGeneratingOpenApi = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";

builder.Services.AddOpenApi();

if (!isGeneratingOpenApi)
{
    builder.Services.AddOptions<DatabaseOptions>()
        .BindConfiguration(DatabaseOptions.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();

    builder.Services.AddDbContext<ConfigDbContext>((serviceProvider, options) =>
        options.UseNpgsql(serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString));
}

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddValidation();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapApplicationEndpoints();
app.MapConfigurationEndpoints();

app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment() && !isGeneratingOpenApi)
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<ConfigDbContext>().Database.MigrateAsync();
}

app.Run();

public partial class Program;
