using FluentAssertions;
using Reservations.Domain.Entities;
using Reservations.Domain.Enums;
using Reservations.Domain.ValueObjects;

namespace Reservations.Domain.Tests;

public class ReservationTests
{
    private static TimeRange CreateValidTimeRange()
    {
        return TimeRange.Create(
            new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 11, 11, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Create_GivenValidData_ShouldCreateReservation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var time = CreateValidTimeRange();
        var price = 100.0m;

        // Act
        var reservation = Reservation.Create(userId, courtId, time, price);

        // Assert
        reservation.Should().NotBeNull();
        reservation.Id.Should().NotBeEmpty();
        reservation.UserId.Should().Be(userId);
        reservation.CourtId.Should().Be(courtId);
        reservation.Time.Should().Be(time);
        reservation.Price.Should().Be(price);
        reservation.Status.Should().Be(ReservationStatus.Pending);
        reservation.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ShouldAddReservationCreatedDomainEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var time = CreateValidTimeRange();
        var price = 100.0m;

        // Act
        var reservation = Reservation.Create(userId, courtId, time, price);

        // Assert
        reservation.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<Reservations.Shared.Events.ReservationCreatedEvent>()
            .Which.ReservationId.Should().Be(reservation.Id);
    }

    [Fact]
    public void MarkAsPaid_GivenPendingReservation_ShouldSetStatusToPaid()
    {
        // Arrange
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidTimeRange(), 100.0m);

        // Act
        reservation.MarkAsPaid();

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Paid);
    }

    [Fact]
    public void MarkAsPaid_GivenCancelledReservation_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidTimeRange(), 100.0m);
        reservation.Cancel();

        // Act
        Action action = () => reservation.MarkAsPaid();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot pay for a cancelled reservation.");
    }

    [Fact]
    public void MarkAsPaid_GivenAlreadyPaidReservation_ShouldSetStatusToPaid()
    {
        // Arrange
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidTimeRange(), 100.0m);
        reservation.MarkAsPaid();

        // Act
        reservation.MarkAsPaid();

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Paid);
    }

    [Fact]
    public void MarkAsPaid_GivenAwaitingOnSitePaymentReservation_ShouldSetStatusToPaid()
    {
        // Arrange
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidTimeRange(), 100.0m);
        reservation.MarkAsAwaitingOnSitePayment();

        // Act
        reservation.MarkAsPaid();

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Paid);
    }

    [Fact]
    public void Cancel_GivenPendingReservation_ShouldSetStatusToCancelled()
    {
        // Arrange
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidTimeRange(), 100.0m);

        // Act
        reservation.Cancel();

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void Cancel_GivenAlreadyCancelledReservation_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidTimeRange(), 100.0m);
        reservation.Cancel();

        // Act
        Action action = () => reservation.Cancel();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Reservation is already cancelled.");
    }

    [Fact]
    public void Cancel_GivenPaidReservation_ShouldSetStatusToCancelled()
    {
        // Arrange
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidTimeRange(), 100.0m);
        reservation.MarkAsPaid();

        // Act
        reservation.Cancel();

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void MarkAsAwaitingOnSitePayment_GivenPendingReservation_ShouldSetStatusToAwaitingOnSitePayment()
    {
        // Arrange
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidTimeRange(), 100.0m);

        // Act
        reservation.MarkAsAwaitingOnSitePayment();

        // Assert
        reservation.Status.Should().Be(ReservationStatus.AwaitingOnSitePayment);
    }

    [Fact]
    public void MarkAsAwaitingOnSitePayment_GivenCancelledReservation_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidTimeRange(), 100.0m);
        reservation.Cancel();

        // Act
        Action action = () => reservation.MarkAsAwaitingOnSitePayment();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot mark a cancelled or already paid reservation for on-site payment.");
    }

    [Fact]
    public void MarkAsAwaitingOnSitePayment_GivenPaidReservation_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidTimeRange(), 100.0m);
        reservation.MarkAsPaid();

        // Act
        Action action = () => reservation.MarkAsAwaitingOnSitePayment();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot mark a cancelled or already paid reservation for on-site payment.");
    }

    [Fact]
    public void MarkAsPaidOnSite_GivenAwaitingOnSitePaymentReservation_ShouldSetStatusToPaid()
    {
        // Arrange
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidTimeRange(), 100.0m);
        reservation.MarkAsAwaitingOnSitePayment();

        // Act
        reservation.MarkAsPaidOnSite();

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Paid);
    }

    [Fact]
    public void MarkAsPaidOnSite_GivenPendingReservation_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidTimeRange(), 100.0m);

        // Act
        Action action = () => reservation.MarkAsPaidOnSite();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only reservations awaiting on-site payment can be marked as paid on site.");
    }

    [Fact]
    public void MarkAsPaidOnSite_GivenCancelledReservation_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidTimeRange(), 100.0m);
        reservation.Cancel();

        // Act
        Action action = () => reservation.MarkAsPaidOnSite();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only reservations awaiting on-site payment can be marked as paid on site.");
    }

    [Fact]
    public void MarkAsPaidOnSite_GivenPaidReservation_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), CreateValidTimeRange(), 100.0m);
        reservation.MarkAsPaid();

        // Act
        Action action = () => reservation.MarkAsPaidOnSite();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only reservations awaiting on-site payment can be marked as paid on site.");
    }
}
