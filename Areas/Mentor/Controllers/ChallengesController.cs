using Laba1_2.Data;
using Laba1_2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Laba1_2.Areas.Mentor.Controllers
{
    [Area("Mentor")]
    [Authorize(Roles = "Mentor")]
    public class ChallengesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ChallengesController> _logger;

        public ChallengesController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            ILogger<ChallengesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var challenges = await _context.Challenges
                    .Where(c => c.CreatedByUserId == user.Id)
                    .Include(c => c.ChallengeLanguages)
                        .ThenInclude(cl => cl.Language)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return View(challenges);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading challenges");
                TempData["ErrorMessage"] = "Помилка завантаження завдань";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Languages = await _context.Languages
                .Where(l => l.IsActive)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string Title,
            int DifficultyLevel,
            string Description,
            string Instructions,
            int Points,
            string? ExampleInput,
            string? ExampleOutput,
            int[] selectedLanguages,
            string? StarterCode,
            string[]? TestCaseInput,
            string[]? TestCaseOutput,
            bool IsActive = true)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Створюємо тест-кейси у форматі JSON
                var testCases = new List<object>();
                if (TestCaseInput != null && TestCaseOutput != null)
                {
                    for (int i = 0; i < TestCaseInput.Length; i++)
                    {
                        if (i < TestCaseOutput.Length)
                        {
                            testCases.Add(new
                            {
                                Input = TestCaseInput[i],
                                ExpectedOutput = TestCaseOutput[i]
                            });
                        }
                    }
                }

                var challenge = new Challenge
                {
                    Title = Title,
                    Description = Description,
                    Instructions = Instructions,
                    ExampleInput = ExampleInput,
                    ExampleOutput = ExampleOutput,
                    TestCases = JsonSerializer.Serialize(testCases),
                    DifficultyLevel = DifficultyLevel,
                    Points = Points,
                    IsActive = IsActive,
                    CreatedByUserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Challenges.Add(challenge);
                await _context.SaveChangesAsync();

                // Додаємо мови програмування
                if (selectedLanguages != null && selectedLanguages.Length > 0)
                {
                    foreach (var langId in selectedLanguages)
                    {
                        var challengeLanguage = new ChallengeLanguage
                        {
                            ChallengeId = challenge.Id,
                            LanguageId = langId,
                            StarterCode = StarterCode
                        };
                        _context.ChallengeLanguages.Add(challengeLanguage);
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Завдання '{Title}' успішно створено!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating challenge");
                TempData["ErrorMessage"] = "Помилка створення завдання: " + ex.Message;

                ViewBag.Languages = await _context.Languages
                    .Where(l => l.IsActive)
                    .ToListAsync();
                return View();
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var challenge = await _context.Challenges
                    .Include(c => c.ChallengeLanguages)
                        .ThenInclude(cl => cl.Language)
                    .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUserId == user!.Id);

                if (challenge == null)
                {
                    TempData["ErrorMessage"] = "Завдання не знайдено";
                    return RedirectToAction(nameof(Index));
                }

                return View(challenge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading challenge details");
                TempData["ErrorMessage"] = "Помилка завантаження завдання";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var challenge = await _context.Challenges
                .Include(c => c.ChallengeLanguages)
                .ThenInclude(cl => cl.Language)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (challenge == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (challenge.CreatedByUserId != user!.Id)
                return Forbid();

            ViewBag.Languages = await _context.Languages
                .Where(l => l.IsActive)
                .ToListAsync();

            return View(challenge);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Challenge challenge)
        {
            if (id != challenge.Id)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var existingChallenge = await _context.Challenges
                .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUserId == user!.Id);

            if (existingChallenge == null)
                return NotFound();

            try
            {
                existingChallenge.Title = challenge.Title;
                existingChallenge.Description = challenge.Description;
                existingChallenge.Instructions = challenge.Instructions;
                existingChallenge.ExampleInput = challenge.ExampleInput;
                existingChallenge.ExampleOutput = challenge.ExampleOutput;
                existingChallenge.TestCases = challenge.TestCases;
                existingChallenge.DifficultyLevel = challenge.DifficultyLevel;
                existingChallenge.Points = challenge.Points;
                existingChallenge.IsActive = challenge.IsActive;
                existingChallenge.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Завдання успішно оновлено!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating challenge");
                TempData["ErrorMessage"] = "Помилка оновлення завдання";
                return View(challenge);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var challenge = await _context.Challenges
                    .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUserId == user!.Id);

                if (challenge == null)
                {
                    return Json(new { success = false, message = "Завдання не знайдено" });
                }

                _context.Challenges.Remove(challenge);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Завдання видалено" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting challenge");
                return Json(new { success = false, message = "Помилка видалення" });
            }
        }
    }
}