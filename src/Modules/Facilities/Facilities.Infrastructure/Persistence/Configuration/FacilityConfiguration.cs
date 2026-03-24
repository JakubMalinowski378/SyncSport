using Microsoft.EntityFrameworkCore;
using Facilities.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Facilities.Domain.ValueObjects;

namespace Facilities.Infrastructure.Persistence.Configuration;

public class FacilityConfiguration : IEntityTypeConfiguration<Facility>
{
    public void Configure(EntityTypeBuilder<Facility> builder)
    {
         builder.ToTable("facilities");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(id => id.Value, value => new FacilityId(value))
                .ValueGeneratedNever();

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex(x => x.Name)
                .IsUnique();

            builder.Property(x => x.Address)
                .HasMaxLength(300)
                .IsRequired();

            builder.OwnsOne(x => x.OpeningHours, openingHours =>
            {
                openingHours.Property(x => x.OpenTime)
                    .HasColumnName("open_time")
                    .IsRequired();

                openingHours.Property(x => x.CloseTime)
                    .HasColumnName("close_time")
                    .IsRequired();
            });

            builder.HasMany(x => x.Courts)
                .WithOne()
                .HasForeignKey("FacilityId")
                .OnDelete(DeleteBehavior.Cascade);
    }
}
