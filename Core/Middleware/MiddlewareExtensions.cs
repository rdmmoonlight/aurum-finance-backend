namespace Aurum.Api.Core.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseAppExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
