using Facilities.Application.Facilities.Commands.EditCourt;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using Shared.Persistence;
using Storage;
using Users.Shared.Authorization;

namespace Facilities.Application.Tests;

public class EditCourtCommandHandlerTests
{
    private readonly IRepository<Facility, FacilityId> _facilityRepository;
    private readonly IFacilityAuthorizationService _facilityAuthorizationService;
    private readonly IImageStorageService _imageStorageService;
    private readonly EditCourtCommandHandler _handler;

    public EditCourtCommandHandlerTests()
    {
        _facilityRepository = Substitute.For<IRepository<Facility, FacilityId>>();
        _facilityAuthorizationService = Substitute.For<IFacilityAuthorizationService>();
        _imageStorageService = Substitute.For<IImageStorageService>();
        _handler = new EditCourtCommandHandler(_facilityRepository, _facilityAuthorizationService, _imageStorageService);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthorized_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var facility = Facility.Create(
            "Test Facility",
            "test-facility",
            "Test Address",
            60,
            CreateUniformOpeningHours(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));
        // Hardcode the ID for the mock
        var propertyInfo = typeof(global::Shared.Domain.Entity<FacilityId>).GetProperty("Id");
        propertyInfo?.SetValue(facility, new FacilityId(facilityId));

        var court = facility.AddCourt("Court 1", "court-1", "Clay");
        var courtId = court.Id.Value;

        var command = new EditCourtCommand
        {
            CourtId = courtId,
            Name = "Updated Court",
            IsActive = true
        };

        _facilityRepository.FirstOrDefaultAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Facility, bool>>>(),
            Arg.Any<Func<IQueryable<Facility>, IIncludableQueryable<Facility, object>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(facility));

        _facilityAuthorizationService
            .When(x => x.AuthorizeFacilityAccess(facilityId))
            .Throw(new UnauthorizedAccessException("You are not authorized to access this facility."));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_GivenValidCommand_WhenFacilityAndCourtExist_ShouldEditCourt()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var facility = Facility.Create(
            "Test Facility",
            "test-facility",
            "Test Address",
            60,
            CreateUniformOpeningHours(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));
        // Hardcode the ID for the mock
        var propertyInfo = typeof(global::Shared.Domain.Entity<FacilityId>).GetProperty("Id");
        propertyInfo?.SetValue(facility, new FacilityId(facilityId));

        var court = facility.AddCourt("Court 1", "court-1", "Clay");
        var courtId = court.Id.Value;

        var command = new EditCourtCommand
        {
            CourtId = courtId,
            Name = "Updated Court",
            IsActive = true
        };

        _facilityRepository.FirstOrDefaultAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Facility, bool>>>(),
            Arg.Any<Func<IQueryable<Facility>, IIncludableQueryable<Facility, object>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(facility));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(facilityId);
        _facilityRepository.Received(1).Update(facility);
        await _facilityRepository.Received(1).SaveChangesAsync(CancellationToken.None);

        var updatedCourt = facility.Courts.Single(c => c.Id.Value == courtId);
        updatedCourt.Name.Should().Be("Updated Court");
        updatedCourt.IsActive.Should().BeTrue();
    }

    private static WeeklyOpeningHours CreateUniformOpeningHours(TimeSpan open, TimeSpan close)
    {
        var dailyHours = Enum.GetValues<DayOfWeek>().Select(day =>
            DailyOpeningHours.Create(day, TimeOnly.FromTimeSpan(open), TimeOnly.FromTimeSpan(close)));
        return WeeklyOpeningHours.Create(dailyHours);
    }
}
