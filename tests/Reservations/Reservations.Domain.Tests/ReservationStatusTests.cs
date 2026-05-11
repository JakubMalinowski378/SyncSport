using FluentAssertions;
using Reservations.Domain.Enums;

namespace Reservations.Domain.Tests;

public class ReservationStatusTests
{
    [Fact]
    public void ReservationStatus_ShouldHavePendingValue()
    {
        // Assert
        ((int)ReservationStatus.Pending).Should().Be(0);
    }

    [Fact]
    public void ReservationStatus_ShouldHavePaidValue()
    {
        // Assert
        ((int)ReservationStatus.Paid).Should().Be(1);
    }

    [Fact]
    public void ReservationStatus_ShouldHaveCancelledValue()
    {
        // Assert
        ((int)ReservationStatus.Cancelled).Should().Be(2);
    }

    [Fact]
    public void ReservationStatus_ShouldHaveAwaitingOnSitePaymentValue()
    {
        // Assert
        ((int)ReservationStatus.AwaitingOnSitePayment).Should().Be(3);
    }
}
