using MediatR;

namespace Reservations.Application.Reservations.Queries.GetAvailableSlots;

public record GetAvailableSlotsQuery(Guid FacilityId, DateOnly Date) : IRequest<AvailableSlotsResponse>;

public record AvailableSlotsResponse(DateOnly Date, IReadOnlyCollection<CourtAvailabilityDto> Courts);

public record CourtAvailabilityDto(Guid CourtId, string CourtName, IReadOnlyCollection<TimeSpan> AvailableStartTimes);