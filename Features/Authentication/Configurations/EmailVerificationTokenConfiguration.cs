using Aurum.Api.Features.Authentication.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aurum.Api.Features.Authentication.Configurations;

public sealed class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("email_verification_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(t => t.TokenHash)
            .IsUnique()
            .HasDatabaseName("email_verification_tokens_token_hash_uq");

        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("email_verification_tokens_user_id_idx");

        builder.Property(t => t.ExpiresAtUtc)
            .IsRequired();

        builder.Property(t => t.CreatedAtUtc)
            .IsRequired();

        builder.Ignore(t => t.IsActive);
    }
}
