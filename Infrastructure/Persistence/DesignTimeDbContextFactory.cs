using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HomeGeBotDbContext>
{
    public HomeGeBotDbContext CreateDbContext(string[] args)
    {
        var connectionUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        var connectionString = string.IsNullOrEmpty(connectionUrl)
            ? string.Empty
            : ConnectionStringFactory.GetNpgsqlConnectionString(connectionUrl);

        var optionsBuilder = new DbContextOptionsBuilder<HomeGeBotDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new HomeGeBotDbContext(optionsBuilder.Options, null);
    }
}
