namespace FopCalculator.Domain.Exceptions;

public sealed class InvalidIncomeException : DomainException
{
    public InvalidIncomeException(decimal income)
        : base($"Некоректний дохід: {income:N2} грн. Дохід не може бути від'ємним.") { }
}