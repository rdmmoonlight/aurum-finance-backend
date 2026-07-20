using Serilog;

namespace Aurum.Api.Infrastructure.Logging;

public static class SerilogConfiguration
{
    /// <summary>
    /// Configures Serilog from appsettings (Serilog section) with sensible
    /// console + rolling file sinks. Reading configuration this way keeps
    /// log levels and sinks environment-specific without code changes.
    /// </summary>
    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName();
        });
    }
}
