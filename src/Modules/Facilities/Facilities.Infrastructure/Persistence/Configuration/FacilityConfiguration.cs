using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

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

        builder.Property(x => x.ReservationDuration)
            .HasColumnName("reservation_duration")
            .IsRequired()
            .HasDefaultValue(60);

        builder.Property(x => x.WeeklyOpeningHours)
            .HasConversion(
                weekly => SerializeWeeklyHours(weekly),
                json => DeserializeWeeklyHours(json))
            .HasColumnName("weekly_opening_hours")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CustomDateHours)
            .HasConversion(
                custom => SerializeCustomDateHours(custom),
                json => DeserializeCustomDateHours(json))
            .HasColumnName("custom_date_hours")
            .HasColumnType("jsonb")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Courts)
            .WithOne()
            .HasForeignKey("FacilityId")
            .OnDelete(DeleteBehavior.Cascade);
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
    private static string SerializeCustomDateHours(IEnumerable<DateSpecificOpeningHours> custom)
    {
        var items = custom
            .OrderBy(x => x.Date)
            .Select(x => new DateSpecificHoursDto(x.Date, x.OpenTime, x.CloseTime, x.IsClosed))
            .ToList();

        return JsonSerializer.Serialize(items);
    }

    private static List<DateSpecificOpeningHours> DeserializeCustomDateHours(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("[")) return [];

        try
        {
            var items = JsonSerializer.Deserialize<List<DateSpecificHoursDto>>(json) ?? [];

            return items.Select(x => x.IsClosed
                ? DateSpecificOpeningHours.CreateClosed(x.Date)
                : DateSpecificOpeningHours.Create(x.Date, x.OpenTime, x.CloseTime))
                .ToList();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private sealed record DailyHoursDto(DayOfWeek DayOfWeek, TimeSpan OpenTime, TimeSpan CloseTime, bool IsClosed);
    private sealed record DateSpecificHoursDto(DateOnly Date, TimeSpan OpenTime, TimeSpan CloseTime, bool IsClosed);
}
