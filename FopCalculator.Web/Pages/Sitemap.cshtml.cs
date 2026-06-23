using FopCalculator.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FopCalculator.Web.Pages;

public class SitemapModel : PageModel
{
    private readonly ITaxRateProvider _taxRateProvider;

    public SitemapModel(ITaxRateProvider taxRateProvider)
    {
        _taxRateProvider = taxRateProvider;
    }

    public IActionResult OnGet()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var urls = new[]
        {
            new { Loc = $"{baseUrl}/",        Priority = "1.0", ChangeFreq = "monthly" },
            new { Loc = $"{baseUrl}/compare", Priority = "0.9", ChangeFreq = "monthly" },
            new { Loc = $"{baseUrl}/salary",  Priority = "0.9", ChangeFreq = "monthly" },
        };

        var xml = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
            {string.Join("\n", urls.Select(u => $"""
                <url>
                    <loc>{u.Loc}</loc>
                    <lastmod>{today}</lastmod>
                    <changefreq>{u.ChangeFreq}</changefreq>
                    <priority>{u.Priority}</priority>
                </url>
            """))}
            </urlset>
            """;

        return Content(xml, "application/xml");
    }
}