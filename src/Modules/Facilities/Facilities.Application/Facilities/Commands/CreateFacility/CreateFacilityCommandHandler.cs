using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using Facilities.Application.Facilities.Common;
using MediatR;
using Shared.Persistence;
using System.Text.Json;
using Storage;
using Shared.Extensions;

namespace Facilities.Application.Facilities.Commands.CreateFacility;

public sealed class CreateFacilityCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository,
    IImageStorageService imageStorageService) : IRequestHandler<CreateFacilityCommand, Guid>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<Guid> Handle(CreateFacilityCommand request, CancellationToken cancellationToken)
    {
        var slug = await UniqueSlugProvider.GenerateAsync(
            request.Name,
            async candidate => await facilityRepository.AnyAsync(f => f.Slug == candidate, cancellationToken));

        var dailyHours = ParseWeeklyHours(request.WeeklyHours);

        var hasCompleteUniqueWeek =
            dailyHours is not null &&
            dailyHours.Count == 7 &&
            dailyHours.Select(x => x.DayOfWeek).Distinct().Count() == 7;

        if (!hasCompleteUniqueWeek)
        {
            throw new ArgumentException("WeeklyHours must contain exactly 7 unique days (Monday-Sunday).", nameof(request.WeeklyHours));
        }

        var weeklyOpeningHours = WeeklyOpeningHours.Create(dailyHours!);

        var reqCustomDateHours = request.CustomDateHours.DeserializeJson<List<DateSpecificHoursDto>>(JsonOptions);

        var customDateHours = reqCustomDateHours?.Select(x => x.IsClosed
            ? DateSpecificOpeningHours.CreateClosed(x.Date)
            : DateSpecificOpeningHours.Create(x.Date, x.OpenTime!.Value, x.CloseTime!.Value)).ToList();

        var facility = Facility.Create(request.Name, slug, request.Address, request.ReservationDuration, weeklyOpeningHours, customDateHours);

        if (request.Images is not null && request.Images.Any())
        {
            var filesToUpload = request.Images.ToUploadStreams();
            var imageUrls = (await imageStorageService.AddRangeAsync(filesToUpload, cancellationToken)).ToList();

            for (int i = 0; i < imageUrls.Count; i++)
            {
                facility.AddImage(ImageUrl.Create(imageUrls[i]));
            }
        }

        await facilityRepository.AddAsync(facility, cancellationToken);
        await facilityRepository.SaveChangesAsync(cancellationToken);

        return facility.Id.Value;
    }

    private static List<DailyOpeningHours> ParseWeeklyHours(string? weeklyHoursJson)
    {
        if (string.IsNullOrWhiteSpace(weeklyHoursJson))
        {
            throw new ArgumentException("WeeklyHours must contain exactly 7 unique days (Monday-Sunday).", nameof(weeklyHoursJson));
        }

        var normalized = weeklyHoursJson.Trim();
        if (normalized.Length >= 2 && normalized[0] == '\'' && normalized[^1] == '\'')
        {
            normalized = normalized[1..^1];
        }

        List<WeeklyHoursPayloadItem>? items = null;

        try
        {
            items = JsonSerializer.Deserialize<List<WeeklyHoursPayloadItem>>(normalized, JsonOptions);
        }
        catch (JsonException)
        {
            try
            {
                var unwrapped = JsonSerializer.Deserialize<string>(normalized, JsonOptions);
                if (!string.IsNullOrWhiteSpace(unwrapped))
                {
                    items = JsonSerializer.Deserialize<List<WeeklyHoursPayloadItem>>(unwrapped, JsonOptions);
                }
            }
            catch (JsonException)
            {
                items = null;
            }
        }

        if (items is null || items.Count == 0)
        {
            throw new ArgumentException("WeeklyHours must contain exactly 7 unique days (Monday-Sunday).", nameof(weeklyHoursJson));
        }

        return items.Select(x =>
        {
            var day = x.DayOfWeek ?? ParseDayOfWeek(x.DayName);
            return x.IsClosed
                ? DailyOpeningHours.CreateClosed(day)
                : DailyOpeningHours.Create(day, x.OpenTime!.Value, x.CloseTime!.Value);
        }).ToList();
    }

    private static DayOfWeek ParseDayOfWeek(string? dayName)
    {
        if (string.IsNullOrWhiteSpace(dayName))
        {
            throw new ArgumentException("Invalid dayName ''.", nameof(dayName));
        }

        if (Enum.TryParse<DayOfWeek>(dayName, ignoreCase: true, out var dayOfWeek))
        {
            return dayOfWeek;
        }

        throw new ArgumentException($"Invalid dayName '{dayName}'.", nameof(dayName));
    }

    private sealed record WeeklyHoursPayloadItem(
        string? DayName,
        DayOfWeek? DayOfWeek,
        TimeOnly? OpenTime,
        TimeOnly? CloseTime,
        bool IsClosed);
}
