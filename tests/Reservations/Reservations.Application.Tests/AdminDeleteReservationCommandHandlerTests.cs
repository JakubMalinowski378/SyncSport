using FluentAssertions;
using NSubstitute;
using Reservations.Application.Reservations.Commands.AdminDeleteReservation;
using Reservations.Domain.Entities;
using Shared.Persistence;
using Users.Shared.Authorization;

namespace Reservations.Application.Tests;

public class AdminDeleteReservationCommandHandlerTests
{
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly IFacilityAuthorizationService _facilityAuthorizationService;
    private readonly AdminDeleteReservationCommandHandler _handler;

    public AdminDeleteReservationCommandHandlerTests()
    {
        _reservationRepository = Substitute.For<IRepository<Reservation, Guid>>();
        _facilityAuthorizationService = Substitute.For<IFacilityAuthorizationService>();
        _handler = new AdminDeleteReservationCommandHandler(_reservationRepository, _facilityAuthorizationService);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_WhenReservationExists_ShouldCancelReservation()
    {
        var facilityId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();
        var command = new AdminDeleteReservationCommand(reservationId, facilityId);

        var reservation = Reservation.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Domain.ValueObjects.TimeRange.Create(
                DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8),
                DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(9)),
            100m);

        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(reservation);

        await _handler.Handle(command, CancellationToken.None);

        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(facilityId);
        reservation.Status.Should().Be(Domain.Enums.ReservationStatus.Cancelled);
        await _reservationRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReservationNotFound_ShouldThrowException()
    {
        var command = new AdminDeleteReservationCommand(Guid.NewGuid(), Guid.NewGuid());

        _reservationRepository.GetByIdAsync(command.Id, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Reservation?)null);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>()
            .WithMessage("Reservation not found.");
    }
}
