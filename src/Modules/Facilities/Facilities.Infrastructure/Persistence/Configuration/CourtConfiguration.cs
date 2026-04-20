using Microsoft.EntityFrameworkCore;
using Facilities.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Facilities.Domain.ValueObjects;
using System.Text.Json;

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

        builder.Property<FacilityId>("FacilityId")
            .HasConversion(id => id.Value, value => new FacilityId(value))
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SurfaceType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.OverrideReservationDuration)
            .HasColumnName("override_reservation_duration");

        builder.Property(x => x.OverrideWeeklyOpeningHours)
            .HasConversion(
                weekly => weekly == null ? null : SerializeWeeklyHours(weekly),
                json => string.IsNullOrWhiteSpace(json) ? null : DeserializeWeeklyHours(json))
            .HasColumnName("override_weekly_opening_hours")
            .HasColumnType("jsonb");
    }

    private static string SerializeWeeklyHours(WeeklyOpeningHours weekly)
    {
        var items = weekly.HoursPerDay
            .Values
            .OrderBy(x => x.DayOfWeek)
            .Select(x => new DailyHoursDto(x.DayOfWeek, x.OpenTime, x.CloseTime, x.IsClosed))
            .ToList();

        return JsonSerializer.Serialize(items);
    }

    private static WeeklyOpeningHours DeserializeWeeklyHours(string json)
    {
        var items = JsonSerializer.Deserialize<List<DailyHoursDto>>(json) ?? [];

        var daily = items.Select(x => x.IsClosed
            ? DailyOpeningHours.CreateClosed(x.DayOfWeek)
            : DailyOpeningHours.Create(x.DayOfWeek, x.OpenTime, x.CloseTime));

        return WeeklyOpeningHours.Create(daily);
    }

    private sealed record DailyHoursDto(DayOfWeek DayOfWeek, TimeSpan OpenTime, TimeSpan CloseTime, bool IsClosed);

}
