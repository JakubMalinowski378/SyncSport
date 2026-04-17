using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Commands.CreateFacility;

public sealed class CreateFacilityCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository) : IRequestHandler<CreateFacilityCommand, Guid>
{
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

        var dailyHours = request.WeeklyHours?.Select(x => x.IsClosed
            ? DailyOpeningHours.CreateClosed(x.DayOfWeek)
            : DailyOpeningHours.Create(x.DayOfWeek, x.OpenTime, x.CloseTime)).ToList();

        var weeklyOpeningHours = dailyHours?.Count == 7 
            ? WeeklyOpeningHours.Create(dailyHours) 
            : WeeklyOpeningHours.CreateUniform(TimeSpan.FromHours(8), TimeSpan.FromHours(22)); // Default fallback if not provided properly

        var customDateHours = request.CustomDateHours?.Select(x => x.IsClosed
            ? DateSpecificOpeningHours.CreateClosed(x.Date)
            : DateSpecificOpeningHours.Create(x.Date, x.OpenTime, x.CloseTime)).ToList();

        var facility = Facility.Create(request.Name, request.Address, weeklyOpeningHours, customDateHours);

        await facilityRepository.AddAsync(facility, cancellationToken);
        await facilityRepository.SaveChangesAsync(cancellationToken);

        return facility.Id.Value;
    }
}
