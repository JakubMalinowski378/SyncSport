using Facilities.Application.Facilities.Commands.CreateFacility;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Persistence;
using Storage;
using System.Linq.Expressions;

namespace Facilities.Application.Tests;

public class CreateFacilityCommandHandlerTests
{
    private readonly IRepository<Facility, FacilityId> _facilityRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly CreateFacilityCommandHandler _handler;

    public CreateFacilityCommandHandlerTests()
    {
        _facilityRepository = Substitute.For<IRepository<Facility, FacilityId>>();
        _imageStorageService = Substitute.For<IImageStorageService>();
        _handler = new CreateFacilityCommandHandler(_facilityRepository, _imageStorageService);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_WhenFacilityDoesNotExist_ShouldCreateAndReturnFacilityId()
    {
        // Arrange
        var command = new CreateFacilityCommand
        {
            Name = "Test Facility",
            Address = "Test Address",
            ReservationDuration = 60,
            WeeklyHours = "[{\"DayOfWeek\":1,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":2,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":3,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":4,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":5,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":6,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":0,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false}]"
        };

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

    private static WeeklyOpeningHours CreateUniformOpeningHours(TimeSpan open, TimeSpan close)
    {
        var dailyHours = Enum.GetValues<DayOfWeek>().Select(day =>
            DailyOpeningHours.Create(day, TimeOnly.FromTimeSpan(open), TimeOnly.FromTimeSpan(close)));
        return WeeklyOpeningHours.Create(dailyHours);
    }
}
