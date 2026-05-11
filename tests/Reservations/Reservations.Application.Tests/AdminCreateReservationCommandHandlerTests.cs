using Facilities.Shared;
using Facilities.Shared.DTOs;
using FluentAssertions;
using NSubstitute;
using Pricing.Shared;
using Reservations.Application.Reservations.Commands.AdminCreateReservation;
using Reservations.Domain.Entities;
using Reservations.Domain.Exceptions;
using Reservations.Domain.Services;
using Shared.Persistence;
using Users.Shared.Authorization;

namespace Reservations.Application.Tests;

public class AdminCreateReservationCommandHandlerTests
{
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly IReservationChecker _reservationChecker;
    private readonly IPricingModuleApi _pricingModuleApi;
    private readonly IFacilityAuthorizationService _facilityAuthorizationService;
    private readonly IFacilitiesModuleApi _facilitiesModuleApi;
    private readonly AdminCreateReservationCommandHandler _handler;

    public AdminCreateReservationCommandHandlerTests()
    {
        _reservationRepository = Substitute.For<IRepository<Reservation, Guid>>();
        _reservationChecker = Substitute.For<IReservationChecker>();
        _pricingModuleApi = Substitute.For<IPricingModuleApi>();
        _facilityAuthorizationService = Substitute.For<IFacilityAuthorizationService>();
        _facilitiesModuleApi = Substitute.For<IFacilitiesModuleApi>();
        _handler = new AdminCreateReservationCommandHandler(
            _reservationRepository,
            _reservationChecker,
            _pricingModuleApi,
            _facilityAuthorizationService,
            _facilitiesModuleApi);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_WhenAllChecksPass_ShouldCreateReservationAndReturnId()
    {
        var courtId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);

        var command = new AdminCreateReservationCommand(userId, courtId, startTime, endTime);

        _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns(facilityId);

        _facilitiesModuleApi.GetFacilityAvailabilityInfoAsync(facilityId, Arg.Any<CancellationToken>())
            .Returns(CreateFacilityAvailabilityDto(facilityId, courtId, 60));

        _reservationChecker.IsCourtAvailableAsync(courtId, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(true);

        _reservationChecker.IsUserHasConcurrentReservationAsync(userId, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(false);

        _pricingModuleApi.CalculatePriceAsync(facilityId, courtId, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(100m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(facilityId);
        await _reservationRepository.Received(1).AddAsync(
            Arg.Is<Reservation>(r => r.UserId == userId && r.CourtId == courtId),
            Arg.Any<CancellationToken>());
        await _reservationRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCourtNotFound_ShouldThrowInvalidOperationException()
    {
        var command = new AdminCreateReservationCommand(
            Guid.NewGuid(), Guid.NewGuid(),
            DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8),
            DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(9));

        _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(command.CourtId, Arg.Any<CancellationToken>())
            .Returns((Guid?)null);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Court not found.");
    }

    [Fact]
    public async Task Handle_WhenStartTimeInPast_ShouldThrowReservationInPastException()
    {
        var courtId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.AddHours(-1);
        var endTime = startTime.AddHours(1);

        var command = new AdminCreateReservationCommand(Guid.NewGuid(), courtId, startTime, endTime);

        _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns(facilityId);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ReservationInPastException>();
    }

    [Fact]
    public async Task Handle_WhenCourtNotAvailable_ShouldThrowReservationOverlapException()
    {
        var courtId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);

        var command = new AdminCreateReservationCommand(Guid.NewGuid(), courtId, startTime, endTime);

        _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns(facilityId);

        _facilitiesModuleApi.GetFacilityAvailabilityInfoAsync(facilityId, Arg.Any<CancellationToken>())
            .Returns(CreateFacilityAvailabilityDto(facilityId, courtId, 60));

        _reservationChecker.IsCourtAvailableAsync(courtId, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(false);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ReservationOverlapException>();
    }

    private static FacilityAvailabilityDto CreateFacilityAvailabilityDto(Guid facilityId, Guid courtId, int durationMinutes)
    {
        return new FacilityAvailabilityDto(
            facilityId,
            "Test Facility",
            new List<CourtAvailabilityInfo>
            {
                new(courtId, "Court 1", durationMinutes)
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
            });
    }
}
