namespace FopCalculator.Domain.Exceptions;

public sealed class IncomeLimitExceededException : DomainException
{
    public IncomeLimitExceededException(decimal income, decimal limit, int group)
        : base($"Дохід {income:N0} грн перевищує ліміт {limit:N0} грн для {group} групи ФОП.") { }
}