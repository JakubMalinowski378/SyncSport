using FluentAssertions;
using NSubstitute;
using Reservations.Application.Reservations.Commands.CancelReservation;
using Reservations.Domain.Entities;
using Reservations.Domain.ValueObjects;
using Shared.Persistence;
using Users.Shared;

namespace Reservations.Application.Tests;

public class CancelReservationCommandHandlerTests
{
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly ICurrentUser _currentUser;
    private readonly CancelReservationCommandHandler _handler;

    public CancelReservationCommandHandlerTests()
    {
        _reservationRepository = Substitute.For<IRepository<Reservation, Guid>>();
        _currentUser = Substitute.For<ICurrentUser>();
        _handler = new CancelReservationCommandHandler(_reservationRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_WhenReservationBelongsToUserAndIsAtLeast24HoursAway_ShouldCancel()
    {
        var userId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();
        var command = new CancelReservationCommand(reservationId);
        var startTime = DateTimeOffset.UtcNow.AddDays(2);

        var reservation = CreateReservation(reservationId, userId, startTime);

        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(reservation);

        _currentUser.GetState().Returns(new CurrentUserState(userId, "test@test.com", "User", [], true));

        await _handler.Handle(command, CancellationToken.None);

        reservation.Status.Should().Be(Domain.Enums.ReservationStatus.Cancelled);
        await _reservationRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReservationNotFound_ShouldThrowException()
    {
        var reservationId = Guid.NewGuid();
        var command = new CancelReservationCommand(reservationId);

        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((Reservation?)null);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>()
            .WithMessage("Reservation not found.");
    }

    [Fact]
    public async Task Handle_WhenReservationBelongsToDifferentUser_ShouldThrowUnauthorizedAccessException()
    {
        var reservationId = Guid.NewGuid();
        var command = new CancelReservationCommand(reservationId);
        var startTime = DateTimeOffset.UtcNow.AddDays(2);

        var reservation = CreateReservation(reservationId, Guid.NewGuid(), startTime);

        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(reservation);

        _currentUser.GetState().Returns(new CurrentUserState(Guid.NewGuid(), "other@test.com", "User", [], true));

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WhenReservationStartsWithin24Hours_ShouldThrowException()
    {
        var userId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();
        var command = new CancelReservationCommand(reservationId);
        var startTime = DateTimeOffset.UtcNow.AddHours(2);

        var reservation = CreateReservation(reservationId, userId, startTime);

        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(reservation);

        _currentUser.GetState().Returns(new CurrentUserState(userId, "test@test.com", "User", [], true));

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>()
            .WithMessage("Reservation can only be cancelled at least 24 hours before it starts.");
    }

    private static Reservation CreateReservation(Guid id, Guid userId, DateTimeOffset startTime)
    {
        var endTime = startTime.AddHours(1);
        var timeRange = TimeRange.Create(startTime, endTime);
        return Reservation.Create(userId, Guid.NewGuid(), timeRange, 100m);
    }
}
