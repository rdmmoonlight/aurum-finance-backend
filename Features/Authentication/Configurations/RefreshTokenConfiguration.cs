using Aurum.Api.Features.Authentication.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aurum.Api.Features.Authentication.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(64); // SHA-256 hex is always 64 characters.

        builder.HasIndex(t => t.TokenHash)
            .IsUnique()
            .HasDatabaseName("refresh_tokens_token_hash_uq");

        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("refresh_tokens_user_id_idx");

        builder.Property(t => t.CreatedByIp)
            .HasMaxLength(64);

        builder.Property(t => t.RevokedByIp)
            .HasMaxLength(64);

        builder.Property(t => t.ExpiresAtUtc)
            .IsRequired();

        builder.Property(t => t.CreatedAtUtc)
            .IsRequired();

        builder.Ignore(t => t.IsActive);
    }
}
