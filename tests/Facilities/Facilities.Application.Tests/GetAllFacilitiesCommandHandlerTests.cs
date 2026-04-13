using Facilities.Application.Facilities.Commands.GetAllFacilities;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Persistence;
using System.Linq.Expressions;

namespace Facilities.Application.Tests;

public class GetAllFacilitiesCommandHandlerTests
{
    private readonly IRepository<Facility, FacilityId> _facilityRepository;
    private readonly GetAllFacilitiesCommandHandler _handler;

    public GetAllFacilitiesCommandHandlerTests()
    {
        _facilityRepository = Substitute.For<IRepository<Facility, FacilityId>>();
        _handler = new GetAllFacilitiesCommandHandler(_facilityRepository);
    }

    [Fact]
    public async Task Handle_GivenValidPagination_ShouldReturnPagedResult()
    {
        // Arrange
        var command = new GetAllFacilitiesCommand(1, 10);

        var facility1 = Facility.Create(
            "Test Facility 1",
            "Address 1",
            WeeklyOpeningHours.CreateUniform(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        var facility2 = Facility.Create(
            "Test Facility 2",
            "Address 2",
            WeeklyOpeningHours.CreateUniform(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        var facilities = new List<Facility> { facility1, facility2 };

        _facilityRepository.GetPagedAsync(
            1,
            10,
            Arg.Any<Expression<Func<Facility, bool>>?>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>?>(),
            Arg.Any<Func<IQueryable<Facility>, IOrderedQueryable<Facility>>?>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((items: (IEnumerable<Facility>)facilities, totalCount: 2)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    public async Task Handle_GivenInvalidPageNumber_ShouldThrowArgumentException(int pageNumber, int pageSize)
    {
        // Arrange
        var command = new GetAllFacilitiesCommand(pageNumber, pageSize);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

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
        var command = new GetAllFacilitiesCommand(pageNumber, pageSize);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("PageSize must be one of: 5, 10, 15, 20, 25, 30.");
    }
}