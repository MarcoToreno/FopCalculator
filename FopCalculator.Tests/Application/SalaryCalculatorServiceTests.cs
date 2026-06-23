using FluentAssertions;
using FopCalculator.Application.Interfaces;
using FopCalculator.Application.Services;
using FopCalculator.Domain.Entities;
using NSubstitute;

namespace FopCalculator.Tests.Application;

public sealed class SalaryCalculatorServiceTests
{
    private readonly ITaxRateProvider _taxRateProvider;
    private readonly SalaryCalculatorService _sut;

    private static readonly TaxRate TestRate = new TaxRate(
        year: 2026,
        minimumWage: 8647m,
        subsistenceMinimum: 3328m,
        pdfoRate: 18m,
        militaryTaxRate: 5m,
        esvRate: 22m);

    public SalaryCalculatorServiceTests()
    {
        _taxRateProvider = Substitute.For<ITaxRateProvider>();
        _taxRateProvider.GetForYear(Arg.Any<int>()).Returns(TestRate);

        var fopCalculator = new FopCalculatorService(_taxRateProvider);
        _sut = new SalaryCalculatorService(_taxRateProvider, fopCalculator);
    }

    // ── GROSS → NET ────────────────────────────────────────────────────────────

    [Fact]
    public void CalculateFromGross_CorrectlyCalculatesPdfo()
    {
        var result = _sut.CalculateFromGross(100_000m, 2026);

        result.Pdfo.Should().Be(18_000m); // 100k * 18%
    }

    [Fact]
    public void CalculateFromGross_CorrectlyCalculatesMilitaryTax()
    {
        var result = _sut.CalculateFromGross(100_000m, 2026);

        result.MilitaryTax.Should().Be(5_000m); // 100k * 5%
    }

    [Fact]
    public void CalculateFromGross_CorrectlyCalculatesNetSalary()
    {
        var result = _sut.CalculateFromGross(100_000m, 2026);

        // 100k - 18k ПДФО - 5k ВЗ = 77k
        result.NetSalary.Should().Be(77_000m);
    }

    [Fact]
    public void CalculateFromGross_CorrectlyCalculatesEmployerEsv()
    {
        var result = _sut.CalculateFromGross(100_000m, 2026);

        result.EmployerEsv.Should().Be(22_000m); // 100k * 22%
    }

    [Fact]
    public void CalculateFromGross_TotalCostForEmployer_IsGrossPlusEsv()
    {
        var result = _sut.CalculateFromGross(100_000m, 2026);

        result.TotalCostForEmployer.Should().Be(122_000m); // 100k + 22k
    }

    [Fact]
    public void CalculateFromGross_TotalDeductions_IsPdfoPlusMilitaryTax()
    {
        var result = _sut.CalculateFromGross(100_000m, 2026);

        result.TotalDeductions.Should().Be(result.Pdfo + result.MilitaryTax);
    }

    // ── NET → GROSS ────────────────────────────────────────────────────────────

    [Fact]
    public void CalculateFromNet_ReturnsCorrectGross()
    {
        // Net 77k → має дати Gross ~100k
        var result = _sut.CalculateFromNet(77_000m, 2026);

        result.NetSalary.Should().BeApproximately(77_000m, 1m);
    }

    [Fact]
    public void CalculateFromGross_ThenFromNet_AreConsistent()
    {
        var fromGross = _sut.CalculateFromGross(50_000m, 2026);
        var fromNet = _sut.CalculateFromNet(fromGross.NetSalary, 2026);

        fromNet.GrossSalary.Should().BeApproximately(50_000m, 1m);
    }

    // ── FOP VS EMPLOYEE ────────────────────────────────────────────────────────

    [Fact]
    public void CompareFopVsEmployee_HighIncome_FopIsBetter()
    {
        // При великому доході ФОП (5% + ЄСВ) вигідніший за найманця (18% + 5%)
        var result = _sut.CompareFopVsEmployee(1_000_000m, 2026);

        result.IsFopBetter.Should().BeTrue();
    }

    [Fact]
    public void CompareFopVsEmployee_FopAdvantage_IsPositiveWhenFopBetter()
    {
        var result = _sut.CompareFopVsEmployee(1_000_000m, 2026);

        if (result.IsFopBetter)
            result.FopAdvantage.Should().BePositive();
        else
            result.FopAdvantage.Should().BeNegative();
    }

    [Fact]
    public void CompareFopVsEmployee_ResultContainsBothScenarios()
    {
        var result = _sut.CompareFopVsEmployee(500_000m, 2026);

        result.AsEmployee.Should().NotBeNull();
        result.AsFop.Should().NotBeNull();
        result.Income.Should().Be(500_000m);
    }
}