using FluentAssertions;
using NSubstitute;
using Reservations.Application.Reservations.Commands.MarkReservationAsPaidOnSite;
using Reservations.Domain.Entities;
using Reservations.Domain.Enums;
using Reservations.Domain.ValueObjects;
using Shared.Persistence;

namespace Reservations.Application.Tests;

public class MarkReservationAsPaidOnSiteCommandHandlerTests
{
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly MarkReservationAsPaidOnSiteCommandHandler _handler;

    public MarkReservationAsPaidOnSiteCommandHandlerTests()
    {
        _reservationRepository = Substitute.For<IRepository<Reservation, Guid>>();
        _handler = new MarkReservationAsPaidOnSiteCommandHandler(_reservationRepository);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_WhenReservationIsAwaitingOnSitePayment_ShouldMarkAsPaid()
    {
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new MarkReservationAsPaidOnSiteCommand(reservationId);

        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);
        var timeRange = TimeRange.Create(startTime, endTime);
        var reservation = Reservation.Create(userId, Guid.NewGuid(), timeRange, 100m);
        reservation.MarkAsAwaitingOnSitePayment();

        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(reservation);

        await _handler.Handle(command, CancellationToken.None);

        reservation.Status.Should().Be(ReservationStatus.Paid);
        await _reservationRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReservationNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = new MarkReservationAsPaidOnSiteCommand(Guid.NewGuid());

        _reservationRepository.GetByIdAsync(command.Id, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Reservation?)null);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Reservation not found.");
    }

    [Fact]
    public async Task Handle_WhenReservationIsNotAwaitingOnSitePayment_ShouldThrowInvalidOperationException()
    {
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new MarkReservationAsPaidOnSiteCommand(reservationId);

        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);
        var timeRange = TimeRange.Create(startTime, endTime);
        var reservation = Reservation.Create(userId, Guid.NewGuid(), timeRange, 100m);

        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(reservation);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>();
    }
}
