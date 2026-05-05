using Facilities.Application.Facilities.Commands.RemoveFacility;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Persistence;
using Users.Shared.Authorization;

namespace Facilities.Application.Tests;

public class RemoveFacilityCommandHandlerTests
{
    private readonly IRepository<Facility, FacilityId> _facilityRepository;
    private readonly IFacilityAuthorizationService _facilityAuthorizationService;
    private readonly RemoveFacilityCommandHandler _handler;

    public RemoveFacilityCommandHandlerTests()
    {
        _facilityRepository = Substitute.For<IRepository<Facility, FacilityId>>();
        _facilityAuthorizationService = Substitute.For<IFacilityAuthorizationService>();
        _handler = new RemoveFacilityCommandHandler(_facilityRepository, _facilityAuthorizationService);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthorized_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new RemoveFacilityCommand(Guid.NewGuid());

        _facilityAuthorizationService
            .When(x => x.AuthorizeFacilityAccess(command.FacilityId))
            .Throw(new UnauthorizedAccessException("You are not authorized to access this facility."));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>();

        var anyId = Arg.Any<FacilityId>();
        await _facilityRepository.DidNotReceiveWithAnyArgs()
            .GetByIdAsync(anyId, Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenValidCommand_WhenFacilityExists_ShouldRemoveFacility()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var command = new RemoveFacilityCommand(facilityId);

        var facility = Facility.Create(
            "Test Facility",
            "test-facility",
            "Test Address",
            60,
            CreateUniformOpeningHours(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        _facilityRepository.GetByIdAsync(
            Arg.Is<FacilityId>(id => id.Value == facilityId),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(facility));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(facilityId);
        _facilityRepository.Received(1).Remove(facility);
        await _facilityRepository.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenFacilityDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var command = new RemoveFacilityCommand(facilityId);

        _facilityRepository.GetByIdAsync(
            Arg.Any<FacilityId>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(null));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>();

        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(facilityId);
        _facilityRepository.DidNotReceive().Remove(Arg.Any<Facility>());
    }

    private static WeeklyOpeningHours CreateUniformOpeningHours(TimeSpan open, TimeSpan close)
    {
        var dailyHours = Enum.GetValues<DayOfWeek>().Select(day =>
            DailyOpeningHours.Create(day, TimeOnly.FromTimeSpan(open), TimeOnly.FromTimeSpan(close)));
        return WeeklyOpeningHours.Create(dailyHours);
    }
}