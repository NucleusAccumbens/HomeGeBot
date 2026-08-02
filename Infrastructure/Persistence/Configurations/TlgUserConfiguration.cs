using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TlgUserConfiguration : IEntityTypeConfiguration<TlgUser>
{
    public void Configure(EntityTypeBuilder<TlgUser> builder)
    {
        builder.ToTable("TlgUsers");
        builder.HasKey(u => u.Id);
    }
}
