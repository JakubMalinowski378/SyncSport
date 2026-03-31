using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Infrastructure.Persistence.Configurations;

internal class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .HasColumnName("Email")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(a => a.Email).IsUnique();

        builder.Property(a => a.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.RefreshToken)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(a => a.PasswordResetToken)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(a => a.RefreshTokenExpiryTime)
            .IsRequired(false);

        builder.Property(a => a.PasswordResetTokenExpiryTime)
            .IsRequired(false);
    }
}
