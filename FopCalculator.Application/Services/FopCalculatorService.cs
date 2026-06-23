using FopCalculator.Application.Interfaces;
using FopCalculator.Application.Models;
using FopCalculator.Domain.Enums;
using FopCalculator.Domain.Exceptions;

namespace FopCalculator.Application.Services;

public sealed class FopCalculatorService : IFopCalculator
{
    private readonly ITaxRateProvider _taxRateProvider;

    public FopCalculatorService(ITaxRateProvider taxRateProvider)
    {
        _taxRateProvider = taxRateProvider;
    }

    public FopCalculationResult Calculate(FopCalculationRequest request)
    {
        if (request.AnnualIncome < 0)
            throw new InvalidIncomeException(request.AnnualIncome);

        var rates = _taxRateProvider.GetForYear(request.Year);

        return request.Group switch
        {
            FopGroup.First => CalculateGroup1(request, rates),
            FopGroup.Second => CalculateGroup2(request, rates),
            FopGroup.Third => CalculateGroup3(request, rates),
            _ => throw new ArgumentOutOfRangeException(nameof(request.Group))
        };
    }

    public ComparisonResult CompareAllGroups(decimal annualIncome, int year)
    {
        var g1 = Calculate(new FopCalculationRequest
        {
            AnnualIncome = annualIncome,
            Group = FopGroup.First,
            Year = year
        });
        var g2 = Calculate(new FopCalculationRequest
        {
            AnnualIncome = annualIncome,
            Group = FopGroup.Second,
            Year = year
        });
        var g3 = Calculate(new FopCalculationRequest
        {
            AnnualIncome = annualIncome,
            Group = FopGroup.Third,
            Year = year
        });

        return new ComparisonResult
        {
            AnnualIncome = annualIncome,
            Year = year,
            Group1 = g1,
            Group2 = g2,
            Group3 = g3
        };
    }

    private FopCalculationResult CalculateGroup1(
        FopCalculationRequest request,
        Domain.Entities.TaxRate rates)
    {
        var monthlyTax = rates.Group1MonthlyTax.Amount;
        var monthlyEsv = request.PayEsv ? rates.MonthlyEsv.Amount : 0;
        // ВЗ для 1-2 груп — фіксована сума від МЗП, незалежно від ЄСВ-перемикача
        var monthlyMilitaryTax = rates.MilitaryTaxGroup12Monthly.Amount;

        return new FopCalculationResult
        {
            Group = FopGroup.First,
            Year = request.Year,
            AnnualIncome = request.AnnualIncome,
            UnifiedTaxMonthly = monthlyTax,
            UnifiedTaxAnnual = monthlyTax * 12,
            UnifiedTaxRate = rates.Group1TaxPercent,
            EsvMonthly = monthlyEsv,
            EsvAnnual = monthlyEsv * 12,
            MilitaryTaxMonthly = monthlyMilitaryTax,
            MilitaryTaxAnnual = monthlyMilitaryTax * 12,
            MaxAllowedIncome = rates.Group1MaxIncome.Amount
        };
    }

    private FopCalculationResult CalculateGroup2(
        FopCalculationRequest request,
        Domain.Entities.TaxRate rates)
    {
        var monthlyTax = rates.Group2MonthlyTax.Amount;
        var monthlyEsv = request.PayEsv ? rates.MonthlyEsv.Amount : 0;
        var monthlyMilitaryTax = rates.MilitaryTaxGroup12Monthly.Amount;

        return new FopCalculationResult
        {
            Group = FopGroup.Second,
            Year = request.Year,
            AnnualIncome = request.AnnualIncome,
            UnifiedTaxMonthly = monthlyTax,
            UnifiedTaxAnnual = monthlyTax * 12,
            UnifiedTaxRate = rates.Group2TaxPercent,
            EsvMonthly = monthlyEsv,
            EsvAnnual = monthlyEsv * 12,
            MilitaryTaxMonthly = monthlyMilitaryTax,
            MilitaryTaxAnnual = monthlyMilitaryTax * 12,
            MaxAllowedIncome = rates.Group2MaxIncome.Amount
        };
    }

    private FopCalculationResult CalculateGroup3(
        FopCalculationRequest request,
        Domain.Entities.TaxRate rates)
    {
        var taxRate = request.TaxSystem == TaxSystem.Simplified5Percent
            ? rates.Group3Rate5Percent
            : rates.Group3Rate3Percent;

        var annualTax = Math.Round(request.AnnualIncome * taxRate / 100, 2);
        var monthlyEsv = request.PayEsv ? rates.MonthlyEsv.Amount : 0;

        // ВЗ для 3 групи — % від фактичного річного доходу, а не фіксована сума
        var annualMilitaryTax = Math.Round(request.AnnualIncome * rates.MilitaryTaxGroup3Percent / 100, 2);

        return new FopCalculationResult
        {
            Group = FopGroup.Third,
            Year = request.Year,
            AnnualIncome = request.AnnualIncome,
            UnifiedTaxMonthly = Math.Round(annualTax / 12, 2),
            UnifiedTaxAnnual = annualTax,
            UnifiedTaxRate = taxRate,
            EsvMonthly = monthlyEsv,
            EsvAnnual = monthlyEsv * 12,
            MilitaryTaxMonthly = Math.Round(annualMilitaryTax / 12, 2),
            MilitaryTaxAnnual = annualMilitaryTax,
            MaxAllowedIncome = rates.Group3MaxIncome.Amount
        };
    }
}