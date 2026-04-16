using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;
using Shared.Domain.Enums;

namespace Users.Infrastructure.Persistence.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasOne<Account>()
            .WithOne()
            .HasForeignKey<User>(u => u.Id)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(u => u.Email, emailBuilder =>
        {
            emailBuilder.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(255)
                .IsRequired();
                
            emailBuilder.HasIndex(e => e.Value).IsUnique();
        });

        builder.ComplexProperty(u => u.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();

            nameBuilder.Property(n => n.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .IsRequired();

        builder.Property(u => u.ManagedFacilityIds)
            .HasField("_managedFacilityIds")
            .HasColumnName("ManagedFacilityIds");

        builder.Ignore(u => u.DomainEvents);
    }
}
