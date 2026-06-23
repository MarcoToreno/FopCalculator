using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FopCalculator.Web.Pages;

public class CompareModel : PageModel
{
    public decimal InitialIncome { get; private set; } = 500000;

    public void OnGet([FromQuery] decimal income = 500000)
    {
        InitialIncome = income > 0 ? income : 500000;
    }
}