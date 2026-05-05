using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using FluentAssertions;

namespace Facilities.Domain.Tests;

public class FacilityTests
{
    private static WeeklyOpeningHours CreateUniformWeeklyHours(TimeSpan open, TimeSpan close)
    {
        var dailyHours = Enum.GetValues<DayOfWeek>().Select(day =>
            DailyOpeningHours.Create(day, TimeOnly.FromTimeSpan(open), TimeOnly.FromTimeSpan(close)));
        return WeeklyOpeningHours.Create(dailyHours);
    }

    private WeeklyOpeningHours GetDefaultOpeningHours()
    {
        return CreateUniformWeeklyHours(TimeSpan.FromHours(8), TimeSpan.FromHours(22));
    }

    [Fact]
    public void Create_GivenValidData_ShouldCreateFacility()
    {
        // Arrange
        var name = "Test Facility";
        var address = "Test Address";
        var openingHours = GetDefaultOpeningHours();

        // Act
        var facility = Facility.Create(name, "test-facility", address, 60, openingHours);

        // Assert
        facility.Should().NotBeNull();
        facility.Id.Should().NotBeNull();
        facility.Name.Should().Be(name);
        facility.Address.Should().Be(address);
        facility.WeeklyOpeningHours.Should().Be(openingHours);
        facility.Courts.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_GivenInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var address = "Test Address";
        var openingHours = GetDefaultOpeningHours();

        // Act
        Action action = () => Facility.Create(invalidName!, "slug", address, 60, openingHours);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Facility name cannot be empty.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_GivenInvalidAddress_ShouldThrowArgumentException(string? invalidAddress)
    {
        // Arrange
        var name = "Test Facility";
        var openingHours = GetDefaultOpeningHours();

        // Act
        Action action = () => Facility.Create(name, "slug", invalidAddress!, 60, openingHours);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Facility address cannot be empty.");
    }

    [Fact]
    public void AddCourt_GivenValidData_ShouldAddCourtToFacility()
    {
        // Arrange
        var facility = Facility.Create("Test Facility", "test-facility", "Address", 60, GetDefaultOpeningHours());
        var courtName = "Court 1";
        var courtSlug = "court-1";
        var surfaceType = "Clay";

        // Act
        var court = facility.AddCourt(courtName, courtSlug, surfaceType);

        // Assert
        court.Should().NotBeNull();
        court.Id.Should().NotBeNull();
        court.Name.Should().Be(courtName);
        court.SurfaceType.Should().Be(surfaceType);

        facility.Courts.Should().ContainSingle();
        facility.Courts.Should().Contain(court);
    }

    [Fact]
    public void AddCourt_GivenExistingCourtName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var facility = Facility.Create("Test Facility", "test-facility", "Address", 60, GetDefaultOpeningHours());
        var courtName = "Court 1";
        facility.AddCourt(courtName, "court-1", "Clay");

        // Act
        Action action = () => facility.AddCourt(courtName, "court-1", "Grass");

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("A court with this slug already exists in this facility.");
    }

    [Fact]
    public void RemoveCourt_GivenExistingCourtId_ShouldRemoveCourtFromFacility()
    {
        // Arrange
        var facility = Facility.Create("Test Facility", "test-facility", "Address", 60, GetDefaultOpeningHours());
        var court = facility.AddCourt("Court 1", "court-1", "Clay");

        // Act
        facility.RemoveCourt(court.Id);

        // Assert
        facility.Courts.Should().BeEmpty();
    }

    [Fact]
    public void RemoveCourt_GivenNonExistingCourtId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var facility = Facility.Create("Test Facility", "test-facility", "Address", 60, GetDefaultOpeningHours());
        var otherCourtId = CourtId.New();

        // Act
        Action action = () => facility.RemoveCourt(otherCourtId);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Court not found for removal.");
    }

    [Fact]
    public void Rename_GivenValidName_ShouldUpdateName()
    {
        // Arrange
        var facility = Facility.Create("Old Name", "old-name", "Address", 60, GetDefaultOpeningHours());
        var newName = "New Name";

        // Act
        facility.Rename(newName);

        // Assert
        facility.Name.Should().Be(newName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Rename_GivenInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var facility = Facility.Create("Name", "name", "Address", 60, GetDefaultOpeningHours());

        // Act
        Action action = () => facility.Rename(invalidName!);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Facility name cannot be empty.");
    }

    [Fact]
    public void ChangeAddress_GivenValidAddress_ShouldUpdateAddress()
    {
        // Arrange
        var facility = Facility.Create("Name", "name", "Old Address", 60, GetDefaultOpeningHours());
        var newAddress = "New Address";

        // Act
        facility.ChangeAddress(newAddress);

        // Assert
        facility.Address.Should().Be(newAddress);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ChangeAddress_GivenInvalidAddress_ShouldThrowArgumentException(string? invalidAddress)
    {
        // Arrange
        var facility = Facility.Create("Name", "name", "Address", 60, GetDefaultOpeningHours());

        // Act
        Action action = () => facility.ChangeAddress(invalidAddress!);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Facility address cannot be empty.");
    }

    [Fact]
    public void ChangeOpeningHours_ShouldUpdateOpeningHours()
    {
        // Arrange
        var facility = Facility.Create("Name", "name", "Address", 60, GetDefaultOpeningHours());
        var newHours = CreateUniformWeeklyHours(TimeSpan.FromHours(9), TimeSpan.FromHours(17));

        // Act
        facility.ChangeOpeningHours(newHours);

        // Assert
        facility.WeeklyOpeningHours.Should().Be(newHours);
    }
}