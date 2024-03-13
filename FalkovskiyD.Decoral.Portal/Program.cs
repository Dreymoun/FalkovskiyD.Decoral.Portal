using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

// Добавляем сервисы локализации
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var supportedCultures = new[] { "en", "ru", "kk-KZ" };
var cultureInfo = supportedCultures.Select(c => new CultureInfo(c)).ToList();

// Конфигурируем опции локализации
builder.Services.Configure<RequestLocalizationOptions>(options =>
{

    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = cultureInfo;
    options.SupportedUICultures = cultureInfo;

    // Задаем культуру на основе куки
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider()
    };
});


var app = builder.Build();

// Конфигурируем промежуточное ПО локализации
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    // Определяем маршруты с поддержкой локализации
    endpoints.MapControllerRoute(
        name: "localized",
        pattern: "{culture=en}/{controller=Home}/{action=Index}/{id?}");

    // Перехватываем неправильные культуры и перенаправляем на культуру по умолчанию
    endpoints.MapGet("/{culture}/{*path}", context =>
    {
        var cultureRouteValue = context.Request.RouteValues["culture"]?.ToString();
        // Если не поддерживаемая культура, перенаправляем на культуру по умолчанию
        if (!supportedCultures.Contains(cultureRouteValue))
        {
            // Используйте асинхронный метод перенаправления
            context.Response.Redirect($"/en{context.Request.Path}");
        }
        return Task.CompletedTask;
    });
});

app.Run();


