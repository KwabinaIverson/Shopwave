using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopwave.Modules.Stores.Domain.Entities;

namespace Shopwave.Modules.Stores.Infrastructure.Persistence.Configurations;

public class SellerReferenceConfiguration : IEntityTypeConfiguration<SellerReference>
{
    public void Configure(EntityTypeBuilder<SellerReference> builder)
    {
        builder.ToTable("seller_references");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.DeletedAt);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}