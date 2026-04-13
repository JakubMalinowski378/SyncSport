using Facilities.Application.Facilities.Commands.CreateFacility;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Persistence;
using System.Linq.Expressions;

namespace Facilities.Application.Tests;

public class CreateFacilityCommandHandlerTests
{
    private readonly IRepository<Facility, FacilityId> _facilityRepository;
    private readonly CreateFacilityCommandHandler _handler;

    public CreateFacilityCommandHandlerTests()
    {
        _facilityRepository = Substitute.For<IRepository<Facility, FacilityId>>();
        _handler = new CreateFacilityCommandHandler(_facilityRepository);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_WhenFacilityDoesNotExist_ShouldCreateAndReturnFacilityId()
    {
        // Arrange
        var command = new CreateFacilityCommand(
            "Test Facility",
            "Test Address",
            TimeSpan.FromHours(8),
            TimeSpan.FromHours(22)
        );

        _facilityRepository.FirstOrDefaultAsync(
            Arg.Any<Expression<Func<Facility, bool>>>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        await _facilityRepository.Received(1).AddAsync(Arg.Is<Facility>(f =>
            f.Name == command.Name &&
            f.Address == command.Address),
            CancellationToken.None);

        await _facilityRepository.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_WhenFacilityExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateFacilityCommand(
            "Test Facility",
            "Test Address",
            TimeSpan.FromHours(8),
            TimeSpan.FromHours(22)
        );

        var existingFacility = Facility.Create(
            "Test Facility",
            "Other Address",
            WeeklyOpeningHours.CreateUniform(TimeSpan.FromHours(10), TimeSpan.FromHours(20)));

        _facilityRepository.FirstOrDefaultAsync(
            Arg.Any<Expression<Func<Facility, bool>>>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(existingFacility));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A facility with this name already exists.");

        await _facilityRepository.DidNotReceive().AddAsync(Arg.Any<Facility>(), Arg.Any<CancellationToken>());
        await _facilityRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
