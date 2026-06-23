namespace FopCalculator.Application.Models;

public sealed class SalaryCalculationResult
{
    public decimal GrossSalary { get; init; }
    public decimal NetSalary { get; init; }
    public decimal Pdfo { get; init; }
    public decimal MilitaryTax { get; init; }
    public decimal EmployerEsv { get; init; }
    public decimal TotalCostForEmployer { get; init; }
    public decimal TotalDeductions => Pdfo + MilitaryTax;
    public decimal EffectiveRate => GrossSalary > 0
        ? Math.Round(TotalDeductions / GrossSalary * 100, 1)
        : 0;
}

public sealed class FopVsEmployeeResult
{
    public decimal Income { get; init; }
    public required SalaryCalculationResult AsEmployee { get; init; }
    public required FopCalculationResult AsFop { get; init; }
    public decimal FopAdvantage => AsFop.NetIncome - AsEmployee.NetSalary;
    public bool IsFopBetter => FopAdvantage > 0;
}