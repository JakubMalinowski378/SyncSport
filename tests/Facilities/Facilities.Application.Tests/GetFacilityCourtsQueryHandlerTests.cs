using Facilities.Application.Facilities.Queries.GetFacilityCourts;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using Shared.Persistence;

namespace Facilities.Application.Tests;

public class GetFacilityCourtsQueryHandlerTests
{
    private readonly IRepository<Facility, FacilityId> _facilityRepository;
    private readonly GetFacilityCourtsQueryHandler _handler;

    public GetFacilityCourtsQueryHandlerTests()
    {
        _facilityRepository = Substitute.For<IRepository<Facility, FacilityId>>();
        _handler = new GetFacilityCourtsQueryHandler(_facilityRepository);
    }

    [Fact]
    public async Task Handle_GivenValidQuery_WhenFacilityExists_ShouldReturnPagedResultOfCourts()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var query = new GetFacilityCourtsQuery(facilityId, 1, 10);

        var facility = Facility.Create(
            "Test Facility",
            "Address",
            WeeklyOpeningHours.CreateUniform(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        facility.AddCourt("Court 1", "Clay");
        facility.AddCourt("Court 2", "Grass");

        _facilityRepository.GetByIdAsync(
            Arg.Is<FacilityId>(id => id.Value == facilityId),
            Arg.Any<Func<IQueryable<Facility>, IIncludableQueryable<Facility, object>>>(),
            Arg.Is(true),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(facility));

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
        var query = new GetFacilityCourtsQuery(Guid.NewGuid(), 1, 10);

        _facilityRepository.GetByIdAsync(
            Arg.Any<FacilityId>(),
            Arg.Any<Func<IQueryable<Facility>, IIncludableQueryable<Facility, object>>>(),
            Arg.Is(true),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(null));

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Facility not found");
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    public async Task Handle_GivenInvalidPageNumber_ShouldThrowArgumentException(int pageNumber, int pageSize)
    {
        // Arrange
        var query = new GetFacilityCourtsQuery(Guid.NewGuid(), pageNumber, pageSize);

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("PageNumber must be greater than 0.");
    }

    [Theory]
    [InlineData(1, 3)]
    [InlineData(1, 0)]
    [InlineData(1, 50)]
    public async Task Handle_GivenInvalidPageSize_ShouldThrowArgumentException(int pageNumber, int pageSize)
    {
        // Arrange
        var query = new GetFacilityCourtsQuery(Guid.NewGuid(), pageNumber, pageSize);

        // Act & Assert
        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("PageSize must be one of: 5, 10, 15, 20, 25, 30.");
    }
}