namespace FopCalculator.Domain.ValueObjects;

public readonly record struct TaxPeriod(int Year, int Month)
{
    public static TaxPeriod Current => new(DateTime.Now.Year, DateTime.Now.Month);

    public static TaxPeriod FromDate(DateTime date) => new(date.Year, date.Month);

    public bool IsValid => Year >= 2020 && Year <= 2030 && Month >= 1 && Month <= 12;

    public override string ToString() => $"{Month:D2}.{Year}";
}