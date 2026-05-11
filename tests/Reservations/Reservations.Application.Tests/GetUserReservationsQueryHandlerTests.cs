using FluentAssertions;
using NSubstitute;
using Reservations.Application.Reservations.Queries.GetUserReservations;
using Reservations.Domain.Entities;
using Reservations.Domain.ValueObjects;
using Shared.Pagination;
using Shared.Persistence;
using System.Linq.Expressions;

namespace Reservations.Application.Tests;

public class GetUserReservationsQueryHandlerTests
{
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly GetUserReservationsQueryHandler _handler;

    public GetUserReservationsQueryHandlerTests()
    {
        _reservationRepository = Substitute.For<IRepository<Reservation, Guid>>();
        _handler = new GetUserReservationsQueryHandler(_reservationRepository);
    }

    [Fact]
    public async Task Handle_GivenValidQuery_WhenReservationsExist_ShouldReturnPagedResult()
    {
        var userId = Guid.NewGuid();
        var query = new GetUserReservationsQuery
        {
            UserId = userId,
            PageNumber = 1,
            PageSize = 10
        };

        var reservation1 = CreateReservation(userId);
        var reservation2 = CreateReservation(userId);
        var reservations = new List<Reservation> { reservation1, reservation2 };

        _reservationRepository.GetPagedAsync(
            1, 10,
            Arg.Any<Expression<Func<Reservation, bool>>?>(),
            Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(),
            Arg.Any<Func<IQueryable<Reservation>, IOrderedQueryable<Reservation>>?>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((items: (IEnumerable<Reservation>)reservations, totalCount: 2)));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(r => r.CourtId.Should().NotBeEmpty());
    }

    [Fact]
    public async Task Handle_GivenValidQuery_WhenNoReservations_ShouldReturnEmptyPagedResult()
    {
        var query = new GetUserReservationsQuery
        {
            UserId = Guid.NewGuid(),
            PageNumber = 1,
            PageSize = 10
        };

        _reservationRepository.GetPagedAsync(
            1, 10,
            Arg.Any<Expression<Func<Reservation, bool>>?>(),
            Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(),
            Arg.Any<Func<IQueryable<Reservation>, IOrderedQueryable<Reservation>>?>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((items: (IEnumerable<Reservation>)new List<Reservation>(), totalCount: 0)));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    private static Reservation CreateReservation(Guid userId)
    {
        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);
        var timeRange = TimeRange.Create(startTime, endTime);
        return Reservation.Create(userId, Guid.NewGuid(), timeRange, 100m);
    }
}
