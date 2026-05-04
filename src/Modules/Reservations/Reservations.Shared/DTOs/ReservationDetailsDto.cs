namespace Reservations.Shared.DTOs;

public sealed record ReservationDetailsDto(
    Guid ReservationId,
    Guid CourtId,
    string CourtName,
    Guid UserId,
    string Status,
    decimal Price,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
