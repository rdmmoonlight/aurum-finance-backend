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

// 1. Tentukan nama Environment secara eksplisit dari sistem operasi
var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

// 2. Gunakan CreateBuilder normal, namun setel nama Environment secara manual
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = envName
});

// 3. Bersihkan total konfigurasi default bawaan .NET yang menyalakan inotify secara diam-diam
builder.Configuration.Sources.Clear();

// 4. Daftarkan ulang konfigurasi secara manual dengan memaksa "reloadOnChange: false" (Bebas dari eror inotify Render!)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// 5. Masukkan User Secrets hanya jika berjalan di lingkungan laptop lokal (Development)
if (envName.Equals("Development", StringComparison.OrdinalIgnoreCase))
{
    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);
}

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
builder.Services.AddReportsServices();
builder.Services.AddLedgerServices();
builder.Services.AddSecurityFeatureServices();
builder.Services.AddBankAccountServices();

var app = builder.Build();

// ---- Request pipeline ---------------------------------------------------
app.UseAppExceptionHandling();

app.UseSerilogRequestLogging();

// Kunci Keamanan: Dokumentasi arsitektur API hanya diizinkan aktif di laptop lokal (Development)
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

app.UseAuditLogging();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

// Exposed for WebApplicationFactory-based integration testing.
public partial class Program { }