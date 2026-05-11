using Facilities.Shared;
using Facilities.Shared.DTOs;
using FluentAssertions;
using NSubstitute;
using Reservations.Application.Common.Interfaces;
using Reservations.Application.Reservations.Queries.GetMyReservations;
using Reservations.Domain.Entities;
using Reservations.Domain.Enums;
using Reservations.Domain.ValueObjects;
using Shared.Persistence;

namespace Reservations.Application.Tests;

public class GetMyReservationsQueryHandlerTests
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IFacilitiesModuleApi _facilitiesModuleApi;
    private readonly GetMyReservationsQueryHandler _handler;

    public GetMyReservationsQueryHandlerTests()
    {
        _reservationRepository = Substitute.For<IReservationRepository>();
        _facilitiesModuleApi = Substitute.For<IFacilitiesModuleApi>();
        _handler = new GetMyReservationsQueryHandler(_reservationRepository, _facilitiesModuleApi);
    }

    [Fact]
    public async Task Handle_GivenValidQuery_WhenReservationsExist_ShouldReturnEnrichedPagedResult()
    {
        var userId = Guid.NewGuid();
        var courtId1 = Guid.NewGuid();
        var courtId2 = Guid.NewGuid();
        var query = new GetMyReservationsQuery
        {
            UserId = userId,
            PageNumber = 1,
            PageSize = 10
        };

        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);

        var reservation1 = Reservation.Create(userId, courtId1, TimeRange.Create(startTime, endTime), 100m);
        var reservation2 = Reservation.Create(userId, courtId2, TimeRange.Create(startTime.AddDays(1), endTime.AddDays(1)), 100m);

        var reservations = new List<Reservation> { reservation1, reservation2 };

        _reservationRepository.GetMyReservationsAsync(userId, Arg.Any<ReservationFilters>(), Arg.Any<CancellationToken>())
            .Returns(reservations);

        var courtDetails = new Dictionary<Guid, CourtWithFacilityDto>
        {
            { courtId1, new CourtWithFacilityDto(courtId1, "Court 1", Guid.NewGuid(), "Facility A") },
            { courtId2, new CourtWithFacilityDto(courtId2, "Court 2", Guid.NewGuid(), "Facility B") }
        };

        _facilitiesModuleApi.GetCourtsWithFacilityByIdsAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.Contains(courtId1) && ids.Contains(courtId2)),
            Arg.Any<CancellationToken>())
            .Returns(courtDetails);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().Contain(r => r.CourtName == "Court 1" && r.FacilityName == "Facility A");
        result.Items.Should().Contain(r => r.CourtName == "Court 2" && r.FacilityName == "Facility B");
    }

    [Fact]
    public async Task Handle_GivenValidQuery_WhenNoReservations_ShouldReturnEmptyPagedResult()
    {
        var userId = Guid.NewGuid();
        var query = new GetMyReservationsQuery
        {
            UserId = userId,
            PageNumber = 1,
            PageSize = 10
        };

        _reservationRepository.GetMyReservationsAsync(userId, Arg.Any<ReservationFilters>(), Arg.Any<CancellationToken>())
            .Returns(new List<Reservation>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_GivenValidQuery_ShouldApplyStatusFilter()
    {
        var userId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var query = new GetMyReservationsQuery
        {
            UserId = userId,
            PageNumber = 1,
            PageSize = 10,
            Status = ReservationStatus.Pending
        };

        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);

        var reservation = Reservation.Create(userId, courtId, TimeRange.Create(startTime, endTime), 100m);
        var reservations = new List<Reservation> { reservation };

        _reservationRepository.GetMyReservationsAsync(
            userId,
            Arg.Is<ReservationFilters>(f => f.Status == ReservationStatus.Pending),
            Arg.Any<CancellationToken>())
            .Returns(reservations);

        _facilitiesModuleApi.GetCourtsWithFacilityByIdsAsync(
            Arg.Any<IEnumerable<Guid>>(),
            Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, CourtWithFacilityDto>
            {
                { courtId, new CourtWithFacilityDto(courtId, "Court 1", Guid.NewGuid(), "Facility A") }
            });

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
    }
}
