using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GulfInfoTracker.Api.Data;

/// <summary>
/// Used by EF CLI tools (dotnet ef migrations add) at design time.
/// Reads connection string from appsettings.Development.json so no AppHost is needed.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(config.GetConnectionString("GulfInfoTracker"));

        return new AppDbContext(optionsBuilder.Options);
    }
}
