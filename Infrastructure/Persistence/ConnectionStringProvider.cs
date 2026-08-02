using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

public class ConnectionStringProvider : IConnectionStringProvider
{
    private readonly IConfiguration _configuration;

    public ConnectionStringProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetConnectionString()
    {
        string? connectionUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrEmpty(connectionUrl))
        {
            return ConnectionStringFactory.GetNpgsqlConnectionString(connectionUrl);
        }

        return _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    }
}
