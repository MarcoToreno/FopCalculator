using FopCalculator.Application.Interfaces;
using FopCalculator.Application.Services;
using FopCalculator.Infrastructure.Providers;
using FopCalculator.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/fop-calculator-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// ← реєструємо фільтр як сервіс
builder.Services.AddScoped<CurrentYearPageFilter>();

builder.Services.AddRazorPages(options =>
{
    // ← правильний спосіб для Razor Pages
    options.Conventions.AddFolderApplicationModelConvention(
        "/",
        model => model.Filters.Add(
            new ServiceFilterAttribute(typeof(CurrentYearPageFilter))));
});

builder.Services.AddControllers();

builder.Services.AddSingleton<ITaxRateProvider, TaxRateProvider>();
builder.Services.AddScoped<IFopCalculator, FopCalculatorService>();
builder.Services.AddScoped<ISalaryCalculator, SalaryCalculatorService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapControllers();

app.Run();