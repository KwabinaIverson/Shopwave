using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopwave.Modules.Stores.Domain.Entities;

namespace Shopwave.Modules.Stores.Infrastructure.Persistence.Configurations;

public class StoreVerificationConfiguration : IEntityTypeConfiguration<StoreVerification>
{
    public void Configure(
        EntityTypeBuilder<StoreVerification> builder)
    {
        builder.ToTable("store_verifications");

        // ─────────────────────────────────────
        // Primary Key
        // ─────────────────────────────────────
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // ─────────────────────────────────────
        // Foreign Key
        // ─────────────────────────────────────
        builder.Property(x => x.StoreId)
            .IsRequired();

        builder.HasIndex(x => x.StoreId);

        // ─────────────────────────────────────
        // Documents
        // ─────────────────────────────────────
        builder.Property(x => x.TaxDocumentUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.RegistrationDocumentUrl)
            .HasMaxLength(500)
            .IsRequired();

        // ─────────────────────────────────────
        // Review Metadata
        // ─────────────────────────────────────
        builder.Property(x => x.SubmittedAt)
            .IsRequired();

        builder.Property(x => x.ReviewedAt);

        builder.Property(x => x.ReviewNote)
            .HasMaxLength(500);

        // ─────────────────────────────────────
        // Status Enum
        // ─────────────────────────────────────
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
    }
}