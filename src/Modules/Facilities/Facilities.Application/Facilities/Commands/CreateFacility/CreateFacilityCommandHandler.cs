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
        var existingFacility = await facilityRepository.FirstOrDefaultAsync(
            x => x.Name == request.Name,
            asNoTracking: true,
            ct: cancellationToken);

        if (existingFacility is not null)
        {
            throw new InvalidOperationException("A facility with this name already exists.");
        }

        var reqWeeklyHours = request.WeeklyHours.DeserializeJson<List<DailyHoursDto>>(JsonOptions);

        var dailyHours = reqWeeklyHours?.Select(x => x.IsClosed
            ? DailyOpeningHours.CreateClosed(x.DayOfWeek)
            : DailyOpeningHours.Create(x.DayOfWeek, x.OpenTime, x.CloseTime)).ToList();

        var weeklyOpeningHours = dailyHours?.Count == 7 
            ? WeeklyOpeningHours.Create(dailyHours) 
            : WeeklyOpeningHours.CreateUniform(TimeSpan.FromHours(8), TimeSpan.FromHours(22)); // Default fallback if not provided properly

        var reqCustomDateHours = request.CustomDateHours.DeserializeJson<List<DateSpecificHoursDto>>(JsonOptions);

        var customDateHours = reqCustomDateHours?.Select(x => x.IsClosed
            ? DateSpecificOpeningHours.CreateClosed(x.Date)
            : DateSpecificOpeningHours.Create(x.Date, x.OpenTime, x.CloseTime)).ToList();

        var facility = Facility.Create(request.Name, request.Address, request.ReservationDuration, weeklyOpeningHours, customDateHours);

        if (request.Images is not null && request.Images.Any())
        {
            var filesToUpload = request.Images.ToUploadStreams();
            var imageUrls = (await imageStorageService.AddRangeAsync(filesToUpload, cancellationToken)).ToList();
            var selectedMainIndex = request.MainImageIndex ?? 0;

            for (int i = 0; i < imageUrls.Count; i++)
            {
                facility.AddImage(ImageUrl.Create(imageUrls[i], i == selectedMainIndex));
            }
        }

        await facilityRepository.AddAsync(facility, cancellationToken);
        await facilityRepository.SaveChangesAsync(cancellationToken);

        return facility.Id.Value;
    }
}
