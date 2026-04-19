using Facilities.Application.Facilities.Commands.EditCourt;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using Shared.Persistence;
using Users.Shared.Authorization;

namespace Facilities.Application.Tests;

public class EditCourtCommandHandlerTests
{
    private readonly IRepository<Facility, FacilityId> _facilityRepository;
    private readonly IFacilityAuthorizationService _facilityAuthorizationService;
    private readonly EditCourtCommandHandler _handler;

    public EditCourtCommandHandlerTests()
    {
        _facilityRepository = Substitute.For<IRepository<Facility, FacilityId>>();
        _facilityAuthorizationService = Substitute.For<IFacilityAuthorizationService>();
        _handler = new EditCourtCommandHandler(_facilityRepository, _facilityAuthorizationService);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthorized_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new EditCourtCommand(Guid.NewGuid(), Guid.NewGuid(), "Updated Court", true);

        _facilityAuthorizationService
            .When(x => x.AuthorizeFacilityAccess(command.FacilityId))
            .Throw(new UnauthorizedAccessException("You are not authorized to access this facility."));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>();

        var anyId = Arg.Any<FacilityId>();
        await _facilityRepository.DidNotReceiveWithAnyArgs()
            .GetByIdAsync(anyId, Arg.Any<Func<IQueryable<Facility>, IIncludableQueryable<Facility, object>>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenValidCommand_WhenFacilityAndCourtExist_ShouldEditCourt()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var facility = Facility.Create(
            "Test Facility",
            "Test Address",
            WeeklyOpeningHours.CreateUniform(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        var court = facility.AddCourt("Court 1", "Clay");
        var courtId = court.Id.Value;

        var command = new EditCourtCommand(facilityId, courtId, "Updated Court", true);

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
        _facilityRepository.Received(1).Update(facility);
        await _facilityRepository.Received(1).SaveChangesAsync(CancellationToken.None);

        var updatedCourt = facility.Courts.Single(c => c.Id.Value == courtId);
        updatedCourt.Name.Should().Be("Updated Court");
        updatedCourt.IsActive.Should().BeTrue();
    }
}
