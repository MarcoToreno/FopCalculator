using FluentAssertions;
using FopCalculator.Infrastructure.Providers;

namespace FopCalculator.Tests.Application;

public sealed class TaxRateProviderTests
{
    private readonly TaxRateProvider _sut = new();

    [Fact]
    public void GetForYear_2026_ReturnsCorrectMinimumWage()
    {
        var rate = _sut.GetForYear(2026);

        rate.MinimumWage.Amount.Should().Be(8647m);
    }

    [Fact]
    public void GetForYear_2025_ReturnsCorrectMinimumWage()
    {
        var rate = _sut.GetForYear(2025);

        rate.MinimumWage.Amount.Should().Be(8000m);
    }

    [Fact]
    public void GetForYear_UnknownYear_ReturnsFallback()
    {
        var act = () => _sut.GetForYear(2099);

        // Не має кидати виняток — повертає найближчий рік
        act.Should().NotThrow();
    }

    [Fact]
    public void GetCurrent_ReturnsCurrentYear()
    {
        var rate = _sut.GetCurrent();

        rate.Year.Should().Be(_sut.GetForYear(DateTime.Now.Year).Year);
    }

    [Fact]
    public void GetAvailableYears_ContainsExpectedYears()
    {
        var years = _sut.GetAvailableYears();

        years.Should().Contain(2025);
        years.Should().Contain(2026);
    }

    [Fact]
    public void GetAvailableYears_IsOrderedDescending()
    {
        var years = _sut.GetAvailableYears();

        years.Should().BeInDescendingOrder();
    }
}