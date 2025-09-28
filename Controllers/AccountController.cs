using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Laba1_2.Models;
using Laba1_2.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Laba1_2.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Додаємо повідомлення про помилки валідації
                TempData["ErrorMessage"] = "Будь ласка, виправте помилки в формі.";
                return View(model);
            }

            try
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Bio = model.Bio,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign default role
                    await _userManager.AddToRoleAsync(user, "Student");

                    _logger.LogInformation("User created a new account with password.");

                    // Автоматичний вхід після реєстрації
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    TempData["SuccessMessage"] = "Реєстрація успішна! Ласкаво просимо на платформу.";
                    return RedirectToAction("Index", "Home");
                }

                // Додаємо помилки реєстрації
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    _logger.LogWarning("Registration error: {Error}", error.Description);
                }

                TempData["ErrorMessage"] = "Помилка реєстрації. Перевірте введені дані.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час реєстрації користувача");
                TempData["ErrorMessage"] = "Сталася помилка під час реєстрації. Спробуйте ще раз.";
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Будь ласка, заповніть всі обов'язкові поля.";
                return View(model);
            }

            try
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Update last login time
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        user.LastLoginAt = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);
                    }

                    _logger.LogInformation("User {Email} logged in.", model.Email);
                    TempData["SuccessMessage"] = "Вітаємо! Ви успішно увійшли в систему.";

                    // Безпечне перенаправлення
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out: {Email}", model.Email);
                    TempData["ErrorMessage"] = "Обліковий запис тимчасово заблоковано.";
                }
                else if (result.IsNotAllowed)
                {
                    TempData["ErrorMessage"] = "Вхід не дозволено для цього облікового запису.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Невірний email або пароль. Спробуйте ще раз.";
                    _logger.LogWarning("Invalid login attempt for {Email}", model.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час входу для {Email}", model.Email);
                TempData["ErrorMessage"] = "Сталася помилка під час входу. Спробуйте ще раз.";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User logged out.");
                TempData["SuccessMessage"] = "Ви успішно вийшли з системи.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час виходу з системи");
                TempData["ErrorMessage"] = "Помилка під час виходу з системи.";
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            TempData["ErrorMessage"] = "У вас немає доступу до цієї сторінки.";
            return View();
        }

        private string GetRedirectUrlBasedOnRole(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            if (User.IsInRole("Admin"))
            {
                return Url.Action("Index", "Home", new { area = "Admin" })!;
            }
            else if (User.IsInRole("Mentor"))
            {
                return Url.Action("Index", "Home", new { area = "Mentor" })!;
            }
            else if (User.IsInRole("Student"))
            {
                return Url.Action("Index", "Home", new { area = "Student" })!;
            }
            else
            {
                return Url.Action("Index", "Home", new { area = "" })!;
            }
        }
    }
}