using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using NSubstitute;
using Pricing.Application.Tariffs.Queries.CalculatePrice;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using Shared.Persistence;

namespace Pricing.Application.Tests;

public class CalculatePriceQueryHandlerTests
{
    private readonly IRepository<Tariff, TariffId> _repository;
    private readonly CalculatePriceQueryHandler _handler;

    public CalculatePriceQueryHandlerTests()
    {
        _repository = Substitute.For<IRepository<Tariff, TariffId>>();
        _handler = new CalculatePriceQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_GivenExistingTariff_ShouldCalculateAndReturnPrice()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var startTime = new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero);
        var endTime = startTime.AddHours(2);

        var query = new CalculatePriceQuery(facilityId, courtId, startTime, endTime);
        var tariff = Tariff.Create(facilityId, new Money(100m));

        _repository.FindAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Tariff, bool>>>(),
            Arg.Any<Func<IQueryable<Tariff>, IIncludableQueryable<Tariff, object>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<Tariff> { tariff }.AsReadOnly());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(200m); // 2 hours * 100
    }

    [Fact]
    public async Task Handle_GivenNoTariffFound_ShouldThrowException()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var courtId = Guid.NewGuid();
        var startTime = new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero);
        var endTime = startTime.AddHours(2);

        var query = new CalculatePriceQuery(facilityId, courtId, startTime, endTime);

        _repository.FindAsync(
            Arg.Any<System.Linq.Expressions.Expression<Func<Tariff, bool>>>(),
            Arg.Any<Func<IQueryable<Tariff>, IIncludableQueryable<Tariff, object>>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>()
        ).Returns(new List<Tariff>().AsReadOnly());

        // Act
        var action = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .WithMessage($"No tariff found for facility {facilityId}");
    }
}