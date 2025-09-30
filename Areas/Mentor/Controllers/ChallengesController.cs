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

        // GET: Mentor/Challenges
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
                    .Include(c => c.Solutions)
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

        // GET: Mentor/Challenges/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Languages = await _context.Languages
                .Where(l => l.IsActive)
                .ToListAsync();
            return View();
        }

        // POST: Mentor/Challenges/Create
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

                // Валідація
                if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Description) ||
                    string.IsNullOrWhiteSpace(Instructions))
                {
                    TempData["ErrorMessage"] = "Заповніть всі обов'язкові поля";
                    ViewBag.Languages = await _context.Languages.Where(l => l.IsActive).ToListAsync();
                    return View();
                }

                if (selectedLanguages == null || selectedLanguages.Length == 0)
                {
                    TempData["ErrorMessage"] = "Оберіть хоча б одну мову програмування";
                    ViewBag.Languages = await _context.Languages.Where(l => l.IsActive).ToListAsync();
                    return View();
                }

                // Створюємо тест-кейси у форматі JSON
                var testCases = new List<object>();
                if (TestCaseInput != null && TestCaseOutput != null)
                {
                    for (int i = 0; i < TestCaseInput.Length; i++)
                    {
                        if (i < TestCaseOutput.Length &&
                            !string.IsNullOrWhiteSpace(TestCaseInput[i]) &&
                            !string.IsNullOrWhiteSpace(TestCaseOutput[i]))
                        {
                            testCases.Add(new
                            {
                                Input = TestCaseInput[i],
                                ExpectedOutput = TestCaseOutput[i]
                            });
                        }
                    }
                }

                if (testCases.Count == 0)
                {
                    TempData["ErrorMessage"] = "Додайте хоча б один тест-кейс";
                    ViewBag.Languages = await _context.Languages.Where(l => l.IsActive).ToListAsync();
                    return View();
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

        // GET: Mentor/Challenges/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var challenge = await _context.Challenges
                    .Include(c => c.ChallengeLanguages)
                        .ThenInclude(cl => cl.Language)
                    .Include(c => c.Solutions)
                        .ThenInclude(s => s.User)
                    .Include(c => c.Solutions)
                        .ThenInclude(s => s.Language)
                    .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUserId == user!.Id);

                if (challenge == null)
                {
                    TempData["ErrorMessage"] = "Завдання не знайдено";
                    return RedirectToAction(nameof(Index));
                }

                // Статистика
                ViewBag.TotalSubmissions = challenge.Solutions.Count;
                ViewBag.SuccessfulSubmissions = challenge.Solutions.Count(s => s.IsSuccessful);
                ViewBag.UniqueUsers = challenge.Solutions.Select(s => s.UserId).Distinct().Count();
                ViewBag.AverageScore = challenge.Solutions.Any()
                    ? challenge.Solutions.Average(s => s.PointsEarned)
                    : 0;

                return View(challenge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading challenge details");
                TempData["ErrorMessage"] = "Помилка завантаження завдання";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Mentor/Challenges/Edit/5
        public async Task<IActionResult> Edit(int id)
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

            ViewBag.Languages = await _context.Languages
                .Where(l => l.IsActive)
                .ToListAsync();

            return View(challenge);
        }

        // POST: Mentor/Challenges/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Challenge challenge, int[] selectedLanguages)
        {
            if (id != challenge.Id)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var existingChallenge = await _context.Challenges
                .Include(c => c.ChallengeLanguages)
                .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUserId == user!.Id);

            if (existingChallenge == null)
            {
                TempData["ErrorMessage"] = "Завдання не знайдено";
                return RedirectToAction(nameof(Index));
            }

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

                // Оновлюємо мови
                if (selectedLanguages != null && selectedLanguages.Length > 0)
                {
                    // Видаляємо старі зв'язки
                    _context.ChallengeLanguages.RemoveRange(existingChallenge.ChallengeLanguages);

                    // Додаємо нові
                    foreach (var langId in selectedLanguages)
                    {
                        _context.ChallengeLanguages.Add(new ChallengeLanguage
                        {
                            ChallengeId = existingChallenge.Id,
                            LanguageId = langId,
                            StarterCode = existingChallenge.ChallengeLanguages
                                .FirstOrDefault(cl => cl.LanguageId == langId)?.StarterCode
                        });
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Завдання успішно оновлено!";
                return RedirectToAction(nameof(Details), new { id = challenge.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating challenge");
                TempData["ErrorMessage"] = "Помилка оновлення завдання";

                ViewBag.Languages = await _context.Languages.Where(l => l.IsActive).ToListAsync();
                return View(challenge);
            }
        }

        // POST: Mentor/Challenges/Delete/5
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

        // GET: Mentor/Challenges/Solutions/5
        public async Task<IActionResult> Solutions(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var challenge = await _context.Challenges
                    .Include(c => c.Solutions)
                        .ThenInclude(s => s.User)
                    .Include(c => c.Solutions)
                        .ThenInclude(s => s.Language)
                    .Include(c => c.Solutions)
                        .ThenInclude(s => s.Results)
                    .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUserId == user!.Id);

                if (challenge == null)
                {
                    TempData["ErrorMessage"] = "Завдання не знайдено";
                    return RedirectToAction(nameof(Index));
                }

                var solutions = challenge.Solutions
                    .OrderByDescending(s => s.SubmittedAt)
                    .ToList();

                ViewBag.Challenge = challenge;
                return View(solutions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading solutions");
                TempData["ErrorMessage"] = "Помилка завантаження рішень";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Mentor/Challenges/SolutionDetails/5
        public async Task<IActionResult> SolutionDetails(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var solution = await _context.Solutions
                    .Include(s => s.User)
                    .Include(s => s.Language)
                    .Include(s => s.Challenge)
                    .Include(s => s.Results)
                    .FirstOrDefaultAsync(s => s.Id == id && s.Challenge.CreatedByUserId == user!.Id);

                if (solution == null)
                {
                    TempData["ErrorMessage"] = "Рішення не знайдено";
                    return RedirectToAction(nameof(Index));
                }

                return View(solution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading solution details");
                TempData["ErrorMessage"] = "Помилка завантаження рішення";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Mentor/Challenges/ToggleStatus/5
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var challenge = await _context.Challenges
                    .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUserId == user!.Id);

                if (challenge == null)
                {
                    TempData["ErrorMessage"] = "Завдання не знайдено";
                    return RedirectToAction(nameof(Index));
                }

                challenge.IsActive = !challenge.IsActive;
                challenge.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = challenge.IsActive
                    ? "Завдання активовано"
                    : "Завдання деактивовано";

                return RedirectToAction(nameof(Details), new { id = challenge.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling challenge status");
                TempData["ErrorMessage"] = "Помилка зміни статусу завдання";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}