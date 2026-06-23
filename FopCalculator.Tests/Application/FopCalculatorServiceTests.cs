using FluentAssertions;
using FopCalculator.Application.Interfaces;
using FopCalculator.Application.Models;
using FopCalculator.Application.Services;
using FopCalculator.Domain.Entities;
using FopCalculator.Domain.Enums;
using FopCalculator.Domain.Exceptions;
using NSubstitute;

namespace FopCalculator.Tests.Application;

public sealed class FopCalculatorServiceTests
{
    private readonly ITaxRateProvider _taxRateProvider;
    private readonly FopCalculatorService _sut;

    // Тестова ставка 2026
    private static readonly TaxRate TestRate = new TaxRate(
        year: 2026,
        minimumWage: 8647m,
        subsistenceMinimum: 3328m,
        militaryTaxGroup12Percent: 10m,
        militaryTaxGroup3Percent: 1m);

    public FopCalculatorServiceTests()
    {
        _taxRateProvider = Substitute.For<ITaxRateProvider>();
        _taxRateProvider.GetForYear(Arg.Any<int>()).Returns(TestRate);
        _sut = new FopCalculatorService(_taxRateProvider);
    }

    // ── GROUP 1 ────────────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_Group1_ReturnsFixedTax()
    {
        var request = new FopCalculationRequest
        {
            AnnualIncome = 500_000m,
            Group = FopGroup.First,
            Year = 2026
        };

        var result = _sut.Calculate(request);

        result.Group.Should().Be(FopGroup.First);
        result.UnifiedTaxMonthly.Should().Be(TestRate.Group1MonthlyTax.Amount);
        result.UnifiedTaxAnnual.Should().Be(TestRate.Group1MonthlyTax.Amount * 12);
    }

    [Fact]
    public void Calculate_Group1_IncludesEsvWhenEnabled()
    {
        var request = new FopCalculationRequest
        {
            AnnualIncome = 500_000m,
            Group = FopGroup.First,
            PayEsv = true,
            Year = 2026
        };

        var result = _sut.Calculate(request);

        result.EsvAnnual.Should().Be(TestRate.MonthlyEsv.Amount * 12);
        result.EsvMonthly.Should().Be(TestRate.MonthlyEsv.Amount);
    }

    [Fact]
    public void Calculate_Group1_ExcludesEsvWhenDisabled()
    {
        var request = new FopCalculationRequest
        {
            AnnualIncome = 500_000m,
            Group = FopGroup.First,
            PayEsv = false,
            Year = 2026
        };

        var result = _sut.Calculate(request);

        result.EsvAnnual.Should().Be(0);
        result.EsvMonthly.Should().Be(0);
    }

    [Fact]
    public void Calculate_Group1_ExceedsLimit_IsWithinLimitFalse()
    {
        var overLimitIncome = TestRate.Group1MaxIncome.Amount + 1;

        var request = new FopCalculationRequest
        {
            AnnualIncome = overLimitIncome,
            Group = FopGroup.First,
            Year = 2026
        };

        var result = _sut.Calculate(request);

        result.IsIncomeWithinLimit.Should().BeFalse();
        result.MaxAllowedIncome.Should().Be(TestRate.Group1MaxIncome.Amount);
    }

    // ── GROUP 2 ────────────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_Group2_ReturnsFixedTax()
    {
        var request = new FopCalculationRequest
        {
            AnnualIncome = 2_000_000m,
            Group = FopGroup.Second,
            Year = 2026
        };

        var result = _sut.Calculate(request);

        result.Group.Should().Be(FopGroup.Second);
        result.UnifiedTaxMonthly.Should().Be(TestRate.Group2MonthlyTax.Amount);
        result.UnifiedTaxAnnual.Should().Be(TestRate.Group2MonthlyTax.Amount * 12);
    }

    [Fact]
    public void Calculate_Group2_NetIncomeCalculatedCorrectly()
    {
        var income = 3_000_000m;
        var request = new FopCalculationRequest
        {
            AnnualIncome = income,
            Group = FopGroup.Second,
            PayEsv = true,
            Year = 2026
        };

        var result = _sut.Calculate(request);

        result.NetIncome.Should().Be(income - result.TotalTaxAnnual);
    }

    // ── GROUP 3 ────────────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_Group3_5Percent_CalculatesFromIncome()
    {
        var income = 1_000_000m;
        var request = new FopCalculationRequest
        {
            AnnualIncome = income,
            Group = FopGroup.Third,
            TaxSystem = TaxSystem.Simplified5Percent,
            PayEsv = false,
            Year = 2026
        };

        var result = _sut.Calculate(request);

        result.UnifiedTaxAnnual.Should().Be(50_000m); // 1M * 5%
        result.UnifiedTaxRate.Should().Be(5m);
    }

    [Fact]
    public void Calculate_Group3_3Percent_CalculatesFromIncome()
    {
        var income = 1_000_000m;
        var request = new FopCalculationRequest
        {
            AnnualIncome = income,
            Group = FopGroup.Third,
            TaxSystem = TaxSystem.Simplified3PercentPlusPdv,
            PayEsv = false,
            Year = 2026
        };

        var result = _sut.Calculate(request);

        result.UnifiedTaxAnnual.Should().Be(30_000m); // 1M * 3%
        result.UnifiedTaxRate.Should().Be(3m);
    }

    [Fact]
    public void Calculate_Group3_EffectiveRateIsCorrect()
    {
        var income = 1_000_000m;
        var request = new FopCalculationRequest
        {
            AnnualIncome = income,
            Group = FopGroup.Third,
            TaxSystem = TaxSystem.Simplified5Percent,
            PayEsv = true,
            Year = 2026
        };

        var result = _sut.Calculate(request);

        var expectedRate = Math.Round(result.TotalTaxAnnual / income * 100, 1);
        result.EffectiveRate.Should().Be(expectedRate);
    }

    // ── VALIDATION ─────────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_NegativeIncome_ThrowsInvalidIncomeException()
    {
        var request = new FopCalculationRequest
        {
            AnnualIncome = -1m,
            Group = FopGroup.Third,
            Year = 2026
        };

        var act = () => _sut.Calculate(request);

        act.Should().Throw<InvalidIncomeException>();
    }

    [Fact]
    public void Calculate_ZeroIncome_ReturnsZeroNetIncome()
    {
        var request = new FopCalculationRequest
        {
            AnnualIncome = 0m,
            Group = FopGroup.Third,
            PayEsv = false,
            Year = 2026
        };

        var result = _sut.Calculate(request);

        result.UnifiedTaxAnnual.Should().Be(0m);
    }

    // ── COMPARE ────────────────────────────────────────────────────────────────

    [Fact]
    public void CompareAllGroups_ReturnsAllThreeGroups()
    {
        var result = _sut.CompareAllGroups(1_000_000m, 2026);

        result.Group1.Should().NotBeNull();
        result.Group2.Should().NotBeNull();
        result.Group3.Should().NotBeNull();
        result.AnnualIncome.Should().Be(1_000_000m);
    }

    [Fact]
    public void CompareAllGroups_BestByNetIncome_IsNeverNull()
    {
        var result = _sut.CompareAllGroups(500_000m, 2026);

        result.BestByNetIncome.Should().NotBeNull();
    }

    [Fact]
    public void CompareAllGroups_HighIncome_Group1And2AreOutOfLimit()
    {
        // Дохід більший за ліміт 1 і 2 групи
        var veryHighIncome = TestRate.Group2MaxIncome.Amount + 1;

        var result = _sut.CompareAllGroups(veryHighIncome, 2026);

        result.Group1.IsIncomeWithinLimit.Should().BeFalse();
        result.Group2.IsIncomeWithinLimit.Should().BeFalse();
        result.Group3.IsIncomeWithinLimit.Should().BeTrue();
        result.BestByNetIncome.Group.Should().Be(FopGroup.Third);
    }
}
