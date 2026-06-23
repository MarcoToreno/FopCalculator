using FopCalculator.Application.Interfaces;
using FopCalculator.Application.Models;
using FopCalculator.Domain.Enums;
using FopCalculator.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FopCalculator.Web.Controllers;

[ApiController]
[Route("api/fop")]
public sealed class FopApiController : ControllerBase
{
    private readonly IFopCalculator _calculator;
    private readonly ISalaryCalculator _salaryCalculator;
    private readonly ITaxRateProvider _taxRateProvider;

    public FopApiController(
        IFopCalculator calculator,
        ISalaryCalculator salaryCalculator,
        ITaxRateProvider taxRateProvider)
    {
        _calculator = calculator;
        _salaryCalculator = salaryCalculator;
        _taxRateProvider = taxRateProvider;
    }

    private int ResolveYear(int? year) => year ?? _taxRateProvider.GetCurrent().Year;

    [HttpGet("years")]
    public IActionResult GetAvailableYears()
    {
        return Ok(_taxRateProvider.GetAvailableYears());
    }

    [HttpGet("calculate")]
    public IActionResult Calculate(
        [FromQuery] decimal annualIncome,
        [FromQuery] int group,
        [FromQuery] int taxSystem = 0,
        [FromQuery] bool payEsv = true,
        [FromQuery] int? year = null)
    {
        if (annualIncome < 0)
            return BadRequest("Дохід не може бути від'ємним");

        try
        {
            var request = new FopCalculationRequest
            {
                AnnualIncome = annualIncome,
                Group = (FopGroup)group,
                TaxSystem = (TaxSystem)taxSystem,
                PayEsv = payEsv,
                Year = ResolveYear(year)
            };

            var result = _calculator.Calculate(request);

            return Ok(new
            {
                group = (int)result.Group,
                year = result.Year,
                annualIncome = result.AnnualIncome,
                unifiedTaxMonthly = result.UnifiedTaxMonthly,
                unifiedTaxAnnual = result.UnifiedTaxAnnual,
                unifiedTaxRate = result.UnifiedTaxRate,
                esvMonthly = result.EsvMonthly,
                esvAnnual = result.EsvAnnual,
                militaryTaxMonthly = result.MilitaryTaxMonthly,
                militaryTaxAnnual = result.MilitaryTaxAnnual,
                totalTaxAnnual = result.TotalTaxAnnual,
                totalTaxMonthly = result.TotalTaxMonthly,
                netIncome = result.NetIncome,
                effectiveRate = result.EffectiveRate,
                maxAllowedIncome = result.MaxAllowedIncome,
                isWithinLimit = result.IsIncomeWithinLimit
            });
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("salary")]
    public IActionResult CalculateSalary(
        [FromQuery] decimal amount,
        [FromQuery] string mode = "gross",
        [FromQuery] int? year = null)
    {
        if (amount <= 0)
            return BadRequest("Сума має бути більше 0");

        try
        {
            var resolvedYear = ResolveYear(year);

            var result = mode == "gross"
                ? _salaryCalculator.CalculateFromGross(amount, resolvedYear)
                : _salaryCalculator.CalculateFromNet(amount, resolvedYear);

            return Ok(new
            {
                grossSalary = result.GrossSalary,
                netSalary = result.NetSalary,
                pdfo = result.Pdfo,
                militaryTax = result.MilitaryTax,
                employerEsv = result.EmployerEsv,
                totalCostForEmployer = result.TotalCostForEmployer,
                totalDeductions = result.TotalDeductions,
                effectiveRate = result.EffectiveRate
            });
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("fop-vs-employee")]
    public IActionResult CompareFopVsEmployee(
        [FromQuery] decimal income,
        [FromQuery] int? year = null)
    {
        if (income <= 0)
            return BadRequest("Дохід має бути більше 0");

        try
        {
            var result = _salaryCalculator.CompareFopVsEmployee(income, ResolveYear(year));

            return Ok(new
            {
                income = result.Income,
                isFopBetter = result.IsFopBetter,
                fopAdvantage = result.FopAdvantage,
                asEmployee = new
                {
                    grossSalary = result.AsEmployee.GrossSalary,
                    netSalary = result.AsEmployee.NetSalary,
                    pdfo = result.AsEmployee.Pdfo,
                    militaryTax = result.AsEmployee.MilitaryTax,
                    employerEsv = result.AsEmployee.EmployerEsv,
                    totalCostForEmployer = result.AsEmployee.TotalCostForEmployer,
                    effectiveRate = result.AsEmployee.EffectiveRate
                },
                asFop = new
                {
                    annualIncome = result.AsFop.AnnualIncome,
                    unifiedTaxAnnual = result.AsFop.UnifiedTaxAnnual,
                    esvAnnual = result.AsFop.EsvAnnual,
                    militaryTaxAnnual = result.AsFop.MilitaryTaxAnnual,
                    totalTaxAnnual = result.AsFop.TotalTaxAnnual,
                    netIncome = result.AsFop.NetIncome,
                    effectiveRate = result.AsFop.EffectiveRate
                }
            });
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("compare")]
    public IActionResult CompareGroups(
        [FromQuery] decimal annualIncome,
        [FromQuery] int? year = null)
    {
        if (annualIncome < 0)
            return BadRequest("Дохід не може бути від'ємним");

        try
        {
            var result = _calculator.CompareAllGroups(annualIncome, ResolveYear(year));

            return Ok(new
            {
                annualIncome = result.AnnualIncome,
                year = result.Year,
                bestGroupNumber = (int)result.BestByNetIncome.Group,
                group1 = MapResult(result.Group1),
                group2 = MapResult(result.Group2),
                group3 = MapResult(result.Group3),
            });
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }

        static object MapResult(FopCalculationResult r) => new
        {
            group = (int)r.Group,
            unifiedTaxMonthly = r.UnifiedTaxMonthly,
            unifiedTaxAnnual = r.UnifiedTaxAnnual,
            unifiedTaxRate = r.UnifiedTaxRate,
            esvAnnual = r.EsvAnnual,
            militaryTaxAnnual = r.MilitaryTaxAnnual,
            totalTaxAnnual = r.TotalTaxAnnual,
            netIncome = r.NetIncome,
            effectiveRate = r.EffectiveRate,
            maxAllowedIncome = r.MaxAllowedIncome,
            isWithinLimit = r.IsIncomeWithinLimit
        };
    }
}