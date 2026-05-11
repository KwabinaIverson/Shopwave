using Microsoft.EntityFrameworkCore;
using Shopwave.Modules.Identity.Domain.Entities;
using Shopwave.Modules.Identity.Infrastructure.Persistence.Configurations;
using Shopwave.Modules.Identity.Application.Abstractions;

namespace Shopwave.Modules.Identity.Infrastructure.Persistence;

public class IdentityDbContext : DbContext, IUnitOfWork
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }   
    
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshToken => Set<RefreshToken>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IdentityDbContext).Assembly);
    }

	public override Task<int> SaveChangesAsync(CancellationToken ct = default) => base.SaveChangesAsync(ct);
}