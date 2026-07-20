using System.Reflection;
using Aurum.Api.Core.Extensions;
using Aurum.Api.Core.Middleware;
using Aurum.Api.Infrastructure.Logging;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Serilog;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyModel;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions { Args = args });

builder.Configuration.AddConfiguration(configuration);

var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
builder.Environment.EnvironmentName = envName;

builder.ConfigureSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new Aurum.Api.Core.Serialization.OptionalJsonConverterFactory());
    });
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

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