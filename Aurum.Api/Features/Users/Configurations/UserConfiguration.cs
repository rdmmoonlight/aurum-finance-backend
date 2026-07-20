using Aurum.Api.Features.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aurum.Api.Features.Users.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        // Case-sensitivity note: AuthService always normalizes to
        // lower-case before reading/writing this column, so a plain unique
        // index is sufficient — no need for a citext column or a
        // lower(email) expression index.
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("users_email_uq");

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(120);

        builder.Property(u => u.CreatedAtUtc)
            .IsRequired();

        builder.Property(u => u.UpdatedAtUtc)
            .IsRequired();
    }
}
