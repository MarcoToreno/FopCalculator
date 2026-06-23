using FopCalculator.Domain.Entities;

namespace FopCalculator.Application.Interfaces;

public interface ITaxRateProvider
{
    TaxRate GetForYear(int year);
    TaxRate GetCurrent();
    IReadOnlyList<int> GetAvailableYears();
}
