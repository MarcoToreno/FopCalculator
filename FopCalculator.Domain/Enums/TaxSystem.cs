namespace FopCalculator.Domain.Enums;

public enum TaxSystem
{
    /// <summary>5% без ПДВ</summary>
    Simplified5Percent,

    /// <summary>3% + ПДВ</summary>
    Simplified3PercentPlusPdv,

    /// <summary>Загальна система</summary>
    General
}