using Bookify.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(role => role.Id);

        // many to many
        builder
            .HasMany(role => role.Users)
            .WithMany(user => user.Roles);

        builder
            .HasMany(role => role.Permissions)
            .WithMany().UsingEntity<RolePermission>();

        // seeds Registered role as record
        builder.HasData(Role.Registered);
    }
}