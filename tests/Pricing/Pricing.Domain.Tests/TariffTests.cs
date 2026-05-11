using FluentAssertions;
using Pricing.Domain.Entities;
using Pricing.Domain.Enums;
using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.Tests;

public class TariffTests
{
    [Fact]
    public void Create_GivenValidData_ShouldCreateTariff()
    {
        // Arrange
        var facilityId = Guid.NewGuid();
        var baseHourlyRate = new Money(100m);

        // Act
        var tariff = Tariff.Create(facilityId, baseHourlyRate);

        // Assert
        tariff.Should().NotBeNull();
        tariff.FacilityId.Should().Be(facilityId);
        tariff.BaseHourlyRate.Should().Be(baseHourlyRate);
        tariff.CourtId.Should().BeNull();
        tariff.PriceRules.Should().BeEmpty();
        tariff.CourtRateOverrides.Should().BeEmpty();
    }

    [Fact]
    public void AddRule_GivenRule_ShouldAddRuleToTariff()
    {
        // Arrange
        var tariff = Tariff.Create(Guid.NewGuid(), new Money(100m));
        var rule = PriceRule.CreateWeekendRule(1.5m);

        // Act
        tariff.AddRule(rule);

        // Assert
        tariff.PriceRules.Should().ContainSingle(r => r.Id == rule.Id);
    }

    [Fact]
    public void RemoveRule_GivenExistingRuleId_ShouldRemoveRule()
    {
        // Arrange
        var tariff = Tariff.Create(Guid.NewGuid(), new Money(100m));
        var rule = PriceRule.CreateWeekendRule(1.5m);
        tariff.AddRule(rule);

        // Act
        tariff.RemoveRule(rule.Id);

        // Assert
        tariff.PriceRules.Should().BeEmpty();
    }

    [Fact]
    public void UpdateBaseRate_GivenNewRate_ShouldUpdateBaseRate()
    {
        // Arrange
        var tariff = Tariff.Create(Guid.NewGuid(), new Money(100m));
        var newRate = new Money(150m);

        // Act
        tariff.UpdateBaseRate(newRate);

        // Assert
        tariff.BaseHourlyRate.Should().Be(newRate);
    }

    [Fact]
    public void SetCourtRateOverride_GivenNewCourtId_ShouldAddOverride()
    {
        // Arrange
        var tariff = Tariff.Create(Guid.NewGuid(), new Money(100m));
        var courtId = Guid.NewGuid();
        var overrideRate = new Money(120m);

        // Act
        tariff.SetCourtRateOverride(courtId, overrideRate);

        // Assert
        tariff.CourtRateOverrides.Should().ContainSingle(o => o.CourtId == courtId && o.HourlyRate == overrideRate);
    }

    [Fact]
    public void SetCourtRateOverride_GivenExistingCourtId_ShouldUpdateOverride()
    {
        // Arrange
        var tariff = Tariff.Create(Guid.NewGuid(), new Money(100m));
        var courtId = Guid.NewGuid();
        tariff.SetCourtRateOverride(courtId, new Money(120m));
        var newOverrideRate = new Money(150m);

        // Act
        tariff.SetCourtRateOverride(courtId, newOverrideRate);

        // Assert
        tariff.CourtRateOverrides.Should().ContainSingle(o => o.CourtId == courtId && o.HourlyRate == newOverrideRate);
    }

    [Fact]
    public void CalculatePrice_GivenNoRulesAndNoOverride_ShouldCalculateWithBaseRate()
    {
        // Arrange
        var baseRate = 100m;
        var tariff = Tariff.Create(Guid.NewGuid(), new Money(baseRate));
        var start = new DateTimeOffset(new DateTime(2026, 5, 11, 10, 0, 0), TimeSpan.Zero); // Monday
        var end = start.AddHours(2);

        // Act
        var price = tariff.CalculatePrice(start, end);

        // Assert
        price.Amount.Should().Be(baseRate * 2);
    }

    [Fact]
    public void CalculatePrice_GivenCourtOverride_ShouldCalculateWithOverrideRate()
    {
        // Arrange
        var baseRate = 100m;
        var overrideRate = 120m;
        var tariff = Tariff.Create(Guid.NewGuid(), new Money(baseRate));
        var courtId = Guid.NewGuid();
        tariff.SetCourtRateOverride(courtId, new Money(overrideRate));

        var start = new DateTimeOffset(new DateTime(2026, 5, 11, 10, 0, 0), TimeSpan.Zero); // Monday
        var end = start.AddHours(2);

        // Act
        var price = tariff.CalculatePrice(start, end, courtId);

        // Assert
        price.Amount.Should().Be(overrideRate * 2);
    }

    [Fact]
    public void CalculatePrice_GivenPriceRule_ShouldApplyRuleToTotal()
    {
        // Arrange
        var baseRate = 100m;
        var tariff = Tariff.Create(Guid.NewGuid(), new Money(baseRate));
        tariff.AddRule(PriceRule.CreateWeekendRule(1.5m));

        var start = new DateTimeOffset(new DateTime(2026, 5, 9, 10, 0, 0), TimeSpan.Zero); // Saturday
        var end = start.AddHours(2);

        // Act
        var price = tariff.CalculatePrice(start, end);

        // Assert
        price.Amount.Should().Be(baseRate * 2 * 1.5m);
    }

    [Fact]
    public void CalculatePrice_GivenInvalidDuration_ShouldThrowArgumentException()
    {
        // Arrange
        var tariff = Tariff.Create(Guid.NewGuid(), new Money(100m));
        var start = new DateTimeOffset(new DateTime(2026, 5, 11, 12, 0, 0), TimeSpan.Zero);
        var end = new DateTimeOffset(new DateTime(2026, 5, 11, 10, 0, 0), TimeSpan.Zero); // End before start

        // Act
        Action action = () => tariff.CalculatePrice(start, end);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("End time must be greater than start time.");
    }
}