using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopwave.Modules.Stores.Domain.Entities;

namespace Shopwave.Modules.Stores.Infrastructure.Persistence.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("stores");

        // ─────────────────────────────────────
        // Primary Key
        // ─────────────────────────────────────
        builder.HasKey(x => x.Id);

        // ─────────────────────────────────────
        // AggregateRoot base properties
        // ─────────────────────────────────────
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.DeletedAt);

        // optional global filter for soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);

        // ─────────────────────────────────────
        // Scalar properties
        // ─────────────────────────────────────
        builder.Property(x => x.OwnerId)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.Property(x => x.BusinessName)
            .HasMaxLength(100)
            .IsRequired();

        // ─────────────────────────────────────
        // Enums
        // ─────────────────────────────────────
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.VerificationStatus)
            .HasConversion<string>()
            .IsRequired();

        // ─────────────────────────────────────
        // Owned Value Object: Address
        // ─────────────────────────────────────
        builder.OwnsOne(x => x.BusinessAddress, address =>
        {
            address.Property(a => a.StreetAddress1)
                .HasColumnName("street_address_1")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(a => a.StreetAddress2)
                .HasColumnName("street_address_2")
                .HasMaxLength(200);

            address.Property(a => a.City)
                .HasColumnName("city")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.StateProvinceRegion)
                .HasColumnName("state_province_region")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.Country)
                .HasColumnName("country")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(a => a.PostalZipCode)
                .HasColumnName("postal_zip_code")
                .HasMaxLength(50);
        });

        // ─────────────────────────────────────
        // Relationships
        // ─────────────────────────────────────
        // The Anti-Corruption Layer link: Ties the Store strictly to the verified SellerReference
       builder.HasOne<SellerReference>()
    		.WithOne(seller => seller.Store)
    		.HasForeignKey<Store>(x => x.OwnerId)
    		.OnDelete(DeleteBehavior.Restrict);

        // ─────────────────────────────────────
        // Child Collection: PayoutMethods
        // ─────────────────────────────────────
        builder.HasMany(x => x.PayoutMethods)
            .WithOne()
            .HasForeignKey("StoreId")
            .OnDelete(DeleteBehavior.Cascade);

        // ─────────────────────────────────────
        // Child Collection: Verifications
        // ─────────────────────────────────────
        builder.HasMany(x => x.Verifications)
            .WithOne()
            .HasForeignKey(x => x.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(x => x.DomainEvents);
        
        // ─────────────────────────────────────
        // Backing Fields for DDD Encapsulation
        // ─────────────────────────────────────
        builder.Metadata.FindNavigation(nameof(Store.PayoutMethods))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata.FindNavigation(nameof(Store.Verifications))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}