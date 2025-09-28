using Laba1_2.Data;
using Laba1_2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Laba1_2.Areas.Mentor.Controllers
{
    [Area("Mentor")]
    [Authorize(Roles = "Mentor")]
    public class ChallengesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ChallengesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var challenges = await _context.Challenges
                .Where(c => c.CreatedByUserId == user!.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(challenges);
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
        public async Task<IActionResult> Create(Challenge challenge, int[] selectedLanguages, Dictionary<int, string> starterCode)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                challenge.CreatedByUserId = user!.Id;
                challenge.CreatedAt = DateTime.UtcNow;

                _context.Challenges.Add(challenge);
                await _context.SaveChangesAsync();

                // Add selected languages
                foreach (var langId in selectedLanguages)
                {
                    var challengeLanguage = new ChallengeLanguage
                    {
                        ChallengeId = challenge.Id,
                        LanguageId = langId,
                        StarterCode = starterCode.ContainsKey(langId) ? starterCode[langId] : null
                    };
                    _context.ChallengeLanguages.Add(challengeLanguage);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Languages = await _context.Languages
                .Where(l => l.IsActive)
                .ToListAsync();
            return View(challenge);
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

            if (ModelState.IsValid)
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
                return RedirectToAction(nameof(Index));
            }

            return View(challenge);
        }
    }
}