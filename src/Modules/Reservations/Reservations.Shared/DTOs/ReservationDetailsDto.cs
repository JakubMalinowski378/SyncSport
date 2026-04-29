namespace Reservations.Shared.DTOs;

public sealed record ReservationDetailsDto(
    Guid ReservationId,
    Guid CourtId,
    Guid UserId,
    string Status,
    decimal Price,
    DateTime StartTime,
    DateTime EndTime);
