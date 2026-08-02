using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Common;

internal class TestDbContext : DbContext, IBotDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<TlgUser> TlgUsers => Set<TlgUser>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Flat> Flats => Set<Flat>();
    public DbSet<Message> Messages => Set<Message>();
}
