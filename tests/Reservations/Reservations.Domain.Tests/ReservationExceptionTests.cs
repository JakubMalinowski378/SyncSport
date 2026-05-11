using FluentAssertions;
using Reservations.Domain.Exceptions;
using Shared.Domain.Exceptions;

namespace Reservations.Domain.Tests;

public class ReservationExceptionTests
{
    [Fact]
    public void ReservationInPastException_ShouldHaveCorrectMessage()
    {
        // Act
        var exception = new ReservationInPastException();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be("Reservations cannot be made in the past.");
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void ReservationOverlapException_ShouldHaveCorrectMessage()
    {
        // Act
        var exception = new ReservationOverlapException();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be("The reservation overlaps with an existing reservation for this court.");
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void UserAlreadyHasReservationException_ShouldHaveCorrectMessage()
    {
        // Act
        var exception = new UserAlreadyHasReservationException();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be("The user already has a reservation during this time span.");
        exception.Should().BeAssignableTo<DomainException>();
    }
}
