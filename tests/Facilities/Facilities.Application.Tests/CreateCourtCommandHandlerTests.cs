using Facilities.Application.Facilities.Commands.CreateCourt;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Persistence;
using Users.Shared.Authorization;

namespace Facilities.Application.Tests;

public class CreateCourtCommandHandlerTests
{
    private readonly IRepository<Facility, FacilityId> _facilityRepository;
    private readonly IFacilityAuthorizationService _facilityAuthorizationService;
    private readonly CreateCourtCommandHandler _handler;

    public CreateCourtCommandHandlerTests()
    {
        _facilityRepository = Substitute.For<IRepository<Facility, FacilityId>>();
        _facilityAuthorizationService = Substitute.For<IFacilityAuthorizationService>();
        _handler = new CreateCourtCommandHandler(_facilityRepository, _facilityAuthorizationService);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthorized_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new CreateCourtCommand(Guid.NewGuid(), "Court 1", "Clay");

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
    public async Task Handle_GivenValidCommand_WhenFacilityExists_ShouldCreateCourtAndReturnId()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var command = new CreateCourtCommand(facilityId, "Court 1", "Clay");

        var facility = Facility.Create(
            "Test Facility",
            "Test Address",
            WeeklyOpeningHours.CreateUniform(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        _facilityRepository.GetByIdAsync(
            Arg.Is<FacilityId>(id => id.Value == facilityId),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(facility));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(facilityId);
        _facilityRepository.Received(1).Update(facility);
        await _facilityRepository.Received(1).SaveChangesAsync(CancellationToken.None);

        facility.Courts.Should().ContainSingle(c => c.Name == "Court 1" && c.SurfaceType == "Clay");
    }

    [Fact]
    public async Task Handle_WhenFacilityDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateCourtCommand(Guid.NewGuid(), "Court 1", "Clay");

        _facilityRepository.GetByIdAsync(
            Arg.Any<FacilityId>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(null));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>();

        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(command.FacilityId);
        _facilityRepository.DidNotReceive().Update(Arg.Any<Facility>());
    }
}