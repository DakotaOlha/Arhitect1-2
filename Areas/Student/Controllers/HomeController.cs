using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Laba1_2.Models;
using Laba1_2.Data;
using Microsoft.EntityFrameworkCore;

namespace Laba1_2.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account", new { area = "" });

            // ВАЖЛИВО: Виконуємо запити ПОСЛІДОВНО, а не паралельно
            // Це вирішує проблему з DbContext concurrency

            // 1. Отримуємо ID завершених завдань
            var completedChallengeIds = await _context.Solutions
                .Where(s => s.UserId == user.Id && s.IsSuccessful)
                .Select(s => s.ChallengeId)
                .Distinct()
                .ToListAsync();

            // 2. Отримуємо ID завдань в процесі
            var inProgressChallengeIds = await _context.Solutions
                .Where(s => s.UserId == user.Id && !s.IsSuccessful)
                .Select(s => s.ChallengeId)
                .Distinct()
                .Where(id => !completedChallengeIds.Contains(id))
                .ToListAsync();

            // 3. Завантажуємо завершені завдання
            var completedChallenges = await _context.Challenges
                .Where(c => completedChallengeIds.Contains(c.Id))
                .Include(c => c.ChallengeLanguages)
                .ThenInclude(cl => cl.Language)
                .OrderByDescending(c => c.CreatedAt)
                .Take(6)
                .ToListAsync();

            // 4. Завантажуємо завдання в процесі
            var inProgressChallenges = await _context.Challenges
                .Where(c => inProgressChallengeIds.Contains(c.Id))
                .Include(c => c.ChallengeLanguages)
                .ThenInclude(cl => cl.Language)
                .Take(6)
                .ToListAsync();

            // 5. Рекомендовані завдання
            var attemptedChallengeIds = completedChallengeIds.Concat(inProgressChallengeIds).ToList();
            var recommendedChallenges = await _context.Challenges
                .Where(c => c.IsActive && !attemptedChallengeIds.Contains(c.Id))
                .Include(c => c.ChallengeLanguages)
                .ThenInclude(cl => cl.Language)
                .OrderBy(c => c.DifficultyLevel)
                .Take(6)
                .ToListAsync();

            // 6. Підрахунок статистики
            var totalCompleted = completedChallengeIds.Count;
            var totalInProgress = inProgressChallengeIds.Count;

            var totalPoints = await _context.Solutions
                .Where(s => s.UserId == user.Id && s.IsSuccessful)
                .SumAsync(s => s.PointsEarned);

            // 7. Остання активність
            var recentActivity = await _context.Solutions
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(5)
                .Include(s => s.Challenge)
                .Include(s => s.Language)
                .ToListAsync();

            // Створюємо ViewModel
            var viewModel = new StudentDashboardViewModel
            {
                UserName = user.UserName ?? user.Email ?? "Студент",
                CompletedChallenges = completedChallenges,
                InProgressChallenges = inProgressChallenges,
                RecommendedChallenges = recommendedChallenges,
                TotalCompleted = totalCompleted,
                TotalInProgress = totalInProgress,
                TotalPoints = totalPoints,
                RecentActivity = recentActivity
            };

            return View(viewModel);
        }
    }

    // ViewModel для передачі даних в View
    public class StudentDashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public List<Challenge> CompletedChallenges { get; set; } = new();
        public List<Challenge> InProgressChallenges { get; set; } = new();
        public List<Challenge> RecommendedChallenges { get; set; } = new();
        public int TotalCompleted { get; set; }
        public int TotalInProgress { get; set; }
        public int TotalPoints { get; set; }
        public List<Solution> RecentActivity { get; set; } = new();
    }
}