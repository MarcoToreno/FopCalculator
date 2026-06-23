using FopCalculator.Domain.Enums;
namespace FopCalculator.Application.Models;
public sealed class FopCalculationResult
{
    public FopGroup Group { get; init; }
    public int Year { get; init; }
    public decimal AnnualIncome { get; init; }
    // Єдиний податок
    public decimal UnifiedTaxMonthly { get; init; }
    public decimal UnifiedTaxAnnual { get; init; }
    public decimal UnifiedTaxRate { get; init; }
    // ЄСВ
    public decimal EsvMonthly { get; init; }
    public decimal EsvAnnual { get; init; }
    // Військовий збір (обов'язковий з 2025 року)
    public decimal MilitaryTaxMonthly { get; init; }
    public decimal MilitaryTaxAnnual { get; init; }
    // Підсумки
    public decimal TotalTaxAnnual => UnifiedTaxAnnual + EsvAnnual + MilitaryTaxAnnual;
    public decimal TotalTaxMonthly => UnifiedTaxMonthly + EsvMonthly + MilitaryTaxMonthly;
    public decimal NetIncome => AnnualIncome - TotalTaxAnnual;
    public decimal EffectiveRate => AnnualIncome > 0
        ? Math.Round(TotalTaxAnnual / AnnualIncome * 100, 1)
        : 0;
    // Ліміти
    public decimal MaxAllowedIncome { get; init; }
    public bool IsIncomeWithinLimit => AnnualIncome <= MaxAllowedIncome;
}