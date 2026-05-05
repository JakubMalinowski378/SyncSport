using Facilities.Application.Facilities.Queries.GetFacilityCourts;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Pagination;
using Shared.Persistence;
using System.Linq.Expressions;

namespace Facilities.Application.Tests;

public class GetFacilityCourtsQueryHandlerTests
{
    private readonly IRepository<Facility, FacilityId> _facilityRepository;
    private readonly IRepository<Court, CourtId> _courtRepository;
    private readonly GetFacilityCourtsQueryHandler _handler;

    public GetFacilityCourtsQueryHandlerTests()
    {
        _facilityRepository = Substitute.For<IRepository<Facility, FacilityId>>();
        _courtRepository = Substitute.For<IRepository<Court, CourtId>>();
        _handler = new GetFacilityCourtsQueryHandler(_facilityRepository, _courtRepository);
    }

    [Fact]
    public async Task Handle_GivenValidQuery_WhenFacilityExists_ShouldReturnPagedResultOfCourts()
    {
        // Arrange
        var facilitySlug = "test-facility";
        var query = new GetFacilityCourtsQuery { FacilitySlug = facilitySlug, PageNumber = 1, PageSize = 10 };

        var facility = Facility.Create(
            "Test Facility",
            facilitySlug,
            "Address",
            60,
            CreateUniformOpeningHours(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        var court1 = facility.AddCourt("Court 1", "court-1", "Clay");
        var court2 = facility.AddCourt("Court 2", "court-2", "Grass");

        _facilityRepository.FirstOrDefaultAsync(
            Arg.Any<Expression<Func<Facility, bool>>>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>?>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(facility));

        _courtRepository.GetPagedAsync(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<Expression<Func<Court, bool>>?>(),
            include: Arg.Any<Func<IQueryable<Court>, IQueryable<Court>>?>(),
            orderBy: Arg.Any<Func<IQueryable<Court>, IOrderedQueryable<Court>>?>(),
            asNoTracking: Arg.Any<bool>(),
            ct: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(((IEnumerable<Court>)new[] { court1, court2 }, 2)));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(c => c.Name == "Court 1");
        result.Items.Should().Contain(c => c.Name == "Court 2");
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WhenFacilityDoesNotExist_ShouldThrowArgumentException()
    {
        // Arrange
        var query = new GetFacilityCourtsQuery { FacilitySlug = "nonexistent", PageNumber = 1, PageSize = 10 };

        _facilityRepository.FirstOrDefaultAsync(
            Arg.Any<Expression<Func<Facility, bool>>>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>?>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(null));

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    public async Task Handle_GivenInvalidPageNumber_ShouldThrowArgumentException(int pageNumber, int pageSize)
    {
        // Arrange
        var query = new GetFacilityCourtsQuery { FacilitySlug = "test", PageNumber = pageNumber, PageSize = pageSize };

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(1, 3)]
    [InlineData(1, 0)]
    [InlineData(1, 50)]
    public async Task Handle_GivenInvalidPageSize_ShouldThrowArgumentException(int pageNumber, int pageSize)
    {
        // Arrange
        var query = new GetFacilityCourtsQuery { FacilitySlug = "test", PageNumber = pageNumber, PageSize = pageSize };

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    private static WeeklyOpeningHours CreateUniformOpeningHours(TimeSpan open, TimeSpan close)
    {
        var dailyHours = Enum.GetValues<DayOfWeek>().Select(day =>
            DailyOpeningHours.Create(day, TimeOnly.FromTimeSpan(open), TimeOnly.FromTimeSpan(close)));
        return WeeklyOpeningHours.Create(dailyHours);
    }
}