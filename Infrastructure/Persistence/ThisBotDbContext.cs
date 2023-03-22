using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
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
        optionsBuilder.UseNpgsql("Host=ec2-18-202-8-133.eu-west-1.compute.amazonaws.com;Port=5432;Database=darcqrveiljiu1;Username=tltenmhfpawurb;Password=38f6c2f89abab0084bf7d71cf95b28ba82ae3c6b42abc29bb43fc0757e5a1ebe;Pooling=true;SSL Mode=Require;Trust Server Certificate=True");
    }

    public async Task SaveChangesAsync()
    {
        await base.SaveChangesAsync();
    }
}
