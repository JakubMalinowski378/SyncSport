namespace Facilities.Shared.DTOs;

public record CourtAvailabilityInfo(Guid CourtId, string Name, int ReservationDurationMinutes);
