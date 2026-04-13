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
    private readonly RemoveCourtCommandHandler _handler;

    public RemoveCourtCommandHandlerTests()
    {
        _facilityRepository = Substitute.For<IRepository<Facility, FacilityId>>();
        _facilityAuthorizationService = Substitute.For<IFacilityAuthorizationService>();
        _handler = new RemoveCourtCommandHandler(_facilityRepository, _facilityAuthorizationService);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_WhenCourtExists_ShouldRemoveCourt()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var facility = Facility.Create(
            "Test Facility",
            "Test Address",
            WeeklyOpeningHours.CreateUniform(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        var court = facility.AddCourt("Court 1", "Clay");
        var courtId = court.Id.Value;

        var command = new RemoveCourtCommand(facilityId, courtId);

        _facilityRepository.GetByIdAsync(
            Arg.Is<FacilityId>(id => id.Value == facilityId),
            Arg.Any<Func<IQueryable<Facility>, IIncludableQueryable<Facility, object>>>(),
            Arg.Is(false),
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
        var facilityId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var command = new RemoveCourtCommand(facilityId, courtId);

        _facilityRepository.GetByIdAsync(
            Arg.Any<FacilityId>(),
            Arg.Any<Func<IQueryable<Facility>, IIncludableQueryable<Facility, object>>>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(null));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>()
            .WithMessage("Facility not found.");

        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(facilityId);
        _facilityRepository.DidNotReceive().Update(Arg.Any<Facility>());
    }

    [Fact]
    public async Task Handle_WhenCourtDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var command = new RemoveCourtCommand(facilityId, courtId);

        var facility = Facility.Create(
            "Test Facility",
            "Test Address",
            WeeklyOpeningHours.CreateUniform(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        _facilityRepository.GetByIdAsync(
            Arg.Is<FacilityId>(id => id.Value == facilityId),
            Arg.Any<Func<IQueryable<Facility>, IIncludableQueryable<Facility, object>>>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(facility));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>()
            .WithMessage("Court not found.");

        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(facilityId);
        _facilityRepository.DidNotReceive().Update(Arg.Any<Facility>());
    }
}