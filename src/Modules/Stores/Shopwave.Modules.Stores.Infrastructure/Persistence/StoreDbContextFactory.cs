using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Shopwave.Modules.Stores.Infrastructure.Persistence;

// This class completely bypasses your Program.cs and tells the EF CLI 
// exactly how to build the DbContext for migrations.
public class StoreDbContextFactory : IDesignTimeDbContextFactory<StoreDbContext>
{
    public StoreDbContext CreateDbContext(string[] args)
    {
        // 1. Tell it where to find your appsettings.json
        // Adjust the path below if your API project is in a different folder!
        var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "API", "Shopwave.API");
        
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // 2. Build the options using the connection string from appsettings
        var builder = new DbContextOptionsBuilder<StoreDbContext>();
        var connectionString = configuration.GetConnectionString("StoresDbConnection");

        builder.UseNpgsql(connectionString);

        return new StoreDbContext(builder.Options);
    }
}