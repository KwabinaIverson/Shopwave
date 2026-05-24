using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopwave.Modules.Stores.Domain.Entities;

namespace Shopwave.Modules.Stores.Infrastructure.Persistence.Configurations;

public class PayoutMethodConfiguration : IEntityTypeConfiguration<PayoutMethod>
{
    public void Configure(EntityTypeBuilder<PayoutMethod> builder)
    {
        builder.ToTable("store_payout_methods");

        // ─────────────────────────────────────
        // Primary Key
        // ─────────────────────────────────────
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        // ─────────────────────────────────────
        // Enum
        // ─────────────────────────────────────
        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        // ─────────────────────────────────────
        // Core Fields
        // ─────────────────────────────────────
        builder.Property(x => x.Provider)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.AccountName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.AccountIdentifier)
            .HasMaxLength(20)
            .IsRequired();

        // ─────────────────────────────────────
        // Verification
        // ─────────────────────────────────────
        builder.Property(x => x.IsVerified)
            .IsRequired();

        builder.Property(x => x.VerifiedAt);

        builder.Property(x => x.VerificationReference)
            .HasMaxLength(100);

        // ─────────────────────────────────────
        // Default flag
        // ─────────────────────────────────────
        builder.Property(x => x.IsDefault)
            .IsRequired();

        // helpful query index
        builder.HasIndex("StoreId", nameof(PayoutMethod.IsDefault));
    }
}