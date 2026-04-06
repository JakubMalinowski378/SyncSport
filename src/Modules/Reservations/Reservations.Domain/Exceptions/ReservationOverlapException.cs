using Shared.Domain.Exceptions;

namespace Reservations.Domain.Exceptions;

public class ReservationOverlapException()
    : DomainException("The reservation overlaps with an existing reservation for this court.");
