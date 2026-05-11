using FluentAssertions;
using Pricing.Domain.Entities;
using Pricing.Domain.Enums;
using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.Tests;

public class PriceRuleTests
{
    [Fact]
    public void CreatePeakHoursRule_GivenValidData_ShouldCreateRule()
    {
        // Arrange
        var start = new TimeOnly(16, 0);
        var end = new TimeOnly(20, 0);
        var multiplier = 1.5m;

        // Act
        var rule = PriceRule.CreatePeakHoursRule(start, end, multiplier);

        // Assert
        rule.Should().NotBeNull();
        rule.Type.Should().Be(RuleType.PeakHoursMultiplier);
        rule.Multiplier.Should().Be(multiplier);
        rule.StartTime.Should().Be(start);
        rule.EndTime.Should().Be(end);
        rule.DayOfWeek.Should().BeNull();
    }

    [Fact]
    public void CreateWeekendRule_GivenValidData_ShouldCreateRule()
    {
        // Arrange
        var multiplier = 1.2m;

        // Act
        var rule = PriceRule.CreateWeekendRule(multiplier);

        // Assert
        rule.Should().NotBeNull();
        rule.Type.Should().Be(RuleType.WeekendMultiplier);
        rule.Multiplier.Should().Be(multiplier);
        rule.DayOfWeek.Should().BeNull();
        rule.StartTime.Should().BeNull();
        rule.EndTime.Should().BeNull();
    }

    [Fact]
    public void CreateDayRule_GivenValidData_ShouldCreateRule()
    {
        // Arrange
        var day = DayOfWeek.Wednesday;
        var multiplier = 0.8m;

        // Act
        var rule = PriceRule.CreateDayRule(day, multiplier);

        // Assert
        rule.Should().NotBeNull();
        rule.Type.Should().Be(RuleType.DayMultiplier);
        rule.Multiplier.Should().Be(multiplier);
        rule.DayOfWeek.Should().Be(day);
        rule.StartTime.Should().BeNull();
        rule.EndTime.Should().BeNull();
    }

    [Fact]
    public void Apply_GiveWeekendRuleOnWeekend_ShouldMultiplyPrice()
    {
        // Arrange
        var rule = PriceRule.CreateWeekendRule(2m);
        var initialPrice = new Money(50m);
        // May 9th 2026 is a Saturday
        var start = new DateTimeOffset(new DateTime(2026, 5, 9, 10, 0, 0), TimeSpan.Zero);
        var end = start.AddHours(1);

        // Act
        var result = rule.Apply(initialPrice, start, end);

        // Assert
        result.Amount.Should().Be(100m);
    }

    [Fact]
    public void Apply_GiveWeekendRuleOnWeekday_ShouldNotChangePrice()
    {
        // Arrange
        var rule = PriceRule.CreateWeekendRule(2m);
        var initialPrice = new Money(50m);
        // May 11th 2026 is a Monday
        var start = new DateTimeOffset(new DateTime(2026, 5, 11, 10, 0, 0), TimeSpan.Zero);
        var end = start.AddHours(1);

        // Act
        var result = rule.Apply(initialPrice, start, end);

        // Assert
        result.Amount.Should().Be(50m);
    }

    [Fact]
    public void Apply_GiveDayRuleOnMatchingDay_ShouldMultiplyPrice()
    {
        // Arrange
        var rule = PriceRule.CreateDayRule(DayOfWeek.Monday, 0.5m);
        var initialPrice = new Money(100m);
        // May 11th 2026 is a Monday
        var start = new DateTimeOffset(new DateTime(2026, 5, 11, 10, 0, 0), TimeSpan.Zero);
        var end = start.AddHours(1);

        // Act
        var result = rule.Apply(initialPrice, start, end);

        // Assert
        result.Amount.Should().Be(50m);
    }

    [Fact]
    public void Apply_GiveDayRuleOnDifferentDay_ShouldNotChangePrice()
    {
        // Arrange
        var rule = PriceRule.CreateDayRule(DayOfWeek.Tuesday, 0.5m);
        var initialPrice = new Money(100m);
        // May 11th 2026 is a Monday
        var start = new DateTimeOffset(new DateTime(2026, 5, 11, 10, 0, 0), TimeSpan.Zero);
        var end = start.AddHours(1);

        // Act
        var result = rule.Apply(initialPrice, start, end);

        // Assert
        result.Amount.Should().Be(100m);
    }

    [Fact]
    public void Apply_GivePeakHoursRuleDuringPeak_ShouldMultiplyPrice()
    {
        // Arrange
        var rule = PriceRule.CreatePeakHoursRule(new TimeOnly(16, 0), new TimeOnly(20, 0), 1.5m);
        var initialPrice = new Money(100m);
        var start = new DateTimeOffset(new DateTime(2026, 5, 11, 17, 0, 0), TimeSpan.Zero);
        var end = start.AddHours(1);

        // Act
        var result = rule.Apply(initialPrice, start, end);

        // Assert
        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Apply_GivePeakHoursRuleOutsidePeak_ShouldNotChangePrice()
    {
        // Arrange
        var rule = PriceRule.CreatePeakHoursRule(new TimeOnly(16, 0), new TimeOnly(20, 0), 1.5m);
        var initialPrice = new Money(100m);
        var start = new DateTimeOffset(new DateTime(2026, 5, 11, 10, 0, 0), TimeSpan.Zero);
        var end = start.AddHours(1);

        // Act
        var result = rule.Apply(initialPrice, start, end);

        // Assert
        result.Amount.Should().Be(100m);
    }
}