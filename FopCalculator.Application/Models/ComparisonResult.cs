namespace FopCalculator.Application.Models;

public sealed class ComparisonResult
{
    public decimal AnnualIncome { get; init; }
    public int Year { get; init; }
    public required FopCalculationResult Group1 { get; init; }
    public required FopCalculationResult Group2 { get; init; }
    public required FopCalculationResult Group3 { get; init; }

    public FopCalculationResult BestByNetIncome =>
        new[] { Group1, Group2, Group3 }
            .Where(g => g.IsIncomeWithinLimit)
            .MaxBy(g => g.NetIncome) ?? Group3;
}