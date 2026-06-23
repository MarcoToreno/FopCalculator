using FopCalculator.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FopCalculator.Web.Filters;

/// <summary>
/// Глобально прокидує поточний податковий рік у ViewData["CurrentYear"]
/// для кожної Razor Page, щоб _Layout і всі сторінки брали рік
/// з одного джерела (TaxRateProvider), а не з DateTime.Now.Year напряму.
/// Якщо для поточного календарного року ще немає ставок у TaxRateProvider,
/// GetCurrent() сам впаде на найближчий доступний рік — і вся розмітка
/// автоматично покаже коректний рік, а не "вигаданий" майбутній.
/// </summary>
public sealed class CurrentYearPageFilter : IAsyncPageFilter
{
    private readonly ITaxRateProvider _taxRateProvider;

    public CurrentYearPageFilter(ITaxRateProvider taxRateProvider)
    {
        _taxRateProvider = taxRateProvider;
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        if (context.HandlerInstance is PageModel pageModel)
        {
            pageModel.ViewData["CurrentYear"] = _taxRateProvider.GetCurrent().Year;
        }

        await next();
    }
}