using BD6.Models;
using BD6.Services;
using Microsoft.AspNetCore.Mvc;

namespace BD6.Controllers
{
    public class AccountController : Controller
    {
        private readonly DbService _dbService;
        public AccountController(DbService dbService)
        {
            _dbService = dbService;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                ViewBag.ErrorMessage = "Email и пароль обязательны.";
                return View();
            }

            var user = await _dbService.AuthenticateUser(model);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserRole", user.Role.RoleName);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Некорректный email или пароль.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _dbService.RegisterUser(model);

                if (success)
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.ErrorMessage = "Email уже занят.";
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

