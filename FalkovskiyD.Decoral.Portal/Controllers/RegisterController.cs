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

            return RedirectToAction("AuthorizationPage");
        }

        // Метод для выхода пользователя
        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("UserName");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Route("authorize")]
        public async Task<IActionResult> Authorize(string name, string password)
        {
            string usersDataPath = Path.Combine(_webHostEnvironment.WebRootPath, "userinfo.txt");
            string logPath = Path.Combine(_webHostEnvironment.WebRootPath, "logs.txt");
            try
            {
                // Проверка существует ли файл с пользователями
                if (!System.IO.File.Exists(usersDataPath))
                {
                    TempData["Message"] = "Ошибка авторизации: файл пользователей не найден.";
                    TempData["MessageClass"] = "error";
                    return View("AuthorizationPage");
                }

                // Чтение и поиск пользователя
                var usersData = await System.IO.File.ReadAllLinesAsync(usersDataPath);
                var userRecord = usersData.FirstOrDefault(u => u.StartsWith($"{name},"));

                if (userRecord != null)
                {
                    var userInfo = userRecord.Split(',');
                    if (userInfo.Length == 3 && userInfo[2] == password) // Индекс 2, потому что пароль находится на третьей позиции после имени и email
                    {
                        // Запись в куки
                        HttpContext.Response.Cookies.Append("UserName", name, new CookieOptions
                        {
                            HttpOnly = true,
                            Expires = DateTimeOffset.Now.AddDays(7)
                        });

                        // Логирование
                        string logMessage = $"Пользователь {name} успешно авторизован.\n";
                        await System.IO.File.AppendAllTextAsync(logPath, logMessage);

                        // Редирект на главную страницу или другую страницу
                        return RedirectToAction("Index", "Home");
                    }
                }

                TempData["Message"] = "Ошибка авторизации: неверное имя пользователя или пароль.";
                TempData["MessageClass"] = "error";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при авторизации пользователя: {Name}", name);

                // Логирование ошибки
                string errorMessage = $"Ошибка при авторизации пользователя {name}: {ex.Message}\n";
                await System.IO.File.AppendAllTextAsync(logPath, errorMessage);

                TempData["Message"] = "Ошибка при авторизации. Попробуйте позже.";
                TempData["MessageClass"] = "error";
            }
            return View("AuthorizationPage");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
