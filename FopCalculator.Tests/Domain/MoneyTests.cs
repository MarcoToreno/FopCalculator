using FluentAssertions;
using FopCalculator.Domain.ValueObjects;

namespace FopCalculator.Tests.Domain;

public sealed class MoneyTests
{
    [Fact]
    public void FromUah_ValidAmount_CreatesCorrectly()
    {
        var money = Money.FromUah(1000m);

        money.Amount.Should().Be(1000m);
        money.Currency.Should().Be("UAH");
    }

    [Fact]
    public void FromUah_NegativeAmount_ThrowsArgumentException()
    {
        var act = () => Money.FromUah(-1m);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*від'ємною*");
    }

    [Fact]
    public void Zero_ReturnsZeroAmount()
    {
        Money.Zero.Amount.Should().Be(0m);
    }

    [Theory]
    [InlineData(1000, 500, 1500)]
    [InlineData(0, 0, 0)]
    [InlineData(100, 200, 300)]
    public void Add_TwoAmounts_ReturnsCorrectSum(
        decimal a, decimal b, decimal expected)
    {
        var result = Money.FromUah(a).Add(Money.FromUah(b));

        result.Amount.Should().Be(expected);
    }

    [Theory]
    [InlineData(1000, 10, 100)]
    [InlineData(8000, 22, 1760)]
    [InlineData(3028, 10, 302.8)]
    public void Percent_CalculatesCorrectly(
        decimal amount, decimal percent, decimal expected)
    {
        var result = Money.FromUah(amount).Percent(percent);

        result.Amount.Should().Be(expected);
    }
}