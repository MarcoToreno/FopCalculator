using FluentAssertions;
using FopCalculator.Domain.Entities;

namespace FopCalculator.Tests.Domain;

public sealed class TaxRateTests
{
    private static TaxRate Create2026() => new TaxRate(
        year: 2026,
        minimumWage: 8647m,
        subsistenceMinimum: 3328m,
        militaryTaxGroup12Percent: 10m,
        militaryTaxGroup3Percent: 1m);

    [Fact]
    public void Group1MaxIncome_CalculatedCorrectly()
    {
        var rate = Create2026();

        // 8647 * 167 = 1,444,049
        rate.Group1MaxIncome.Amount.Should().Be(8647m * 167);
    }

    [Fact]
    public void Group2MaxIncome_CalculatedCorrectly()
    {
        var rate = Create2026();

        rate.Group2MaxIncome.Amount.Should().Be(8647m * 834);
    }

    [Fact]
    public void Group3MaxIncome_CalculatedCorrectly()
    {
        var rate = Create2026();

        rate.Group3MaxIncome.Amount.Should().Be(8647m * 1167);
    }

    [Fact]
    public void MonthlyEsv_CalculatedCorrectly()
    {
        var rate = Create2026();

        // 8647 * 22% = 1902.34
        rate.MonthlyEsv.Amount.Should().Be(Math.Round(8647m * 22m / 100m, 2));
    }

    [Fact]
    public void Group1MonthlyTax_BasedOnSubsistenceMinimum()
    {
        var rate = Create2026();

        // 3328 * 10% = 332.80
        rate.Group1MonthlyTax.Amount.Should().Be(Math.Round(3328m * 10m / 100m, 2));
    }

    [Fact]
    public void Group2MonthlyTax_BasedOnMinimumWage()
    {
        var rate = Create2026();

        // 8647 * 20% = 1729.40
        rate.Group2MonthlyTax.Amount.Should().Be(Math.Round(8647m * 20m / 100m, 2));
    }

    [Fact]
    public void MilitaryTaxGroup12Monthly_CalculatedCorrectly()
    {
        var rate = Create2026();

        // 8647 * 10% = 864.70
        rate.MilitaryTaxGroup12Monthly.Amount
            .Should().Be(Math.Round(8647m * 10m / 100m, 2));
    }
}