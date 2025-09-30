using Laba1_2.Data;
using Laba1_2.Models;
using Laba1_2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Laba1_2.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class ChallengesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ICodeExecutionService _codeExecutionService;
        private readonly ILogger<ChallengesController> _logger;

        public ChallengesController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            ICodeExecutionService codeExecutionService,
            ILogger<ChallengesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _codeExecutionService = codeExecutionService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? difficulty)
        {
            var query = _context.Challenges
                .Include(c => c.ChallengeLanguages)
                .ThenInclude(cl => cl.Language)
                .Where(c => c.IsActive);

            if (difficulty.HasValue)
            {
                query = query.Where(c => c.DifficultyLevel == difficulty.Value);
            }

            var challenges = await query
                .OrderBy(c => c.DifficultyLevel)
                .ThenBy(c => c.Title)
                .ToListAsync();

            return View(challenges);
        }

        public async Task<IActionResult> Details(int id)
        {
            var challenge = await _context.Challenges
                .Include(c => c.ChallengeLanguages)
                .ThenInclude(cl => cl.Language)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (challenge == null)
                return NotFound();

            return View(challenge);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitSolution([FromBody] SubmitSolutionRequest request)
        {
            try
            {
                _logger.LogInformation($"Submitting solution for challenge {request.ChallengeId}");

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Користувач не знайдений" });
                }

                // Перевіряємо чи існує завдання
                var challenge = await _context.Challenges
                    .FirstOrDefaultAsync(c => c.Id == request.ChallengeId && c.IsActive);

                if (challenge == null)
                {
                    return Json(new { success = false, message = "Завдання не знайдено" });
                }

                // Перевіряємо чи існує мова
                var language = await _context.Languages
                    .FirstOrDefaultAsync(l => l.Id == request.LanguageId);

                if (language == null)
                {
                    return Json(new { success = false, message = "Мова програмування не знайдена" });
                }

                // Виконуємо код
                _logger.LogInformation($"Executing code for challenge {challenge.Title} in {language.Name}");

                var executionResult = await _codeExecutionService
                    .ExecuteCodeAsync(request.Code, language, challenge.TestCases);

                // Створюємо рішення
                var solution = new Solution
                {
                    UserId = user.Id,
                    ChallengeId = request.ChallengeId,
                    LanguageId = request.LanguageId,
                    Code = request.Code,
                    SubmittedAt = DateTime.UtcNow,
                    IsSuccessful = executionResult.IsSuccessful,
                    ExecutionTimeMs = executionResult.ExecutionTimeMs,
                    ErrorMessage = executionResult.ErrorMessage,
                    PointsEarned = executionResult.IsSuccessful ? challenge.Points : 0
                };

                _context.Solutions.Add(solution);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Solution saved with ID {solution.Id}, Success: {solution.IsSuccessful}");

                // Зберігаємо результати тест-кейсів
                foreach (var testResult in executionResult.TestResults)
                {
                    var result = new Result
                    {
                        UserId = user.Id,
                        ChallengeId = request.ChallengeId,
                        SolutionId = solution.Id,
                        Status = testResult.Passed ? ResultStatus.Passed : ResultStatus.Failed,
                        Input = testResult.Input,
                        ExpectedOutput = testResult.ExpectedOutput,
                        ActualOutput = testResult.ActualOutput,
                        ErrorMessage = testResult.ErrorMessage,
                        ExecutionTimeMs = testResult.ExecutionTimeMs,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Results.Add(result);
                }

                // Оновлюємо статистику користувача, якщо успішно
                if (solution.IsSuccessful)
                {
                    // Перевіряємо чи це перше успішне рішення для цього завдання
                    var previousSuccessfulSolution = await _context.Solutions
                        .AnyAsync(s => s.UserId == user.Id
                                    && s.ChallengeId == request.ChallengeId
                                    && s.IsSuccessful
                                    && s.Id != solution.Id);

                    if (!previousSuccessfulSolution)
                    {
                        user.TotalScore += solution.PointsEarned;
                        user.SolvedChallenges += 1;
                        await _userManager.UpdateAsync(user);
                        _logger.LogInformation($"Updated user stats: Score={user.TotalScore}, Solved={user.SolvedChallenges}");
                    }
                }

                await _context.SaveChangesAsync();

                if (executionResult.IsSuccessful)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Вітаємо! Всі тести пройдено успішно! Ви заробили {solution.PointsEarned} балів!",
                        pointsEarned = solution.PointsEarned,
                        testsPassedCount = executionResult.TestResults.Count(t => t.Passed),
                        totalTests = executionResult.TestResults.Count
                    });
                }
                else
                {
                    var failedTests = executionResult.TestResults.Where(t => !t.Passed).ToList();
                    var errorMessage = failedTests.Any()
                        ? $"Деякі тести не пройдено. Пройдено {executionResult.TestResults.Count(t => t.Passed)}/{executionResult.TestResults.Count} тестів."
                        : executionResult.ErrorMessage ?? "Помилка виконання коду";

                    return Json(new
                    {
                        success = false,
                        message = errorMessage,
                        testsPassedCount = executionResult.TestResults.Count(t => t.Passed),
                        totalTests = executionResult.TestResults.Count,
                        failedTests = failedTests.Select(t => new
                        {
                            input = t.Input,
                            expected = t.ExpectedOutput,
                            actual = t.ActualOutput,
                            error = t.ErrorMessage
                        }).ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting solution");
                return Json(new
                {
                    success = false,
                    message = $"Виникла помилка: {ex.Message}"
                });
            }
        }
    }

    public class SubmitSolutionRequest
    {
        public int ChallengeId { get; set; }
        public int LanguageId { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}