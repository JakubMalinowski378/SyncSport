using Shared.Domain.Exceptions;

namespace Reservations.Domain.Exceptions;

public class ReservationInPastException()
    : DomainException("Reservations cannot be made in the past.");
