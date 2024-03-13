using FalkovskiyD.Decoral.Portal.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FalkovskiyD.Decoral.Portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IWebHostEnvironment webHostEnvironment, ILogger<HomeController> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Project()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ContactUs()
        {
            return View();
        }
        // ����� ��� ��������� �������� � ��������������� �� ���������� URL
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(culture))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );
            }

            return LocalRedirect(returnUrl);
        }
        [HttpPost]
        public async Task<IActionResult> ContactUs(string firstName, string phone, string email4, string message)
        {
            string logPath = Path.Combine(_webHostEnvironment.WebRootPath, "logs.txt");

            try
            {
                // �������� ���������� email
                if (string.IsNullOrEmpty(email4) || !Regex.IsMatch(email4, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
                {
                    TempData["Message"] = "����������, ��������� ��������� ����������� �����.";
                    TempData["MessageClass"] = "error";
                    return View();
                }

                // ������ � ���-����
                string logMessage = $"������������ {email4} �������� ���������: {message}\n";
                await System.IO.File.AppendAllTextAsync(logPath, logMessage);

                TempData["Message"] = "���� ��������� ����������.";
                TempData["MessageClass"] = "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "������ ��� �������� ��������� �������������: {Email}", email4);
                TempData["Message"] = "��������� ������ ��� �������� ���������.";
                TempData["MessageClass"] = "error";
            }

            // ����� �� ������ ��������� �� �� �� ��������, ����� �������� ���������
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Subscribe(string email)
        {
            string subscribersPath = Path.Combine(_webHostEnvironment.WebRootPath, "subscribers.txt");
            string logPath = Path.Combine(_webHostEnvironment.WebRootPath, "logs.txt");

            try
            {
                if (string.IsNullOrEmpty(email) || !Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
                {
                    TempData["Message"] = "����������, ��������� ����������� �����";
                    TempData["MessageClass"] = "error";
                }
                else
                {
                    await System.IO.File.AppendAllTextAsync(subscribersPath, email + Environment.NewLine);
                    TempData["Message"] = "���� ������ ����������";
                    TempData["MessageClass"] = "success";

                    string logMessage = $"������������ {email} ������� ��������.\n";
                    await System.IO.File.AppendAllTextAsync(logPath, logMessage);
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "��������� ������ ��� ��������� ������ �������.";
                TempData["MessageClass"] = "error";

                string errorMessage = $"������ ��� �������� ������������ {email}: {ex.Message}\n";
                await System.IO.File.AppendAllTextAsync(logPath, errorMessage);
            }

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
