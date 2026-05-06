using Shopwave.Modules.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shopwave.Modules.Identity.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        // ── Primary Key ─────────────────────────────
        builder.HasKey(u => u.Id);

        // ── Email ───────────────────────────────────
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        // ── Name ────────────────────────────────────
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        // ── Security ────────────────────────────────
        builder.Property(u => u.PasswordHash)
            .IsRequired();

        // ── Contact ─────────────────────────────────
        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        // ── Role ────────────────────────────────────
        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>();

        // ── Auditing ────────────────────────────────
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired();

        // ── Soft Delete ─────────────────────────────
        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.DeletedAt)
            .IsRequired(false);

        // ── Global Query Filter (Soft Delete) ───────
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
