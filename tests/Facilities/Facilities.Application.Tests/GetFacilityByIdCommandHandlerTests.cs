using Facilities.Application.Facilities.Commands.GetFacilityById;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Domain;
using Shared.Persistence;

namespace Facilities.Application.Tests;

public class GetFacilityByIdCommandHandlerTests
{
    private readonly IRepository<Facility, FacilityId> _facilityRepository;
    private readonly GetFacilityByIdCommandHandler _handler;

    public GetFacilityByIdCommandHandlerTests()
    {
        _facilityRepository = Substitute.For<IRepository<Facility, FacilityId>>();
        _handler = new GetFacilityByIdCommandHandler(_facilityRepository);
    }

    [Fact]
    public async Task Handle_GivenValidId_WhenFacilityExists_ShouldReturnResult()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var command = new GetFacilityByIdCommand(facilityId);

        var existingFacility = Facility.Create(
            "Test Facility",
            "Test Address",
            WeeklyOpeningHours.CreateUniform(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        // We change the generated Id to match the requested one
        typeof(Entity<FacilityId>).GetProperty("Id")!.SetValue(existingFacility, new FacilityId(facilityId));

        _facilityRepository.GetByIdAsync(
            Arg.Is<FacilityId>(id => id.Value == facilityId),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(existingFacility));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(facilityId);
        result.Name.Should().Be("Test Facility");
        result.Address.Should().Be("Test Address");
        result.OpeningHours.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_GivenValidId_WhenFacilityDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var command = new GetFacilityByIdCommand(facilityId);

        _facilityRepository.GetByIdAsync(
            Arg.Any<FacilityId>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
