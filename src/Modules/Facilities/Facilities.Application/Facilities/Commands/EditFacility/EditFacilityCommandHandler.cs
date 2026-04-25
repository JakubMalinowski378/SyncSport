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

        var parsedWeeklyHours = request.WeeklyHours.DeserializeJson<List<DailyHoursDto>>(JsonOptions);
        var dailyHours = parsedWeeklyHours?.Select(x => x.IsClosed
            ? DailyOpeningHours.CreateClosed(x.DayOfWeek)
            : DailyOpeningHours.Create(x.DayOfWeek, x.OpenTime, x.CloseTime)).ToList();

        var weeklyOpeningHours = dailyHours?.Count == 7 
            ? WeeklyOpeningHours.Create(dailyHours) 
            : WeeklyOpeningHours.CreateUniform(TimeSpan.FromHours(8), TimeSpan.FromHours(22)); // Default fallback

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

        if (request.Images is not null && request.Images.Any())
        {
            var imageUrls = await imageStorageService.AddRangeAsync(request.Images.ToUploadStreams(), cancellationToken);
            var currentImages = facility.Images.ToList();
            foreach (var img in currentImages)
            {
                facility.RemoveImage(img);
            }

            var selectedMainIndex = request.MainImageIndex ?? 0;
            foreach (var (imageUrl, index) in imageUrls.Select((url, index) => (url, index)))
            {
                facility.AddImage(ImageUrl.Create(imageUrl, index == selectedMainIndex));
            }
        }

        facilityRepository.Update(facility);
        await facilityRepository.SaveChangesAsync(cancellationToken);
    }
}
