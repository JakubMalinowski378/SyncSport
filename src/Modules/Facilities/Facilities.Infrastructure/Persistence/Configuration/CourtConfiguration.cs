using Microsoft.EntityFrameworkCore;
using Facilities.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Facilities.Domain.ValueObjects;

namespace Facilities.Infrastructure.Persistence.Configuration;

public class CourtConfiguration : IEntityTypeConfiguration<Court>
{
    public void Configure(EntityTypeBuilder<Court> builder)
    {
        builder.ToTable("courts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new CourtId(value))
            .ValueGeneratedNever();

        builder.Property<Guid>("FacilityId").IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SurfaceType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();
    }

}
