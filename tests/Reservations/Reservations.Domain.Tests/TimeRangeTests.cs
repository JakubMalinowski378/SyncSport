using FluentAssertions;
using Reservations.Domain.ValueObjects;

namespace Reservations.Domain.Tests;

public class TimeRangeTests
{
    [Fact]
    public void Create_GivenValidStartAndEnd_ShouldCreateTimeRange()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 5, 11, 11, 0, 0, TimeSpan.Zero);

        // Act
        var timeRange = TimeRange.Create(start, end);

        // Assert
        timeRange.Should().NotBeNull();
        timeRange.Start.Should().Be(start);
        timeRange.End.Should().Be(end);
    }

    [Fact]
    public void Create_GivenStartEqualToEnd_ShouldThrowArgumentException()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero);

        // Act
        Action action = () => TimeRange.Create(start, end);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Start time must be before end time.");
    }

    [Fact]
    public void Create_GivenStartAfterEnd_ShouldThrowArgumentException()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 5, 11, 11, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero);

        // Act
        Action action = () => TimeRange.Create(start, end);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Start time must be before end time.");
    }

    [Fact]
    public void Create_GivenDurationLessThan60Minutes_ShouldThrowArgumentException()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 5, 11, 10, 30, 0, TimeSpan.Zero);

        // Act
        Action action = () => TimeRange.Create(start, end);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Reservation duration must be between 60 and 120 minutes.");
    }

    [Fact]
    public void Create_GivenDurationMoreThan120Minutes_ShouldThrowArgumentException()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 5, 11, 13, 0, 0, TimeSpan.Zero);

        // Act
        Action action = () => TimeRange.Create(start, end);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Reservation duration must be between 60 and 120 minutes.");
    }

    [Fact]
    public void Create_GivenExactly60Minutes_ShouldCreateTimeRange()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 5, 11, 11, 0, 0, TimeSpan.Zero);

        // Act
        var timeRange = TimeRange.Create(start, end);

        // Assert
        timeRange.Should().NotBeNull();
        timeRange.Start.Should().Be(start);
        timeRange.End.Should().Be(end);
    }

    [Fact]
    public void Create_GivenExactly120Minutes_ShouldCreateTimeRange()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 5, 11, 12, 0, 0, TimeSpan.Zero);

        // Act
        var timeRange = TimeRange.Create(start, end);

        // Assert
        timeRange.Should().NotBeNull();
        timeRange.Start.Should().Be(start);
        timeRange.End.Should().Be(end);
    }

    [Fact]
    public void Overlaps_GivenOverlappingRanges_ShouldReturnTrue()
    {
        // Arrange
        var range1 = TimeRange.Create(
            new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 11, 12, 0, 0, TimeSpan.Zero));
        var range2 = TimeRange.Create(
            new DateTimeOffset(2026, 5, 11, 11, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 11, 13, 0, 0, TimeSpan.Zero));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_GivenNonOverlappingRanges_ShouldReturnFalse()
    {
        // Arrange
        var range1 = TimeRange.Create(
            new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 11, 11, 0, 0, TimeSpan.Zero));
        var range2 = TimeRange.Create(
            new DateTimeOffset(2026, 5, 11, 11, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 11, 12, 0, 0, TimeSpan.Zero));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_GivenOneRangeInsideAnother_ShouldReturnTrue()
    {
        // Arrange
        var range1 = TimeRange.Create(
            new DateTimeOffset(2026, 5, 11, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 11, 11, 0, 0, TimeSpan.Zero));
        var range2 = TimeRange.Create(
            new DateTimeOffset(2026, 5, 11, 9, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 11, 10, 30, 0, TimeSpan.Zero));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_GivenAdjacentRanges_ShouldReturnFalse()
    {
        // Arrange
        var range1 = TimeRange.Create(
            new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 11, 11, 0, 0, TimeSpan.Zero));
        var range2 = TimeRange.Create(
            new DateTimeOffset(2026, 5, 11, 11, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 11, 12, 0, 0, TimeSpan.Zero));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equality_GivenSameStartAndEnd_ShouldBeEqual()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 5, 11, 11, 0, 0, TimeSpan.Zero);

        var range1 = TimeRange.Create(start, end);
        var range2 = TimeRange.Create(start, end);

        // Act & Assert
        range1.Should().Be(range2);
        range1.GetHashCode().Should().Be(range2.GetHashCode());
    }

    [Fact]
    public void Equality_GivenDifferentStart_ShouldNotBeEqual()
    {
        // Arrange
        var range1 = TimeRange.Create(
            new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 11, 11, 0, 0, TimeSpan.Zero));
        var range2 = TimeRange.Create(
            new DateTimeOffset(2026, 5, 11, 11, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 11, 12, 0, 0, TimeSpan.Zero));

        // Act & Assert
        range1.Should().NotBe(range2);
    }
}
