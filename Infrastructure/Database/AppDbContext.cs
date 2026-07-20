using Aurum.Api.Features.Accounting.Accounts.Entities;
using Aurum.Api.Features.Accounting.Periods.Entities;
using Aurum.Api.Features.Journals.Entities;
using Aurum.Api.Features.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Api.Infrastructure.Database;

/// <summary>
/// Primary EF Core database context for the application, backed by PostgreSQL.
/// Entity sets are added here as each feature module introduces its own
/// persistence model (e.g. Users, Journals, Ledger entries).
///
/// IMPORTANT: Accounts and Periods map onto tables that already existed
/// before this API — see each entity's Configuration class. Only Users is a
/// genuinely new table. Migrations generated from this context must not
/// include CreateTable/AlterTable for "accounts" or "periods" — strip those
/// operations out before applying if EF's migration scaffolding adds them
/// (this can happen if a migration is generated before the model "knows"
/// the tables already exist in a target database).
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Authentication / Users feature — genuinely new table.
    public DbSet<User> Users => Set<User>();

    // Accounting feature — map onto pre-existing tables, no schema change.
    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Period> Periods => Set<Period>();

    // Journals feature — maps onto pre-existing journal_entries table, no schema change.
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Declare native Postgres enum types so EF Core's migration
        // scaffolding recognizes them (they already exist in the database —
        // created by the original Drizzle schema, not by this context).
        modelBuilder.HasPostgresEnum<AccountRole>("account_role");
        modelBuilder.HasPostgresEnum<PeriodStatus>("period_status");
        modelBuilder.HasPostgresEnum<JournalKind>("journal_kind");

        // Apply entity configurations from this assembly as feature modules
        // add IEntityTypeConfiguration<T> classes alongside their entities.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
