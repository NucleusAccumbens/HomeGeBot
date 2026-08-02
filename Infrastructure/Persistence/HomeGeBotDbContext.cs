using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Persistence;

public class HomeGeBotDbContext : DbContext, IBotDbContext
{
    private readonly AuditableEntitySaveChangesInterceptor? _auditableEntitySaveChangesInterceptor;

    public DbSet<TlgUser> TlgUsers => Set<TlgUser>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Admin> Admins => Set<Admin>();

    public DbSet<Flat> Flats => Set<Flat>();

    public DbSet<Message> Messages => Set<Message>();

    public HomeGeBotDbContext(DbContextOptions<HomeGeBotDbContext> options,
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor)
        : base(options)
    {
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
    }

    public HomeGeBotDbContext()
    {
        _auditableEntitySaveChangesInterceptor = null;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Domain.Common.ChatId>()
            .HaveConversion<ChatIdConverter>();
    }

    private class ChatIdConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Domain.Common.ChatId, long>
    {
        public ChatIdConverter()
            : base(
                v => v.Value,
                v => new Domain.Common.ChatId(v))
        {
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_auditableEntitySaveChangesInterceptor != null)
        {
            optionsBuilder.AddInterceptors(_auditableEntitySaveChangesInterceptor);
        }

        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = ConnectionStringFactory.GetConnectionString(null!);
            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseNpgsql(connectionString);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
