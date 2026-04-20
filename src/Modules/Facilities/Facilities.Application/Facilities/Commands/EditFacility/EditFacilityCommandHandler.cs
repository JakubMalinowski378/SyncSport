using Users.Shared.Authorization;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Commands.EditFacility;

public sealed class EditFacilityCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository,
    IFacilityAuthorizationService facilityAuthorizationService) : IRequestHandler<EditFacilityCommand>
{
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

        var dailyHours = request.WeeklyHours?.Select(x => x.IsClosed
            ? DailyOpeningHours.CreateClosed(x.DayOfWeek)
            : DailyOpeningHours.Create(x.DayOfWeek, x.OpenTime, x.CloseTime)).ToList();

        var weeklyOpeningHours = dailyHours?.Count == 7 
            ? WeeklyOpeningHours.Create(dailyHours) 
            : WeeklyOpeningHours.CreateUniform(TimeSpan.FromHours(8), TimeSpan.FromHours(22)); // Default fallback

        var customDateHours = request.CustomDateHours?.Select(x => x.IsClosed
            ? DateSpecificOpeningHours.CreateClosed(x.Date)
            : DateSpecificOpeningHours.Create(x.Date, x.OpenTime, x.CloseTime)).ToList();

        facility.Rename(request.Name);
        facility.ChangeAddress(request.Address);        
        facility.ChangeReservationDuration(request.ReservationDuration);        facility.ChangeOpeningHours(weeklyOpeningHours);

        if (customDateHours is not null)
        {
            facility.ChangeCustomDateHours(customDateHours);
        }

        facilityRepository.Update(facility);
        await facilityRepository.SaveChangesAsync(cancellationToken);
    }
}
