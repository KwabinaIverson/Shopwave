using Shopwave.Modules.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shopwave.Modules.Identity.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // ── Table ─────────────────────────────
        builder.ToTable("refresh_tokens");

        // ── Primary Key ───────────────────────
        builder.HasKey(x => x.Id);

        // ── Properties ────────────────────────
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Token)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ReplacedByToken)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // ── Relationships ─────────────────────
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Indexes ───────────────────────────

        // fast lookup during refresh flow
        builder.HasIndex(x => x.Token)
            .IsUnique();

        // user session queries
        builder.HasIndex(x => x.UserId);

        // cleanup / expiration jobs
        builder.HasIndex(x => x.ExpiresAt);

        // optional: common auth validation query optimization
        builder.HasIndex(x => new { x.UserId, x.IsRevoked, x.ExpiresAt });
        
        builder.HasQueryFilter(x => !x.User.IsDeleted);
    }
}