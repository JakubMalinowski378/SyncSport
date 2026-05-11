using FluentAssertions;
using Pricing.Domain.ValueObjects;

namespace Pricing.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Constructor_GivenPositiveAmount_ShouldSetAmount()
    {
        // Act
        var money = new Money(100.5m);

        // Assert
        money.Amount.Should().Be(100.5m);
    }

    [Fact]
    public void Constructor_GivenNegativeAmount_ShouldThrowArgumentException()
    {
        // Act
        Action action = () => new Money(-1m);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Amount cannot be negative (Parameter 'amount')");
    }

    [Fact]
    public void Zero_ShouldReturnMoneyWithZeroAmount()
    {
        // Act
        var money = Money.Zero();

        // Assert
        money.Amount.Should().Be(0m);
    }

    [Fact]
    public void Add_GivenAnotherMoney_ShouldReturnNewMoneyWithSum()
    {
        // Arrange
        var moneyA = new Money(50m);
        var moneyB = new Money(25.5m);

        // Act
        var result = moneyA.Add(moneyB);

        // Assert
        result.Amount.Should().Be(75.5m);
    }

    [Fact]
    public void Multiply_GivenMultiplier_ShouldReturnNewMoneyWithProduct()
    {
        // Arrange
        var money = new Money(50m);

        // Act
        var result = money.Multiply(1.5m);

        // Assert
        result.Amount.Should().Be(75m);
    }

    [Fact]
    public void AdditionOperator_GivenTwoMonies_ShouldReturnSum()
    {
        // Arrange
        var moneyA = new Money(50m);
        var moneyB = new Money(25m);

        // Act
        var result = moneyA + moneyB;

        // Assert
        result.Amount.Should().Be(75m);
    }

    [Fact]
    public void MultiplicationOperator_GivenMoneyAndDecimal_ShouldReturnProduct()
    {
        // Arrange
        var money = new Money(50m);

        // Act
        var result = money * 2m;

        // Assert
        result.Amount.Should().Be(100m);
    }

    [Fact]
    public void Equality_GivenSameAmount_ShouldBeEqual()
    {
        // Arrange
        var moneyA = new Money(50m);
        var moneyB = new Money(50m);

        // Act & Assert
        moneyA.Should().Be(moneyB);
        moneyA.GetHashCode().Should().Be(moneyB.GetHashCode());
    }
}