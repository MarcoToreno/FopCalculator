using FopCalculator.Domain.Enums;

namespace FopCalculator.Application.Models;

public sealed class FopCalculationRequest
{
    /// <summary>Річний дохід (грн)</summary>
    public decimal AnnualIncome { get; init; }

    /// <summary>Група ФОП</summary>
    public FopGroup Group { get; init; }

    /// <summary>Система оподаткування (тільки для 3 групи)</summary>
    public TaxSystem TaxSystem { get; init; } = TaxSystem.Simplified5Percent;

    /// <summary>Рік розрахунку</summary>
    public int Year { get; init; } = DateTime.Now.Year;

    /// <summary>Чи платити ЄСВ (за замовчуванням так)</summary>
    public bool PayEsv { get; init; } = true;
}