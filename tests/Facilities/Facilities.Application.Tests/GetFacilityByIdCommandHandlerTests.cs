using Facilities.Application.Facilities.Queries.GetFacilityById;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Persistence;
using System.Linq.Expressions;

namespace Facilities.Application.Tests;

public class GetFacilityByIdQueryHandlerTests
{
    private readonly IRepository<Facility, FacilityId> _facilityRepository;
    private readonly GetFacilityByIdQueryHandler _handler;

    public GetFacilityByIdQueryHandlerTests()
    {
        _facilityRepository = Substitute.For<IRepository<Facility, FacilityId>>();
        _handler = new GetFacilityByIdQueryHandler(_facilityRepository);
    }

    [Fact]
    public async Task Handle_GivenValidSlug_WhenFacilityExists_ShouldReturnResult()
    {
        // Arrange
        var slug = "test-facility";
        var command = new GetFacilityByIdQuery(slug);

        var existingFacility = Facility.Create(
            "Test Facility",
            slug,
            "Test Address",
            60,
            CreateUniformOpeningHours(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        _facilityRepository.FirstOrDefaultAsync(
            Arg.Any<Expression<Func<Facility, bool>>>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>?>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(existingFacility));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Facility");
        result.Address.Should().Be("Test Address");
        result.OpeningHours.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_GivenValidSlug_WhenFacilityDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var command = new GetFacilityByIdQuery("nonexistent");

        _facilityRepository.FirstOrDefaultAsync(
            Arg.Any<Expression<Func<Facility, bool>>>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>?>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    private static WeeklyOpeningHours CreateUniformOpeningHours(TimeSpan open, TimeSpan close)
    {
        var dailyHours = Enum.GetValues<DayOfWeek>().Select(day =>
            DailyOpeningHours.Create(day, TimeOnly.FromTimeSpan(open), TimeOnly.FromTimeSpan(close)));
        return WeeklyOpeningHours.Create(dailyHours);
    }
}
