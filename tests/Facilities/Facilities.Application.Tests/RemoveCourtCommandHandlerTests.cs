using Facilities.Application.Facilities.Commands.RemoveCourt;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using Shared.Persistence;
using Users.Shared.Authorization;

namespace Facilities.Application.Tests;

public class RemoveCourtCommandHandlerTests
{
    private readonly IRepository<Facility, FacilityId> _facilityRepository;
    private readonly IFacilityAuthorizationService _facilityAuthorizationService;
    private readonly IRepository<Court, CourtId> _courtRepository;
    private readonly RemoveCourtCommandHandler _handler;

    public RemoveCourtCommandHandlerTests()
    {
        _facilityRepository = Substitute.For<IRepository<Facility, FacilityId>>();
        _facilityAuthorizationService = Substitute.For<IFacilityAuthorizationService>();
        _courtRepository = Substitute.For<IRepository<Court, CourtId>>();
        _handler = new RemoveCourtCommandHandler(_facilityRepository, _courtRepository, _facilityAuthorizationService);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthorized_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var facility = Facility.Create("Test Facility", "test-facility", "Test Address", 60, CreateUniformOpeningHours(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));
        var propertyInfo = typeof(global::Shared.Domain.Entity<FacilityId>).GetProperty("Id");
        propertyInfo?.SetValue(facility, new FacilityId(facilityId));

        var court = facility.AddCourt("Court 1", "court-1", "Clay");
        var command = new RemoveCourtCommand(court.Id.Value);

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
    public async Task Handle_GivenValidCommand_WhenCourtExists_ShouldRemoveCourt()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var facility = Facility.Create("Test Facility", "test-facility", "Test Address", 60, CreateUniformOpeningHours(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));
        var propertyInfo = typeof(global::Shared.Domain.Entity<FacilityId>).GetProperty("Id");
        propertyInfo?.SetValue(facility, new FacilityId(facilityId));

        var court = facility.AddCourt("Court 1", "court-1", "Clay");
        var courtId = court.Id.Value;

        var command = new RemoveCourtCommand(courtId);

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
        facility.Courts.Should().NotContain(c => c.Id.Value == courtId);
        _facilityRepository.Received(1).Update(facility);
        await _facilityRepository.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenFacilityDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var command = new RemoveCourtCommand(Guid.NewGuid());

        _facilityRepository.FirstOrDefaultAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Facility, bool>>>(),
            Arg.Any<Func<IQueryable<Facility>, IIncludableQueryable<Facility, object>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(null));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>().WithMessage("Facility not found.");

        _facilityAuthorizationService.DidNotReceiveWithAnyArgs().AuthorizeFacilityAccess(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_WhenCourtDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var facility = Facility.Create("Test Facility", "test-facility", "Test Address", 60, CreateUniformOpeningHours(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));
        var propertyInfo = typeof(global::Shared.Domain.Entity<FacilityId>).GetProperty("Id");
        propertyInfo?.SetValue(facility, new FacilityId(facilityId));

        var command = new RemoveCourtCommand(Guid.NewGuid()); // Random CourtId that is not in the facility

        _facilityRepository.FirstOrDefaultAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Facility, bool>>>(),
            Arg.Any<Func<IQueryable<Facility>, IIncludableQueryable<Facility, object>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(facility));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>().WithMessage("Court not found.");

        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(facilityId);
        _facilityRepository.DidNotReceive().Update(Arg.Any<Facility>());
    }

    private static WeeklyOpeningHours CreateUniformOpeningHours(TimeSpan open, TimeSpan close)
    {
        var dailyHours = Enum.GetValues<DayOfWeek>().Select(day =>
            DailyOpeningHours.Create(day, TimeOnly.FromTimeSpan(open), TimeOnly.FromTimeSpan(close)));
        return WeeklyOpeningHours.Create(dailyHours);
    }
}
