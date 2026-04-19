using Facilities.Application.Facilities.Commands.EditFacility;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Persistence;
using System.Linq.Expressions;
using Users.Shared.Authorization;

namespace Facilities.Application.Tests;

public class EditFacilityCommandHandlerTests
{
    private readonly IRepository<Facility, FacilityId> _facilityRepository;
    private readonly IFacilityAuthorizationService _facilityAuthorizationService;
    private readonly EditFacilityCommandHandler _handler;

    public EditFacilityCommandHandlerTests()
    {
        _facilityRepository = Substitute.For<IRepository<Facility, FacilityId>>();
        _facilityAuthorizationService = Substitute.For<IFacilityAuthorizationService>();
        _handler = new EditFacilityCommandHandler(_facilityRepository, _facilityAuthorizationService);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthorized_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new EditFacilityCommand(Guid.NewGuid(), "Updated Facility", "Updated Address", null, null);

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
    public async Task Handle_GivenValidCommand_WhenFacilityExists_ShouldUpdateFacility()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var command = new EditFacilityCommand(facilityId, "Updated Facility", "Updated Address", null, null);

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

        // Let ForNameReturn null so no duplicate checking passes
        _facilityRepository.FirstOrDefaultAsync(
            Arg.Any<Expression<Func<Facility, bool>>>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Is(true),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(null));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(facilityId);
        facility.Name.Should().Be("Updated Facility");
        facility.Address.Should().Be("Updated Address");

        _facilityRepository.Received(1).Update(facility);
        await _facilityRepository.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenFacilityDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var command = new EditFacilityCommand(Guid.NewGuid(), "Updated", "Address", null, null);

        _facilityRepository.GetByIdAsync(
            Arg.Any<FacilityId>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(null));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>();

        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(command.FacilityId);
        _facilityRepository.DidNotReceive().Update(Arg.Any<Facility>());
    }

    [Fact]
    public async Task Handle_WhenAnotherFacilityWithSameNameExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var targetFacilityId = Guid.NewGuid();
        var otherFacilityId = Guid.NewGuid();
        var command = new EditFacilityCommand(targetFacilityId, "Duplicate Name", "Address", null, null);

        var facility = Facility.Create(
            "Old Name",
            "Test Address",
            WeeklyOpeningHours.CreateUniform(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        var existingFacility = Facility.Create(
            "Duplicate Name",
            "Another Address",
            WeeklyOpeningHours.CreateUniform(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        typeof(Shared.Domain.Entity<FacilityId>).GetProperty("Id")!.SetValue(existingFacility, new FacilityId(otherFacilityId));

        _facilityRepository.GetByIdAsync(
            Arg.Is<FacilityId>(id => id.Value == targetFacilityId),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(facility));

        _facilityRepository.FirstOrDefaultAsync(
            Arg.Any<Expression<Func<Facility, bool>>>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Is(true),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(existingFacility));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>();

        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(targetFacilityId);
        _facilityRepository.DidNotReceive().Update(Arg.Any<Facility>());
    }
}