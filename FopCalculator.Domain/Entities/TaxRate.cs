using FopCalculator.Domain.Enums;
using FopCalculator.Domain.ValueObjects;
namespace FopCalculator.Domain.Entities;
/// <summary>
/// Податкові ставки на конкретний рік
/// </summary>
public sealed class TaxRate
{
    public int Year { get; }
    /// <summary>Мінімальна зарплата (грн)</summary>
    public Money MinimumWage { get; }
    /// <summary>Прожитковий мінімум для працездатних осіб (грн)</summary>
    public Money SubsistenceMinimum { get; }
    /// <summary>Ставка ЄСВ (%)</summary>
    public decimal EsvRate { get; }
    /// <summary>Ставка ПДФО (%) — для найманих працівників</summary>
    public decimal PdfoRate { get; }
    /// <summary>Ставка військового збору (%) — для найманих працівників</summary>
    public decimal MilitaryTaxRate { get; }
    /// <summary>Ліміт доходу 1 групи (в мінімальних зарплатах)</summary>
    public int Group1IncomeLimit { get; }
    /// <summary>Ліміт доходу 2 групи (в мінімальних зарплатах)</summary>
    public int Group2IncomeLimit { get; }
    /// <summary>Ліміт доходу 3 групи (в мінімальних зарплатах)</summary>
    public int Group3IncomeLimit { get; }
    /// <summary>Фіксована ставка єдиного податку 1 групи (% від прожиткового мінімуму)</summary>
    public decimal Group1TaxPercent { get; }
    /// <summary>Фіксована ставка єдиного податку 2 групи (% від мінімальної зарплати)</summary>
    public decimal Group2TaxPercent { get; }
    /// <summary>Ставка єдиного податку 3 групи без ПДВ (%)</summary>
    public decimal Group3Rate5Percent { get; }
    /// <summary>Ставка єдиного податку 3 групи з ПДВ (%)</summary>
    public decimal Group3Rate3Percent { get; }
    /// <summary>Ставка військового збору для 1 і 2 груп (% від мінімальної зарплати, фіксована сума на місяць)</summary>
    public decimal MilitaryTaxGroup12Percent { get; }
    /// <summary>Ставка військового збору для 3 групи (% від доходу)</summary>
    public decimal MilitaryTaxGroup3Percent { get; }

    public TaxRate(
        int year,
        decimal minimumWage,
        decimal subsistenceMinimum,
        decimal esvRate = 22m,
        decimal pdfoRate = 18m,
        decimal militaryTaxRate = 5m,
        int group1IncomeLimit = 167,
        int group2IncomeLimit = 834,
        int group3IncomeLimit = 1167,
        decimal group1TaxPercent = 10m,
        decimal group2TaxPercent = 20m,
        decimal group3Rate5Percent = 5m,
        decimal group3Rate3Percent = 3m,
        decimal militaryTaxGroup12Percent = 10m,
        decimal militaryTaxGroup3Percent = 1m)
    {
        Year = year;
        MinimumWage = Money.FromUah(minimumWage);
        SubsistenceMinimum = Money.FromUah(subsistenceMinimum);
        EsvRate = esvRate;
        PdfoRate = pdfoRate;
        MilitaryTaxRate = militaryTaxRate;
        Group1IncomeLimit = group1IncomeLimit;
        Group2IncomeLimit = group2IncomeLimit;
        Group3IncomeLimit = group3IncomeLimit;
        Group1TaxPercent = group1TaxPercent;
        Group2TaxPercent = group2TaxPercent;
        Group3Rate5Percent = group3Rate5Percent;
        Group3Rate3Percent = group3Rate3Percent;
        MilitaryTaxGroup12Percent = militaryTaxGroup12Percent;
        MilitaryTaxGroup3Percent = militaryTaxGroup3Percent;
    }

    // Розраховані поля — ліміти доходу
    public Money Group1MaxIncome => Money.FromUah(MinimumWage.Amount * Group1IncomeLimit);
    public Money Group2MaxIncome => Money.FromUah(MinimumWage.Amount * Group2IncomeLimit);
    public Money Group3MaxIncome => Money.FromUah(MinimumWage.Amount * Group3IncomeLimit);

    // Єдиний податок (фіксований для 1-2 груп)
    public Money Group1MonthlyTax => Money.FromUah(
        Math.Round(SubsistenceMinimum.Amount * Group1TaxPercent / 100, 2));
    public Money Group2MonthlyTax => Money.FromUah(
        Math.Round(MinimumWage.Amount * Group2TaxPercent / 100, 2));

    // ЄСВ — однаковий мінімальний платіж для всіх груп
    public Money MonthlyEsv => Money.FromUah(
        Math.Round(MinimumWage.Amount * EsvRate / 100, 2));

    // Військовий збір ФОП: для 1-2 груп фіксована сума від мінімальної зарплати,
    // для 3 групи — відсоток від фактичного доходу (рахується окремо в сервісі)
    public Money MilitaryTaxGroup12Monthly => Money.FromUah(
        Math.Round(MinimumWage.Amount * MilitaryTaxGroup12Percent / 100, 2));
}