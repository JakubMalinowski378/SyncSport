using Facilities.Application.Facilities.Commands.EditFacility;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Domain;
using Shared.Persistence;
using Storage;
using System.Linq.Expressions;
using Users.Shared.Authorization;

namespace Facilities.Application.Tests;

public class EditFacilityCommandHandlerTests
{
    private readonly IRepository<Facility, FacilityId> _facilityRepository;
    private readonly IFacilityAuthorizationService _facilityAuthorizationService;
    private readonly IImageStorageService _imageStorageService;
    private readonly EditFacilityCommandHandler _handler;

    public EditFacilityCommandHandlerTests()
    {
        _facilityRepository = Substitute.For<IRepository<Facility, FacilityId>>();
        _facilityAuthorizationService = Substitute.For<IFacilityAuthorizationService>();
        _imageStorageService = Substitute.For<IImageStorageService>();
        _handler = new EditFacilityCommandHandler(_facilityRepository, _facilityAuthorizationService, _imageStorageService);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotAuthorized_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new EditFacilityCommand
        {
            FacilityId = Guid.NewGuid(),
            Name = "Updated Facility",
            Address = "Updated Address"
        };

        _facilityAuthorizationService
            .When(x => x.AuthorizeFacilityAccess(command.FacilityId))
            .Throw(new UnauthorizedAccessException("You are not authorized to access this facility."));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>();

        var anyId = Arg.Any<FacilityId>();
        await _facilityRepository.DidNotReceiveWithAnyArgs()
            .GetByIdAsync(anyId, Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenValidCommand_WhenFacilityExists_ShouldUpdateFacility()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var command = new EditFacilityCommand
        {
            FacilityId = facilityId,
            Name = "Updated Facility",
            Address = "Updated Address",
            ReservationDuration = 60,
            WeeklyHours = "[{\"DayOfWeek\":1,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":2,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":3,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":4,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":5,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":6,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":0,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false}]"
        };

        var facility = Facility.Create(
            "Test Facility",
            "test-facility",
            "Test Address",
            60,
            CreateUniformOpeningHours(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        _facilityRepository.GetByIdAsync(
            Arg.Is<FacilityId>(id => id.Value == facilityId),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(facility));

        // Let ForNameReturn null so no duplicate checking passes
        _facilityRepository.FirstOrDefaultAsync(
            Arg.Any<Expression<Func<Facility, bool>>>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Is(true),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(null));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(facilityId);
        facility.Name.Should().Be("Updated Facility");
        facility.Address.Should().Be("Updated Address");

        _facilityRepository.Received(1).Update(facility);
        await _facilityRepository.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Handle_WhenFacilityDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var command = new EditFacilityCommand
        {
            FacilityId = Guid.NewGuid(),
            Name = "Updated",
            Address = "Address",
            WeeklyHours = "[{\"DayOfWeek\":1,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":2,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":3,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":4,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":5,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":6,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":0,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false}]"
        };

        _facilityRepository.GetByIdAsync(
            Arg.Any<FacilityId>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(null));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>();

        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(command.FacilityId);
        _facilityRepository.DidNotReceive().Update(Arg.Any<Facility>());
    }

    [Fact]
    public async Task Handle_WhenAnotherFacilityWithSameNameExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var targetFacilityId = Guid.NewGuid();
        var otherFacilityId = Guid.NewGuid();
        var command = new EditFacilityCommand
        {
            FacilityId = targetFacilityId,
            Name = "Duplicate Name",
            Address = "Address",
            ReservationDuration = 60,
            WeeklyHours = "[{\"DayOfWeek\":1,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":2,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":3,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":4,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":5,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":6,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false},{\"DayOfWeek\":0,\"OpenTime\":\"08:00\",\"CloseTime\":\"22:00\",\"IsClosed\":false}]"
        };

        var facility = Facility.Create(
            "Old Name",
            "old-name",
            "Test Address",
            60,
            CreateUniformOpeningHours(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        var existingFacility = Facility.Create(
            "Duplicate Name",
            "duplicate-name",
            "Another Address",
            60,
            CreateUniformOpeningHours(TimeSpan.FromHours(8), TimeSpan.FromHours(22)));

        typeof(Entity<FacilityId>).GetProperty("Id")!.SetValue(existingFacility, new FacilityId(otherFacilityId));

        _facilityRepository.GetByIdAsync(
            Arg.Is<FacilityId>(id => id.Value == targetFacilityId),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Is(false),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(facility));

        _facilityRepository.FirstOrDefaultAsync(
            Arg.Any<Expression<Func<Facility, bool>>>(),
            Arg.Any<Func<IQueryable<Facility>, IQueryable<Facility>>>(),
            Arg.Is(true),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Facility?>(existingFacility));

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>();

        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(targetFacilityId);
        _facilityRepository.DidNotReceive().Update(Arg.Any<Facility>());
    }

    private static WeeklyOpeningHours CreateUniformOpeningHours(TimeSpan open, TimeSpan close)
    {
        var dailyHours = Enum.GetValues<DayOfWeek>().Select(day =>
            DailyOpeningHours.Create(day, TimeOnly.FromTimeSpan(open), TimeOnly.FromTimeSpan(close)));
        return WeeklyOpeningHours.Create(dailyHours);
    }
}