using Facilities.Shared;
using Facilities.Shared.DTOs;
using FluentAssertions;
using NSubstitute;
using Pricing.Shared;
using Reservations.Application.Reservations.Commands.CreateReservation;
using Reservations.Domain.Entities;
using Reservations.Domain.Exceptions;
using Reservations.Domain.Services;
using Shared.Persistence;

namespace Reservations.Application.Tests;

public class CreateReservationCommandHandlerTests
{
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly IReservationChecker _reservationChecker;
    private readonly IFacilitiesModuleApi _facilitiesModuleApi;
    private readonly IPricingModuleApi _pricingModuleApi;
    private readonly CreateReservationCommandHandler _handler;

    public CreateReservationCommandHandlerTests()
    {
        _reservationRepository = Substitute.For<IRepository<Reservation, Guid>>();
        _reservationChecker = Substitute.For<IReservationChecker>();
        _facilitiesModuleApi = Substitute.For<IFacilitiesModuleApi>();
        _pricingModuleApi = Substitute.For<IPricingModuleApi>();
        _handler = new CreateReservationCommandHandler(
            _reservationRepository,
            _reservationChecker,
            _facilitiesModuleApi,
            _pricingModuleApi);
    }

    [Fact]
    public async Task Handle_GivenValidCommand_WhenAllChecksPass_ShouldCreateReservationAndReturnId()
    {
        var courtId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);

        var command = new CreateReservationCommand(courtId, startTime, endTime)
        {
            UserId = Guid.NewGuid()
        };

        _reservationChecker.IsCourtAvailableAsync(courtId, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(true);

        _reservationChecker.IsUserHasConcurrentReservationAsync(command.UserId, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(false);

        _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns(facilityId);

        _facilitiesModuleApi.GetFacilityAvailabilityInfoAsync(facilityId, Arg.Any<CancellationToken>())
            .Returns(CreateFacilityAvailabilityDto(facilityId, courtId, 60));

        _pricingModuleApi.CalculatePriceAsync(facilityId, courtId, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(100m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
        await _reservationRepository.Received(1).AddAsync(
            Arg.Is<Reservation>(r => r.UserId == command.UserId && r.CourtId == courtId),
            Arg.Any<CancellationToken>());
        await _reservationRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStartTimeInPast_ShouldThrowReservationInPastException()
    {
        var courtId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.AddHours(-1);
        var endTime = startTime.AddHours(1);
        var command = new CreateReservationCommand(courtId, startTime, endTime)
        {
            UserId = Guid.NewGuid()
        };

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ReservationInPastException>();
        await _facilitiesModuleApi.DidNotReceiveWithAnyArgs().GetFacilityIdByCourtIdAsync(default);
    }

    [Fact]
    public async Task Handle_WhenCourtNotAvailable_ShouldThrowReservationOverlapException()
    {
        var courtId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);

        var command = new CreateReservationCommand(courtId, startTime, endTime)
        {
            UserId = Guid.NewGuid()
        };

        _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns(facilityId);

        _facilitiesModuleApi.GetFacilityAvailabilityInfoAsync(facilityId, Arg.Any<CancellationToken>())
            .Returns(CreateFacilityAvailabilityDto(facilityId, courtId, 60));

        _reservationChecker.IsCourtAvailableAsync(courtId, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(false);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<ReservationOverlapException>();
    }

    [Fact]
    public async Task Handle_WhenUserHasConcurrentReservation_ShouldThrowUserAlreadyHasReservationException()
    {
        var courtId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);

        var command = new CreateReservationCommand(courtId, startTime, endTime)
        {
            UserId = Guid.NewGuid()
        };

        _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns(facilityId);

        _facilitiesModuleApi.GetFacilityAvailabilityInfoAsync(facilityId, Arg.Any<CancellationToken>())
            .Returns(CreateFacilityAvailabilityDto(facilityId, courtId, 60));

        _reservationChecker.IsCourtAvailableAsync(courtId, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(true);

        _reservationChecker.IsUserHasConcurrentReservationAsync(command.UserId, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(true);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UserAlreadyHasReservationException>();
    }

    [Fact]
    public async Task Handle_WhenPayOnSiteIsTrue_ShouldCreateReservationWithAwaitingOnSitePaymentStatus()
    {
        var courtId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);

        var command = new CreateReservationCommand(courtId, startTime, endTime, PayOnSite: true)
        {
            UserId = Guid.NewGuid()
        };

        _reservationChecker.IsCourtAvailableAsync(courtId, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(true);

        _reservationChecker.IsUserHasConcurrentReservationAsync(command.UserId, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(false);

        _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns(facilityId);

        _facilitiesModuleApi.GetFacilityAvailabilityInfoAsync(facilityId, Arg.Any<CancellationToken>())
            .Returns(CreateFacilityAvailabilityDto(facilityId, courtId, 60));

        _pricingModuleApi.CalculatePriceAsync(facilityId, courtId, startTime, endTime, Arg.Any<CancellationToken>())
            .Returns(100m);

        await _handler.Handle(command, CancellationToken.None);

        await _reservationRepository.Received(1).AddAsync(
            Arg.Is<Reservation>(r => r.Status == Domain.Enums.ReservationStatus.AwaitingOnSitePayment),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenFacilityNotFoundForCourt_ShouldThrowInvalidOperationException()
    {
        var courtId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);

        var command = new CreateReservationCommand(courtId, startTime, endTime)
        {
            UserId = Guid.NewGuid()
        };

        _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns((Guid?)null);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Facility not found for the given court.");
    }

    [Fact]
    public async Task Handle_WhenFacilityAvailabilityInfoNotFound_ShouldThrowInvalidOperationException()
    {
        var courtId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);

        var command = new CreateReservationCommand(courtId, startTime, endTime)
        {
            UserId = Guid.NewGuid()
        };

        _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns(facilityId);

        _facilitiesModuleApi.GetFacilityAvailabilityInfoAsync(facilityId, Arg.Any<CancellationToken>())
            .Returns((FacilityAvailabilityDto?)null);

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Facility availability info not found.");
    }

    [Fact]
    public async Task Handle_WhenCourtNotFoundInFacility_ShouldThrowInvalidOperationException()
    {
        var courtId = Guid.NewGuid();
        var otherCourtId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.Date.AddDays(3).AddHours(8);
        var endTime = startTime.AddHours(1);

        var command = new CreateReservationCommand(courtId, startTime, endTime)
        {
            UserId = Guid.NewGuid()
        };

        _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(courtId, Arg.Any<CancellationToken>())
            .Returns(facilityId);

        _facilitiesModuleApi.GetFacilityAvailabilityInfoAsync(facilityId, Arg.Any<CancellationToken>())
            .Returns(CreateFacilityAvailabilityDto(facilityId, otherCourtId, 60));

        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Court not found in facility.");
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
