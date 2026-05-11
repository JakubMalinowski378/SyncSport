using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using Pricing.Application.Tariffs.Commands.CreateTariff;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using Shared.Persistence;
using Users.Shared.Authorization;

namespace Pricing.Application.Tests;

public class CreateTariffCommandHandlerTests
{
    private readonly IRepository<Tariff, TariffId> _repository;
    private readonly IFacilityAuthorizationService _facilityAuthorizationService;
    private readonly CreateTariffCommandHandler _handler;

    public CreateTariffCommandHandlerTests()
    {
        _repository = Substitute.For<IRepository<Tariff, TariffId>>();
        _facilityAuthorizationService = Substitute.For<IFacilityAuthorizationService>();
        _handler = new CreateTariffCommandHandler(_repository, _facilityAuthorizationService);
    }

    [Fact]
    public async Task Handle_GivenNoExistingTariff_ShouldCreateNewTariff()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var baseHourlyRate = 100.0m;
        var courtId1 = Guid.NewGuid();
        var courtRate1 = 120.0m;

        var command = new CreateTariffCommand(
            facilityId,
            baseHourlyRate,
            new List<CourtRateOverrideRequest>
            {
                new CourtRateOverrideRequest(courtId1, courtRate1)
            }
        );

        _repository.FindAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Tariff, bool>>>(),
            Arg.Any<Func<IQueryable<Tariff>, IIncludableQueryable<Tariff, object>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<Tariff>().AsReadOnly());

        // Act
        var tariffId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(facilityId);

        await _repository.Received(1).AddAsync(Arg.Is<Tariff>(t =>
            t.FacilityId == facilityId &&
            t.BaseHourlyRate.Amount == baseHourlyRate &&
            t.CourtRateOverrides.Count == 1 &&
            t.CourtRateOverrides.Any(o => o.CourtId == courtId1 && o.HourlyRate.Amount == courtRate1)
        ), Arg.Any<CancellationToken>());

        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        tariffId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_GivenExistingTariff_ShouldUpdateExistingTariff()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var oldBaseHourlyRate = 50.0m;
        var newBaseHourlyRate = 100.0m;
        var courtId1 = Guid.NewGuid();
        var courtRate1 = 120.0m;

        var command = new CreateTariffCommand(
            facilityId,
            newBaseHourlyRate,
            new List<CourtRateOverrideRequest>
            {
                new CourtRateOverrideRequest(courtId1, courtRate1)
            }
        );

        var existingTariff = Tariff.Create(facilityId, new Money(oldBaseHourlyRate));

        _repository.FindAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Tariff, bool>>>(),
            Arg.Any<Func<IQueryable<Tariff>, IIncludableQueryable<Tariff, object>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<Tariff> { existingTariff }.AsReadOnly());

        // Act
        var tariffId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(facilityId);

        _repository.Received(1).Update(existingTariff);
        existingTariff.BaseHourlyRate.Amount.Should().Be(newBaseHourlyRate);
        existingTariff.CourtRateOverrides.Should().ContainSingle(o => o.CourtId == courtId1 && o.HourlyRate.Amount == courtRate1);

        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        tariffId.Should().Be(existingTariff.Id.Value);
    }
}