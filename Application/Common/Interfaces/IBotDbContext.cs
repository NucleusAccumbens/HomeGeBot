using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IBotDbContext 
{
    DbSet<TlgUser> TlgUsers { get; }
    DbSet<Client> Clients { get; }
    DbSet<Admin> Admins { get; }      
    DbSet<Flat> Flats { get; }
    DbSet<Message> Messages { get; }
    DbSet<HasPets> HasPets { get; }
    Task SaveChangesAsync();
}
