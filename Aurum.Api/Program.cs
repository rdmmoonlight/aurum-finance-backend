using System.Reflection;
using Aurum.Api.Core.Extensions;
using Aurum.Api.Core.Middleware;
using Aurum.Api.Infrastructure.Logging;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Serilog;

// ---- Fix inotify limit: Inisialisasi Builder Tanpa Default Configuration Sources ----
var options = new WebApplicationOptions { Args = args };
var builder = WebApplication.CreateBuilder(options);

// Bersihkan dan pasang kembali konfigurasi secara manual tanpa file watcher (reloadOnChange: false)
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// ---- Logging -----------------------------------------------------------
builder.ConfigureSerilog();

// ---- Core services -------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Lets Update*Request DTOs use Optional<T> to distinguish "field
        // omitted" from "field explicitly null" — see Core/Shared/Optional.cs.
        options.JsonSerializerOptions.Converters.Add(new Aurum.Api.Core.Serialization.OptionalJsonConverterFactory());
    });
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
builder.Services.AddAppSecurityServices();
builder.Services.AddAccountingServices();
builder.Services.AddJournalsServices();

var app = builder.Build();

// ---- Request pipeline ---------------------------------------------------
app.UseAppExceptionHandling();

app.UseSerilogRequestLogging();

// Swagger is left available in every environment (including production) so the
// API can be inspected post-deploy on Render. Restrict this behind a flag or
// remove the `else` exposure once the API is closer to a public release.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Aurum API v1");
});

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