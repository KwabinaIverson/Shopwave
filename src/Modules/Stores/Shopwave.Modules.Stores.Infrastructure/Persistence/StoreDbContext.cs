using Microsoft.EntityFrameworkCore;
using Shopwave.Modules.Stores.Domain.Entities;
using Shopwave.Modules.Stores.Infrastructure.Persistence.Configurations;
using Shopwave.Modules.Stores.Application.Abstractions;

namespace Shopwave.Modules.Stores.Infrastructure.Persistence;

public class StoreDbContext : DbContext, IStoreUnitOfWork
{
    public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options)
    {
    }   
    
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<PayoutMethod> PayoutMethods => Set<PayoutMethod>();
    public DbSet<StoreVerification> StoreVerifications => Set<StoreVerification>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(StoreDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default) => base.SaveChangesAsync(ct);
}