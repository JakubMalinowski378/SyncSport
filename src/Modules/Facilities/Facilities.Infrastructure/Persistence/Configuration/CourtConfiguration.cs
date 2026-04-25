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

        builder.Property(x => x.Slug)
            .HasColumnName("slug")
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(x => x.Slug)
            .IsUnique();

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

        builder.Property(x => x.Images)
            .HasConversion(
                images => JsonSerializer.Serialize(images != null ? images.Select(i => new StoredImageDto(i.Value, i.IsMain)).ToList() : new List<StoredImageDto>(), (JsonSerializerOptions?)null),
                json => DeserializeImages(json)
            )
            .HasColumnName("images")
            .HasColumnType("jsonb")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .Metadata.SetValueComparer(CollectionValueComparers.CreateCollectionComparer<ImageUrl>());
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

    private static List<ImageUrl> DeserializeImages(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]" || json == "null" || !json.TrimStart().StartsWith("["))
            return new List<ImageUrl>();

        try
        {
            var storedImages = JsonSerializer.Deserialize<List<StoredImageDto>>(json, (JsonSerializerOptions?)null);
            if (storedImages is not null && storedImages.Count > 0)
            {
                return storedImages.Select(x => ImageUrl.Create(x.Url, x.IsMain)).ToList();
            }

            var urls = JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>();
            return urls.Select((url, index) => ImageUrl.Create(url, index == 0)).ToList();
        }
        catch (JsonException)
        {
            return new List<ImageUrl>();
        }
    }

    private sealed record StoredImageDto(string Url, bool IsMain);
}
