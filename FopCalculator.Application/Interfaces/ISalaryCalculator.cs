using FopCalculator.Application.Models;

namespace FopCalculator.Application.Interfaces;

public interface ISalaryCalculator
{
    SalaryCalculationResult CalculateFromGross(decimal grossSalary, int year);
    SalaryCalculationResult CalculateFromNet(decimal netSalary, int year);
    FopVsEmployeeResult CompareFopVsEmployee(decimal income, int year);
}