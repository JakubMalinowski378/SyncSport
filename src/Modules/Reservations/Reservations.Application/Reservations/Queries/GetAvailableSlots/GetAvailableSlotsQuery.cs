using MediatR;
using Reservations.Domain.Enums;

namespace Reservations.Application.Reservations.Queries.GetAvailableSlots;

public record ReservationSlotResponse(Guid Id, DateTime StartTime, DateTime EndTime, ReservationStatus Status);
public record AvailableSlotResponse(DateTime StartTime, DateTime EndTime);
public record GetAvailableSlotsResponse(
    IReadOnlyCollection<ReservationSlotResponse> Reservations,
    IReadOnlyCollection<AvailableSlotResponse> AvailableSlots);

public record GetAvailableSlotsQuery(Guid FacilityId, Guid CourtId, DateTime Date) 
    : IRequest<GetAvailableSlotsResponse>;