using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;

namespace Pricing.Infrastructure.Persistence.Configurations;

internal sealed class PriceRuleConfiguration : IEntityTypeConfiguration<PriceRule>
{
    public void Configure(EntityTypeBuilder<PriceRule> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(
                priceRuleId => priceRuleId.Value,
                value => new PriceRuleId(value));

        builder.Property(r => r.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.DayOfWeek)
            .HasConversion<string>()
            .IsRequired(false);

        builder.Property(r => r.StartTime)
            .IsRequired(false);

        builder.Property(r => r.EndTime)
            .IsRequired(false);

        builder.Property(r => r.Multiplier)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(r => r.TariffId)
            .HasConversion(
                tariffId => tariffId.Value,
                value => new TariffId(value))
            .IsRequired();
    }
}