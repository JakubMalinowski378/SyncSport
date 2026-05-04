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

        builder.Property(x => x.Slug)
            .HasColumnName("slug")
            .HasMaxLength(220)
            .IsRequired();

        builder.HasIndex(x => x.Slug)
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
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .Metadata.SetValueComparer(CollectionValueComparers.CreateCollectionComparer<DateSpecificOpeningHours>());

        builder.Property(x => x.Images)
            .HasConversion(
                images => JsonSerializer.Serialize(images != null ? images.Select(i => new StoredImageDto(i.Value)).ToList() : new List<StoredImageDto>(), (JsonSerializerOptions?)null),
                json => DeserializeImages(json)
            )
            .HasColumnName("images")
            .HasColumnType("jsonb")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .Metadata.SetValueComparer(CollectionValueComparers.CreateCollectionComparer<ImageUrl>());

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

    private sealed record DailyHoursDto(DayOfWeek DayOfWeek, TimeOnly OpenTime, TimeOnly CloseTime, bool IsClosed);
    private sealed record DateSpecificHoursDto(DateOnly Date, TimeOnly OpenTime, TimeOnly CloseTime, bool IsClosed);

    private static List<ImageUrl> DeserializeImages(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]" || json == "null" || !json.TrimStart().StartsWith("["))
            return new List<ImageUrl>();

        try
        {
            var storedImages = JsonSerializer.Deserialize<List<StoredImageDto>>(json, (JsonSerializerOptions?)null);
            if (storedImages is not null && storedImages.Count > 0)
            {
                return storedImages.Select(x => ImageUrl.Create(x.Url)).ToList();
            }

            var urls = JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>();
            return urls.Select(ImageUrl.Create).ToList();
        }
        catch (JsonException)
        {
            return new List<ImageUrl>();
        }
    }

    private sealed record StoredImageDto(string Url);
}
