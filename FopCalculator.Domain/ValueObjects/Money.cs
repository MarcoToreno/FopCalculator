namespace FopCalculator.Domain.ValueObjects;

public readonly record struct Money(decimal Amount, string Currency = "UAH")
{
    public static Money Zero => new(0);

    public static Money FromUah(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Сума не може бути від'ємною", nameof(amount));
        return new Money(amount);
    }

    public Money Add(Money other) => new(Amount + other.Amount, Currency);
    public Money Subtract(Money other) => new(Amount - other.Amount, Currency);
    public Money Multiply(decimal factor) => new(Amount * factor, Currency);
    public Money Percent(decimal percent) => new(Amount * percent / 100, Currency);

    public override string ToString() => $"{Amount:N2} {Currency}";
    public string ToUahString() => $"{Amount:N0} грн";
}