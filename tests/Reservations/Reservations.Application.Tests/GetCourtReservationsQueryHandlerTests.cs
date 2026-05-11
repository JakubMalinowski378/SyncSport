using Facilities.Shared;
using Facilities.Shared.DTOs;
using FluentAssertions;
using NSubstitute;
using Reservations.Application.Reservations.Queries.GetCourtReservations;
using Reservations.Domain.Entities;
using Reservations.Domain.ValueObjects;
using Shared.Persistence;
using System.Linq.Expressions;

namespace Reservations.Application.Tests;

public class GetCourtReservationsQueryHandlerTests
{
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly IFacilitiesModuleApi _facilitiesModuleApi;
    private readonly GetCourtReservationsQueryHandler _handler;

    public GetCourtReservationsQueryHandlerTests()
    {
        _reservationRepository = Substitute.For<IRepository<Reservation, Guid>>();
        _facilitiesModuleApi = Substitute.For<IFacilitiesModuleApi>();
        _handler = new GetCourtReservationsQueryHandler(_reservationRepository, _facilitiesModuleApi);
    }

    [Fact]
    public async Task Handle_GivenValidQuery_WhenReservationsExist_ShouldReturnWeekViewWithSlots()
    {
        var courtId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var weekDate = new DateOnly(2026, 5, 11); // Monday

        var query = new GetCourtReservationsQuery(courtId, weekDate);

        var mondayUtc = new DateTimeOffset(2026, 5, 11, 6, 0, 0, TimeSpan.Zero); // 08:00 Polish time

        var reservation = Reservation.Create(
            Guid.NewGuid(),
            courtId,
            TimeRange.Create(mondayUtc.AddHours(1), mondayUtc.AddHours(2)),
            100m);

        var reservations = new List<Reservation> { reservation };

        _reservationRepository.FindAsync(
            Arg.Any<Expression<Func<Reservation, bool>>>(),
            null,
            true,
            Arg.Any<CancellationToken>())
            .Returns(reservations);

        _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns(facilityId);

        _facilitiesModuleApi.GetFacilityAvailabilityInfoAsync(facilityId, Arg.Any<CancellationToken>())
            .Returns(new FacilityAvailabilityDto(
                facilityId,
                "Test Facility",
                new List<CourtAvailabilityInfo>
                {
                    new(courtId, "Court 1", 60)
                },
                new List<OpeningHoursAvailabilityInfo>
                {
                    new(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(22, 0)),
                    new(DayOfWeek.Tuesday, new TimeOnly(8, 0), new TimeOnly(22, 0)),
                    new(DayOfWeek.Wednesday, new TimeOnly(8, 0), new TimeOnly(22, 0)),
                    new(DayOfWeek.Thursday, new TimeOnly(8, 0), new TimeOnly(22, 0)),
                    new(DayOfWeek.Friday, new TimeOnly(8, 0), new TimeOnly(22, 0)),
                    new(DayOfWeek.Saturday, new TimeOnly(8, 0), new TimeOnly(22, 0)),
                    new(DayOfWeek.Sunday, new TimeOnly(8, 0), new TimeOnly(22, 0)),
                }));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Days.Should().HaveCount(7);
        result.WeekStartDate.Should().Be(new DateTimeOffset(2026, 5, 10, 22, 0, 0, TimeSpan.Zero)); // Polish midnight UTC
    }

    [Fact]
    public async Task Handle_GivenValidQuery_WhenNoReservations_ShouldReturnAllSlotsAsAvailable()
    {
        var courtId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var weekDate = new DateOnly(2026, 5, 11);

        var query = new GetCourtReservationsQuery(courtId, weekDate);

        _reservationRepository.FindAsync(
            Arg.Any<Expression<Func<Reservation, bool>>>(),
            null,
            true,
            Arg.Any<CancellationToken>())
            .Returns(new List<Reservation>());

        _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns(facilityId);

        _facilitiesModuleApi.GetFacilityAvailabilityInfoAsync(facilityId, Arg.Any<CancellationToken>())
            .Returns(new FacilityAvailabilityDto(
                facilityId,
                "Test Facility",
                new List<CourtAvailabilityInfo>
                {
                    new(courtId, "Court 1", 60)
                },
                new List<OpeningHoursAvailabilityInfo>
                {
                    new(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(22, 0)),
                }));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Days.Should().HaveCount(7);
        result.Days.Should().Contain(d => d.DayOfWeek == DayOfWeek.Monday);
    }

    [Fact]
    public async Task Handle_WhenFacilityNotFound_ShouldThrowException()
    {
        var courtId = Guid.NewGuid();
        var query = new GetCourtReservationsQuery(courtId, new DateOnly(2026, 5, 11));

        _reservationRepository.FindAsync(
            Arg.Any<Expression<Func<Reservation, bool>>>(),
            null,
            true,
            Arg.Any<CancellationToken>())
            .Returns(new List<Reservation>());

        _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns((Guid?)null);

        var action = async () => await _handler.Handle(query, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>()
            .WithMessage("Facility for court not found.");
    }
}
