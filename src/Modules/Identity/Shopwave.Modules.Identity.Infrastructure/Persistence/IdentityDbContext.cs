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
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }

	public override Task<int> SaveChangesAsync(CancellationToken ct = default) => base.SaveChangesAsync(ct);
}