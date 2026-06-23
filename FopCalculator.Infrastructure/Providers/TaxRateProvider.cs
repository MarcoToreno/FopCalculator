using FopCalculator.Application.Interfaces;
using FopCalculator.Domain.Entities;
namespace FopCalculator.Infrastructure.Providers;
public sealed class TaxRateProvider : ITaxRateProvider
{
    // Джерела: Закон про Державний бюджет на відповідний рік, роз'яснення ДПС.
    // 2026: МЗП 8647 грн, ПМ 3328 грн (станом на 01.01.2026, фіксується на весь рік).
    private static readonly Dictionary<int, TaxRate> _rates = new()
    {
        [2024] = new TaxRate(
            year: 2024,
            minimumWage: 7100m,
            subsistenceMinimum: 3028m,
            militaryTaxRate: 1.5m,         // ВЗ для найманих у 2024 ще 1.5%
            militaryTaxGroup12Percent: 0m, // ВЗ для ФОП 1-2 груп діє з 2025 року
            militaryTaxGroup3Percent: 0m), // ВЗ для ФОП 3 групи діє з 2025 року

        [2025] = new TaxRate(
            year: 2025,
            minimumWage: 8000m,
            subsistenceMinimum: 3028m,
            militaryTaxRate: 5m,
            militaryTaxGroup12Percent: 10m,
            militaryTaxGroup3Percent: 1m),

        [2026] = new TaxRate(
            year: 2026,
            minimumWage: 8647m,
            subsistenceMinimum: 3328m,
            militaryTaxRate: 5m,
            militaryTaxGroup12Percent: 10m,
            militaryTaxGroup3Percent: 1m),
    };

    public TaxRate GetForYear(int year)
    {
        if (_rates.TryGetValue(year, out var rate))
            return rate;
        // Якщо рік не знайдено — беремо найближчий
        var closestYear = _rates.Keys
            .OrderBy(y => Math.Abs(y - year))
            .First();
        return _rates[closestYear];
    }
    public TaxRate GetCurrent() => GetForYear(DateTime.Now.Year);
    public IReadOnlyList<int> GetAvailableYears() =>
        _rates.Keys.OrderDescending().ToList();
}