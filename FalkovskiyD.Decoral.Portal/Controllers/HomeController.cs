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
        // Метод для установки культуры и перенаправления на предыдущий URL
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
                // Проверка валидности email
                if (string.IsNullOrEmpty(email4) || !Regex.IsMatch(email4, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
                {
                    TempData["Message"] = "Пожалуйста, проверьте введенный электронный адрес.";
                    TempData["MessageClass"] = "error";
                    return View();
                }

                // Запись в лог-файл
                string logMessage = $"Пользователь {email4} отправил сообщение: {message}\n";
                await System.IO.File.AppendAllTextAsync(logPath, logMessage);

                TempData["Message"] = "Ваше сообщение отправлено.";
                TempData["MessageClass"] = "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке сообщения пользователем: {Email}", email4);
                TempData["Message"] = "Произошла ошибка при отправке сообщения.";
                TempData["MessageClass"] = "error";
            }

            // Здесь вы можете вернуться на ту же страницу, чтобы показать сообщение
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
                    TempData["Message"] = "Пожалуйста, проверьте электронный адрес";
                    TempData["MessageClass"] = "error";
                }
                else
                {
                    await System.IO.File.AppendAllTextAsync(subscribersPath, email + Environment.NewLine);
                    TempData["Message"] = "Ваше письмо отправлено";
                    TempData["MessageClass"] = "success";

                    string logMessage = $"Пользователь {email} успешно подписан.\n";
                    await System.IO.File.AppendAllTextAsync(logPath, logMessage);
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Произошла ошибка при обработке вашего запроса.";
                TempData["MessageClass"] = "error";

                string errorMessage = $"Ошибка при подписке пользователя {email}: {ex.Message}\n";
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
