using Facilities.Shared;
using FluentAssertions;
using NSubstitute;
using Reservations.Application.Reservations.Queries.GetReservation;
using Reservations.Domain.Entities;
using Reservations.Domain.ValueObjects;
using Shared.Domain.Enums;
using Shared.Persistence;
using Users.Shared;

namespace Reservations.Application.Tests;

public class GetReservationQueryHandlerTests
{
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IFacilitiesModuleApi _facilitiesModuleApi;
    private readonly GetReservationQueryHandler _handler;

    public GetReservationQueryHandlerTests()
    {
        _reservationRepository = Substitute.For<IRepository<Reservation, Guid>>();
        _currentUser = Substitute.For<ICurrentUser>();
        _facilitiesModuleApi = Substitute.For<IFacilitiesModuleApi>();
        _handler = new GetReservationQueryHandler(_reservationRepository, _currentUser, _facilitiesModuleApi);
    }

    [Fact]
    public async Task Handle_GivenValidQuery_WhenAdminUser_ShouldReturnReservationDetails()
    {
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var query = new GetReservationQuery(reservationId);

        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);
        var timeRange = TimeRange.Create(startTime, endTime);
        var reservation = Reservation.Create(userId, courtId, timeRange, 100m);

        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), true, Arg.Any<CancellationToken>())
            .Returns(reservation);

        _currentUser.GetState().Returns(new CurrentUserState(userId, "admin@test.com", "Admin", [], true));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(reservation.Id);
        result.UserId.Should().Be(reservation.UserId);
        result.CourtId.Should().Be(courtId);
        result.Status.Should().Be(reservation.Status);
    }

    [Fact]
    public async Task Handle_GivenValidQuery_WhenUserRole_ShouldReturnReservationOnlyIfOwnedByUser()
    {
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var query = new GetReservationQuery(reservationId);

        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);
        var timeRange = TimeRange.Create(startTime, endTime);
        var reservation = Reservation.Create(userId, courtId, timeRange, 100m);

        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), true, Arg.Any<CancellationToken>())
            .Returns(reservation);

        _currentUser.GetState().Returns(new CurrentUserState(userId, "user@test.com", "User", [], true));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(reservation.Id);
    }

    [Fact]
    public async Task Handle_WhenUserRoleAndReservationNotOwnedByUser_ShouldReturnNull()
    {
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var query = new GetReservationQuery(reservationId);

        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);
        var timeRange = TimeRange.Create(startTime, endTime);
        var reservation = Reservation.Create(otherUserId, Guid.NewGuid(), timeRange, 100m);

        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), true, Arg.Any<CancellationToken>())
            .Returns(reservation);

        _currentUser.GetState().Returns(new CurrentUserState(userId, "user@test.com", "User", [], true));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenUserRoleIsManagerAndReservationNotInManagedFacility_ShouldReturnNull()
    {
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var otherFacilityId = Guid.NewGuid();
        var query = new GetReservationQuery(reservationId);

        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);
        var timeRange = TimeRange.Create(startTime, endTime);
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), timeRange, 100m);

        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), true, Arg.Any<CancellationToken>())
            .Returns(reservation);

        _currentUser.GetState().Returns(new CurrentUserState(userId, "manager@test.com", "Manager", [otherFacilityId], true));

        _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(reservation.CourtId, Arg.Any<CancellationToken>())
            .Returns(facilityId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenReservationNotFound_ShouldReturnNull()
    {
        var query = new GetReservationQuery(Guid.NewGuid());

        _reservationRepository.GetByIdAsync(query.Id, Arg.Any<Func<IQueryable<Reservation>, IQueryable<Reservation>>?>(), true, Arg.Any<CancellationToken>())
            .Returns((Reservation?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
