using System.Reflection;
using Aurum.Api.Core.Extensions;
using Aurum.Api.Core.Middleware;
using Aurum.Api.Infrastructure.Logging;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;

var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = envName
});

builder.Configuration.Sources.Clear();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// Load User Secrets only when running locally in Development.
if (envName.Equals("Development", StringComparison.OrdinalIgnoreCase))
{
    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);
}

// Render (and similar platforms) provide the port to bind via $PORT.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(System.Net.IPAddress.Any, int.Parse(port));
});

// ---- Logging -----------------------------------------------------------
builder.ConfigureSerilog();

// ---- Core services -------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// All request validation is funneled through FluentValidation +
// ValidateAndThrowAppExceptionAsync (see Core/Extensions/FluentValidationExtensions.cs),
// so every error response matches the ErrorResponse {error} shape. Disable
// ASP.NET's own automatic 400/ProblemDetails response so it can't produce
// a differently-shaped error for the same kind of failure.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddSwaggerDocumentation();
builder.Services.AddAppApiVersioning();
builder.Services.AddAppCors(builder.Configuration);
builder.Services.AddAppRateLimiting(builder.Configuration);
builder.Services.AddAppDatabase(builder.Configuration);
builder.Services.AddAppHealthChecks(builder.Configuration);
builder.Services.AddAppAuthentication(builder.Configuration);

// Pass builder.Configuration agar SmtpOptions bisa terbaca dari appsettings.json
builder.Services.AddAppSecurityServices(builder.Configuration);

var app = builder.Build();

// ---- Request pipeline ---------------------------------------------------
app.UseAppExceptionHandling();

app.UseSerilogRequestLogging();

// Swagger is only exposed locally, never in production.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Aurum API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors(CorsServiceExtensions.PolicyName);

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

// Exposed for WebApplicationFactory-based integration testing.
public partial class Program { }