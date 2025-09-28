using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Laba1_2.Models;
using Laba1_2.Services;
using Laba1_2.ViewModels;
using System.Diagnostics;

namespace Laba1_2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IChallengeService _challengeService;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<User> userManager,
            IChallengeService challengeService)
        {
            _logger = logger;
            _userManager = userManager;
            _challengeService = challengeService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    // Redirect based on user role
                    if (roles.Contains("Admin"))
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    else if (roles.Contains("Mentor"))
                        return RedirectToAction("Index", "Home", new { area = "Mentor" });
                    else if (roles.Contains("Student"))
                        return RedirectToAction("Index", "Home", new { area = "Student" });
                }
            }

            // Show public landing page
            var recentChallenges = await _challengeService.GetActivesChallengesAsync();
            ViewBag.RecentChallenges = recentChallenges.Take(6).ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}