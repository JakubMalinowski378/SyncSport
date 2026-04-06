using Shared.Domain.Exceptions;

namespace Reservations.Domain.Exceptions;

public class UserAlreadyHasReservationException()
    : DomainException("The user already has a reservation during this time span.");
