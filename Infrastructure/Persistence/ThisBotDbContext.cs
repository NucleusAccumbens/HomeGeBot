using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Infrastructure.Persistence;

public class ThisBotDbContext : DbContext, IBotDbContext
{
    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;

    public DbSet<TlgUser> TlgUsers => Set<TlgUser>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Admin> Admins => Set<Admin>();

    public DbSet<Flat> Flats => Set<Flat>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<HasPets> HasPets => Set<HasPets>();

    public ThisBotDbContext(DbContextOptions<ThisBotDbContext> options,
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor)
        : base(options)
    {
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
    }

    public ThisBotDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntitySaveChangesInterceptor);
        optionsBuilder.UseNpgsql(GetConnectionString());
    }

    public async Task SaveChangesAsync()
    {
        await base.SaveChangesAsync();
    }

    private static string GetConnectionString()
    {
        string? connectionUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (connectionUrl != null)
        {
            string userPassSide = connectionUrl.Split("@")[0];
            string hostSide = connectionUrl.Split("@")[1];
            string user = userPassSide.Split(":")[1][2..];
            string password = userPassSide.Split(':')[2];
            string host = hostSide.Split("/")[0];
            var database = hostSide.Split("/")[1].Split("?")[0];

            return $"Host={host};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }

        else return String.Empty;
    }
}
