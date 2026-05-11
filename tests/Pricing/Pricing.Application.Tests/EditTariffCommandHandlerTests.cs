using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using Pricing.Application.Tariffs.Commands.EditTariff;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using Shared.Persistence;
using Users.Shared.Authorization;

namespace Pricing.Application.Tests;

public class EditTariffCommandHandlerTests
{
    private readonly IRepository<Tariff, TariffId> _repository;
    private readonly IFacilityAuthorizationService _facilityAuthorizationService;
    private readonly EditTariffCommandHandler _handler;

    public EditTariffCommandHandlerTests()
    {
        _repository = Substitute.For<IRepository<Tariff, TariffId>>();
        _facilityAuthorizationService = Substitute.For<IFacilityAuthorizationService>();
        _handler = new EditTariffCommandHandler(_repository, _facilityAuthorizationService);
    }

    [Fact]
    public async Task Handle_GivenExistingTariff_ShouldUpdateAndReturnTrue()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var oldBaseHourlyRate = 50.0m;
        var newBaseHourlyRate = 100.0m;
        var courtId1 = Guid.NewGuid();
        var courtRate1 = 120.0m;

        var command = new EditTariffCommand(
            facilityId,
            newBaseHourlyRate,
            new List<EditCourtRateOverrideRequest>
            {
                new EditCourtRateOverrideRequest(courtId1, courtRate1)
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
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(facilityId);

        _repository.Received(1).Update(existingTariff);
        existingTariff.BaseHourlyRate.Amount.Should().Be(newBaseHourlyRate);
        existingTariff.CourtRateOverrides.Should().ContainSingle(o => o.CourtId == courtId1 && o.HourlyRate.Amount == courtRate1);

        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenNonExistingTariff_ShouldReturnFalse()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var baseHourlyRate = 100.0m;

        var command = new EditTariffCommand(
            facilityId,
            baseHourlyRate,
            null
        );

        _repository.FindAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Tariff, bool>>>(),
            Arg.Any<Func<IQueryable<Tariff>, IIncludableQueryable<Tariff, object>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<Tariff>().AsReadOnly());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _facilityAuthorizationService.Received(1).AuthorizeFacilityAccess(facilityId);
        _repository.DidNotReceive().Update(Arg.Any<Tariff>());
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}