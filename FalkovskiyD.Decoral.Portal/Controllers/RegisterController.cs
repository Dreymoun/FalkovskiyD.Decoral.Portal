using FalkovskiyD.Decoral.Portal.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FalkovskiyD.Decoral.Portal.Controllers
{
    [Route("[controller]")]
    public class RegisterController : Controller
    {
        private readonly ILogger<RegisterController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RegisterController(ILogger<RegisterController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Показывает страницу регистрации
        [HttpGet("RegisterPage")]
        public IActionResult RegisterPage()
        {
            return View();
        }

        // Показывает страницу авторизации
        [HttpGet("AuthorizationPage")]
        public IActionResult AuthorizationPage()
        {
            return View();
        }

        [HttpPost]
        [Route("registeruser")]
        public async Task<IActionResult> RegisterUser(string name, string email, string password, string confirmPassword)
        {
            string logPath = Path.Combine(_webHostEnvironment.WebRootPath, "logs.txt");
            string userInfoPath = Path.Combine(_webHostEnvironment.WebRootPath, "userinfo.txt");

            try
            {
                if (!Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
                {
                    TempData["Message"] = "Ошибка: некорректный электронный адрес.";
                    TempData["MessageClass"] = "error";
                    return View("RegisterPage");
                }

                if (password != confirmPassword)
                {
                    TempData["Message"] = "Ошибка: пароли не совпадают.";
                    TempData["MessageClass"] = "error";
                    return View("RegisterPage");
                }

                // Запись данных пользователя
                string userInfo = $"{name},{email},{password}\n";
                await System.IO.File.AppendAllTextAsync(userInfoPath, userInfo);

                // Логирование
                string logMessage = $"Зарегистрирован новый пользователь: {email}\n";
                await System.IO.File.AppendAllTextAsync(logPath, logMessage);

                TempData["Message"] = "Пользователь успешно зарегистрирован.";
                TempData["MessageClass"] = "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при регистрации пользователя: {Email}", email);

                TempData["Message"] = "Ошибка при регистрации. Попробуйте позже.";
                TempData["MessageClass"] = "error";

                // Логирование ошибки
                string errorMessage = $"Ошибка при регистрации пользователя {email}: {ex.Message}\n";
                await System.IO.File.AppendAllTextAsync(logPath, errorMessage);
            }

            return RedirectToAction("RegisterPage");
        }

        // Метод для выхода пользователя
        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("UserName");
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
