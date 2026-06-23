using FopCalculator.Application.Interfaces;
using FopCalculator.Application.Models;
using FopCalculator.Domain.Enums;
namespace FopCalculator.Application.Services;
public sealed class SalaryCalculatorService : ISalaryCalculator
{
    private readonly ITaxRateProvider _taxRateProvider;
    private readonly IFopCalculator _fopCalculator;
    public SalaryCalculatorService(
        ITaxRateProvider taxRateProvider,
        IFopCalculator fopCalculator)
    {
        _taxRateProvider = taxRateProvider;
        _fopCalculator = fopCalculator;
    }
    public SalaryCalculationResult CalculateFromGross(decimal grossSalary, int year)
    {
        var rates = _taxRateProvider.GetForYear(year);
        var pdfo = Math.Round(grossSalary * rates.PdfoRate / 100, 2);
        var militaryTax = Math.Round(grossSalary * rates.MilitaryTaxRate / 100, 2);
        var employerEsv = Math.Round(grossSalary * rates.EsvRate / 100, 2);
        var netSalary = grossSalary - pdfo - militaryTax;
        return new SalaryCalculationResult
        {
            GrossSalary = grossSalary,
            NetSalary = netSalary,
            Pdfo = pdfo,
            MilitaryTax = militaryTax,
            EmployerEsv = employerEsv,
            TotalCostForEmployer = grossSalary + employerEsv
        };
    }
    public SalaryCalculationResult CalculateFromNet(decimal netSalary, int year)
    {
        var rates = _taxRateProvider.GetForYear(year);
        var totalRate = (rates.PdfoRate + rates.MilitaryTaxRate) / 100;
        var grossSalary = Math.Round(netSalary / (1 - totalRate), 2);
        return CalculateFromGross(grossSalary, year);
    }
    public FopVsEmployeeResult CompareFopVsEmployee(decimal income, int year)
    {
        var asEmployee = CalculateFromGross(income, year);
        var asFop = _fopCalculator.Calculate(new FopCalculationRequest
        {
            AnnualIncome = income,
            Group = FopGroup.Third,
            Year = year,
            TaxSystem = TaxSystem.Simplified5Percent
        });
        return new FopVsEmployeeResult
        {
            Income = income,
            AsEmployee = asEmployee,
            AsFop = asFop
        };
    }
}