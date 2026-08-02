namespace Infrastructure.Persistence;

public static class ConnectionStringFactory
{
    public static string GetNpgsqlConnectionString(string connectionUrl)
    {
        try
        {
            var uri = new Uri(connectionUrl);
            var userInfo = uri.UserInfo.Split(':');
            var user = userInfo[0];
            var password = userInfo[1];
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');

            return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse connection URL: {ex.Message}", ex);
        }
    }
}
