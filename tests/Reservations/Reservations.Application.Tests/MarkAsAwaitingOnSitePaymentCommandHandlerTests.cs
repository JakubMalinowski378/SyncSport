using FluentAssertions;
using NSubstitute;
using Reservations.Application.Reservations.Commands.MarkAsAwaitingOnSitePayment;
using Reservations.Domain.Entities;
using Reservations.Domain.ValueObjects;
using Shared.Persistence;
using Users.Shared;

namespace Reservations.Application.Tests;

public class MarkAsAwaitingOnSitePaymentCommandHandlerTests
{
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly ICurrentUser _currentUser;
    private readonly MarkAsAwaitingOnSitePaymentCommandHandler _handler;

    public MarkAsAwaitingOnSitePaymentCommandHandlerTests()
    {
        _reservationRepository = Substitute.For<IRepository<Reservation, Guid>>();
        _currentUser = Substitute.For<ICurrentUser>();
        _handler = new MarkAsAwaitingOnSitePaymentCommandHandler(_reservationRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_WhenReservationBelongsToUser_ShouldMarkAsAwaitingOnSitePayment()
    {
        var userId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();

        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);
        var timeRange = TimeRange.Create(startTime, endTime);
        var reservation = Reservation.Create(userId, Guid.NewGuid(), timeRange, 100m);

        var command = new MarkAsAwaitingOnSitePaymentCommand(reservationId);

        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(reservation);

        _currentUser.GetState().Returns(new CurrentUserState(userId, "test@test.com", "User", [], true));

        await _handler.Handle(command, CancellationToken.None);

        reservation.Status.Should().Be(Domain.Enums.ReservationStatus.AwaitingOnSitePayment);
        await _reservationRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReservationNotFound_ShouldThrowException()
    {
        var command = new MarkAsAwaitingOnSitePaymentCommand(Guid.NewGuid());

        _reservationRepository.GetByIdAsync(command.Id, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Reservation?)null);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>()
            .WithMessage("Reservation not found.");
    }

    [Fact]
    public async Task Handle_WhenReservationBelongsToDifferentUser_ShouldThrowUnauthorizedAccessException()
    {
        var reservationId = Guid.NewGuid();

        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);
        var timeRange = TimeRange.Create(startTime, endTime);
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), timeRange, 100m);

        var command = new MarkAsAwaitingOnSitePaymentCommand(reservationId);

        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(reservation);

        _currentUser.GetState().Returns(new CurrentUserState(Guid.NewGuid(), "other@test.com", "User", [], true));

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
