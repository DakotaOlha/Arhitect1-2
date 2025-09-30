using Laba1_2.Data;
using Laba1_2.Models;
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

        public ChallengesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        public async Task<IActionResult> Solve(int id, int languageId)
        {
            var challenge = await _context.Challenges
                .Include(c => c.ChallengeLanguages.Where(cl => cl.LanguageId == languageId))
                .ThenInclude(cl => cl.Language)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (challenge == null)
                return NotFound();

            var challengeLanguage = challenge.ChallengeLanguages.FirstOrDefault();
            if (challengeLanguage == null)
                return BadRequest("Language not supported for this challenge");

            ViewBag.ChallengeLanguage = challengeLanguage;
            return View(challenge);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitSolution(int challengeId, int languageId, string code)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var solution = new Solution
            {
                UserId = user.Id,
                ChallengeId = challengeId,
                LanguageId = languageId,
                Code = code,
                SubmittedAt = DateTime.UtcNow
            };

            _context.Solutions.Add(solution);
            await _context.SaveChangesAsync();
            solution.IsSuccessful = true;
            solution.PointsEarned = 10;
            await _context.SaveChangesAsync();
                
            return Json(new { success = true, message = "Solution submitted successfully!" });
        }
    }
}
