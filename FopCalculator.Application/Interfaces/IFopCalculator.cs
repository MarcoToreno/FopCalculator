using FopCalculator.Application.Models;
using FopCalculator.Domain.Enums;

namespace FopCalculator.Application.Interfaces;

public interface IFopCalculator
{
    FopCalculationResult Calculate(FopCalculationRequest request);
    ComparisonResult CompareAllGroups(decimal annualIncome, int year);
}