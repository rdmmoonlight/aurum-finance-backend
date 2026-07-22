using Aurum.Api.Features.Authentication.Entities;
using Aurum.Api.Features.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Infrastructure.Database;

/// <summary>
/// Primary EF Core database context for the application, backed by
/// PostgreSQL. Currently holds the Authentication/Users schema only —
/// add a DbSet here as each new feature introduces its own persistence
/// model.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();

    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations from this assembly as feature modules
        // add IEntityTypeConfiguration<T> classes alongside their entities.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
