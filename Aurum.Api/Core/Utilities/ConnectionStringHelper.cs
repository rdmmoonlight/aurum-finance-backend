namespace Aurum.Api.Core.Utilities;

/// <summary>
/// Hosting platforms such as Render and database providers such as Neon
/// typically expose PostgreSQL connection details as a single
/// "postgres://user:password@host:port/database?sslmode=require" URL.
/// Npgsql expects a keyword=value connection string, so this helper
/// converts one format into the other when needed.
/// </summary>
public static class ConnectionStringHelper
{
    public static string NormalizeToNpgsqlFormat(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        var isUrlFormat =
            connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);

        if (!isUrlFormat)
        {
            // Already in Npgsql keyword=value format — use as-is.
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.TrimStart('/');
        var port = uri.Port > 0 ? uri.Port : 5432;

        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var sslMode = query["sslmode"] ?? "Require";
        var trustServerCertificate = query["sslmode"] is "require" or "prefer" ? "true" : "false";

        return
            $"Host={uri.Host};" +
            $"Port={port};" +
            $"Database={database};" +
            $"Username={username};" +
            $"Password={password};" +
            $"SSL Mode={sslMode};" +
            $"Trust Server Certificate={trustServerCertificate};";
    }
}
