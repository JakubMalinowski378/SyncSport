using Users.Shared.Authorization;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Facilities.Application.Facilities.Common;
using Shared.Extensions;
using Shared.Persistence;
using Storage;
using System.Text.Json;

namespace Facilities.Application.Facilities.Commands.EditFacility;

public sealed class EditFacilityCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository,
    IFacilityAuthorizationService facilityAuthorizationService,
    IImageStorageService imageStorageService) : IRequestHandler<EditFacilityCommand>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task Handle(EditFacilityCommand request, CancellationToken cancellationToken)
    {
        facilityAuthorizationService.AuthorizeFacilityAccess(request.FacilityId);

        var facility = await facilityRepository.GetByIdAsync(
            new FacilityId(request.FacilityId),
            asNoTracking: false,
            ct: cancellationToken);

        if (facility is null)
        {
            throw new Exception("Facility not found.");
        }

        var existingFacility = await facilityRepository.FirstOrDefaultAsync(
            x => x.Name == request.Name,
            asNoTracking: true,
            ct: cancellationToken);

        if (existingFacility is not null && existingFacility.Id.Value != request.FacilityId)
        {
            throw new InvalidOperationException("A facility with this name already exists.");
        }

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

        var parsedCustomDateHours = request.CustomDateHours.DeserializeJson<List<DateSpecificHoursDto>>(JsonOptions);
        var customDateHours = parsedCustomDateHours?.Select(x => x.IsClosed
            ? DateSpecificOpeningHours.CreateClosed(x.Date)
            : DateSpecificOpeningHours.Create(x.Date, x.OpenTime, x.CloseTime)).ToList();

        facility.Rename(request.Name);
        facility.ChangeAddress(request.Address);
        facility.ChangeReservationDuration(request.ReservationDuration);
        facility.ChangeOpeningHours(weeklyOpeningHours);

        if (customDateHours is not null)
        {
            facility.ChangeCustomDateHours(customDateHours);
        }

        var hasImageChanges =
            (request.RemovedImageUrls is not null && request.RemovedImageUrls.Count > 0) ||
            (request.Images is not null && request.Images.Any());

        if (hasImageChanges)
        {
            var currentImages = facility.Images.ToList();
            var removedUrls = request.RemovedImageUrls is null
                ? []
                : request.RemovedImageUrls.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var removedExistingImages = currentImages
                .Where(x => removedUrls.Contains(x.Value))
                .ToList();

            var imagesToKeep = currentImages
                .Where(x => !removedUrls.Contains(x.Value))
                .ToList();

            foreach (var removedImage in removedExistingImages)
            {
                await imageStorageService.DeleteAsync(removedImage.Value, cancellationToken);
            }

            var uploadedImageUrls = request.Images is not null && request.Images.Any()
                ? (await imageStorageService.AddRangeAsync(request.Images.ToUploadStreams(), cancellationToken)).ToList()
                : [];

            var finalImages = imagesToKeep
                .Concat(uploadedImageUrls.Select(url => ImageUrl.Create(url)))
                .ToList();

            foreach (var existingImage in currentImages)
            {
                facility.RemoveImage(existingImage);
            }

            foreach (var finalImage in finalImages)
            {
                facility.AddImage(ImageUrl.Create(finalImage.Value));
            }
        }

        facilityRepository.Update(facility);
        await facilityRepository.SaveChangesAsync(cancellationToken);
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
                : DailyOpeningHours.Create(day, x.OpenTime, x.CloseTime);
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
        TimeOnly OpenTime,
        TimeOnly CloseTime,
        bool IsClosed);
}
