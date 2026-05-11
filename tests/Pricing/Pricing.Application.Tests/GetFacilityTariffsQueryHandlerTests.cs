using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using Pricing.Application.Tariffs.Queries.GetFacilityTariffs;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using Shared.Persistence;

namespace Pricing.Application.Tests;

public class GetFacilityTariffsQueryHandlerTests
{
    private readonly IRepository<Tariff, TariffId> _repository;
    private readonly GetFacilityTariffsQueryHandler _handler;

    public GetFacilityTariffsQueryHandlerTests()
    {
        _repository = Substitute.For<IRepository<Tariff, TariffId>>();
        _handler = new GetFacilityTariffsQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_GivenExistingTariffs_ShouldReturnTariffDtos()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var query = new GetFacilityTariffsQuery(facilityId);

        var tariff = Tariff.Create(facilityId, new Money(100m));
        tariff.SetCourtRateOverride(courtId, new Money(120m));

        _repository.FindAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Tariff, bool>>>(),
            Arg.Any<Func<IQueryable<Tariff>, IIncludableQueryable<Tariff, object>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<Tariff> { tariff }.AsReadOnly());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        var tariffDto = result.First();
        tariffDto.Id.Should().Be(tariff.Id.Value);
        tariffDto.FacilityId.Should().Be(facilityId);
        tariffDto.BaseHourlyRate.Should().Be(100m);

        tariffDto.CourtOverrides.Should().HaveCount(1);
        tariffDto.CourtOverrides.First().CourtId.Should().Be(courtId);
        tariffDto.CourtOverrides.First().HourlyRate.Should().Be(120m);
    }

    [Fact]
    public async Task Handle_GivenNoTariffs_ShouldReturnEmptyList()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var query = new GetFacilityTariffsQuery(facilityId);

        _repository.FindAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Tariff, bool>>>(),
            Arg.Any<Func<IQueryable<Tariff>, IIncludableQueryable<Tariff, object>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<Tariff>().AsReadOnly());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}