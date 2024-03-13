using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

// ��������� ������� �����������
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var supportedCultures = new[] { "en", "ru", "kk-KZ" };
var cultureInfo = supportedCultures.Select(c => new CultureInfo(c)).ToList();

// ������������� ����� �����������
builder.Services.Configure<RequestLocalizationOptions>(options =>
{

    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = cultureInfo;
    options.SupportedUICultures = cultureInfo;

    // ������ �������� �� ������ ����
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider()
    };
});


var app = builder.Build();

// ������������� ������������� �� �����������
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
    // ���������� �������� � ���������� �����������
    endpoints.MapControllerRoute(
        name: "localized",
        pattern: "{culture=en}/{controller=Home}/{action=Index}/{id?}");

    // ������������� ������������ �������� � �������������� �� �������� �� ���������
    endpoints.MapGet("/{culture}/{*path}", context =>
    {
        var cultureRouteValue = context.Request.RouteValues["culture"]?.ToString();
        // ���� �� �������������� ��������, �������������� �� �������� �� ���������
        if (!supportedCultures.Contains(cultureRouteValue))
        {
            // ����������� ����������� ����� ���������������
            context.Response.Redirect($"/en{context.Request.Path}");
        }
        return Task.CompletedTask;
    });
});

app.Run();


