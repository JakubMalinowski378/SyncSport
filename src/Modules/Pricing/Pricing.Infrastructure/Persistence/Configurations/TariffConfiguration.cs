using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;

namespace Pricing.Infrastructure.Persistence.Configurations;

internal sealed class TariffConfiguration : IEntityTypeConfiguration<Tariff>
{
    public void Configure(EntityTypeBuilder<Tariff> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(
                tariffId => tariffId.Value,
                value => new TariffId(value));

        builder.Property(t => t.FacilityId)
            .IsRequired();

        builder.Property(t => t.CourtId)
            .IsRequired(false);

        builder.ComplexProperty(t => t.BaseHourlyRate, rateBuilder =>
        {
            rateBuilder.Property(m => m.Amount)
                .HasColumnName("BaseHourlyRate")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.HasMany(t => t.PriceRules)
            .WithOne()
            .HasForeignKey(r => r.TariffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(t => t.CourtRateOverrides, overrideBuilder =>
        {
            overrideBuilder.ToTable("CourtRateOverrides");

            overrideBuilder.WithOwner()
                .HasForeignKey("TariffId");

            overrideBuilder.Property(x => x.CourtId)
                .ValueGeneratedNever()
                .IsRequired();

            overrideBuilder.Property(x => x.HourlyRate)
                .HasConversion(
                    money => money.Amount,
                    amount => new Money(amount))
                .HasColumnName("HourlyRate")
                .HasPrecision(18, 2)
                .IsRequired();

            overrideBuilder.HasKey("TariffId", nameof(CourtRateOverride.CourtId));
        });
    }
}