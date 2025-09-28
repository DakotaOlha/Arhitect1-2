using Laba1_2.Data;
using Laba1_2.Models;
using Microsoft.EntityFrameworkCore;

namespace Laba1_2.Services
{
    public class ChallengeService : IChallengeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICodeExecutionService _codeExecutionService;
        private readonly ILogger<ChallengeService> _logger;

        public ChallengeService(
            ApplicationDbContext context,
            ICodeExecutionService codeExecutionService,
            ILogger<ChallengeService> logger)
        {
            _context = context;
            _codeExecutionService = codeExecutionService;
            _logger = logger;
        }

        public async Task<List<Challenge>> GetActivesChallengesAsync()
        {
            return await _context.Challenges
                .Where(c => c.IsActive)
                .Include(c => c.ChallengeLanguages)
                .ThenInclude(cl => cl.Language)
                .OrderBy(c => c.DifficultyLevel)
                .ThenBy(c => c.Title)
                .ToListAsync();
        }

        public async Task<List<Challenge>> GetChallengesByDifficultyAsync(int difficulty)
        {
            return await _context.Challenges
                .Where(c => c.IsActive && c.DifficultyLevel == difficulty)
                .Include(c => c.ChallengeLanguages)
                .ThenInclude(cl => cl.Language)
                .OrderBy(c => c.Title)
                .ToListAsync();
        }

        public async Task<Challenge?> GetChallengeByIdAsync(int id)
        {
            return await _context.Challenges
                .Include(c => c.ChallengeLanguages)
                .ThenInclude(cl => cl.Language)
                .Include(c => c.CreatedByUser)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<List<Challenge>> GetChallengesByMentorAsync(string mentorId)
        {
            return await _context.Challenges
                .Where(c => c.CreatedByUserId == mentorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> SubmitSolutionAsync(Solution solution)
        {
            try
            {
                // Get challenge and language info
                var challenge = await _context.Challenges
                    .FirstOrDefaultAsync(c => c.Id == solution.ChallengeId);

                var language = await _context.Languages
                    .FirstOrDefaultAsync(l => l.Id == solution.LanguageId);

                if (challenge == null || language == null)
                    return false;

                // Execute the code
                var executionResult = await _codeExecutionService
                    .ExecuteCodeAsync(solution.Code, language, challenge.TestCases);

                // Update solution with results
                solution.IsSuccessful = executionResult.IsSuccessful;
                solution.ExecutionTimeMs = executionResult.ExecutionTimeMs;
                solution.ErrorMessage = executionResult.ErrorMessage;
                solution.PointsEarned = executionResult.IsSuccessful ? challenge.Points : 0;

                // Save solution
                _context.Solutions.Add(solution);
                await _context.SaveChangesAsync();

                // Create result records
                foreach (var testResult in executionResult.TestResults)
                {
                    var result = new Result
                    {
                        UserId = solution.UserId,
                        ChallengeId = solution.ChallengeId,
                        SolutionId = solution.Id,
                        Status = testResult.Passed ? ResultStatus.Passed : ResultStatus.Failed,
                        Input = testResult.Input,
                        ExpectedOutput = testResult.ExpectedOutput,
                        ActualOutput = testResult.ActualOutput,
                        ErrorMessage = testResult.ErrorMessage,
                        ExecutionTimeMs = testResult.ExecutionTimeMs
                    };

                    _context.Results.Add(result);
                }

                // Update user statistics if successful
                if (solution.IsSuccessful)
                {
                    var user = await _context.Users.FindAsync(solution.UserId);
                    if (user != null)
                    {
                        user.TotalScore += solution.PointsEarned;
                        user.SolvedChallenges += 1;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting solution");
                return false;
            }
        }

        public async Task<List<Solution>> GetUserSolutionsAsync(string userId, int? challengeId = null)
        {
            IQueryable<Solution> query = _context.Solutions
                .Where(s => s.UserId == userId)
                .Include(s => s.Challenge)
                .Include(s => s.Language);

            if (challengeId.HasValue)
            {
                query = query.Where(s => s.ChallengeId == challengeId.Value);
            }

            return await query
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<UserStatistics> GetUserStatisticsAsync(string userId)
        {
            var solutions = await _context.Solutions
                .Where(s => s.UserId == userId)
                .Include(s => s.Challenge)
                .Include(s => s.Language)
                .ToListAsync();

            var successfulSolutions = solutions.Where(s => s.IsSuccessful).ToList();

            var stats = new UserStatistics
            {
                TotalChallengesSolved = successfulSolutions.Count,
                TotalPoints = successfulSolutions.Sum(s => s.PointsEarned),
                EasyChallengesSolved = successfulSolutions.Count(s => s.Challenge.DifficultyLevel <= 3),
                MediumChallengesSolved = successfulSolutions.Count(s => s.Challenge.DifficultyLevel >= 4 && s.Challenge.DifficultyLevel <= 6),
                HardChallengesSolved = successfulSolutions.Count(s => s.Challenge.DifficultyLevel >= 7),
                SuccessRate = solutions.Count > 0 ? (double)successfulSolutions.Count / solutions.Count * 100 : 0,
                LastSubmission = solutions.Any() ? solutions.Max(s => s.SubmittedAt) : DateTime.MinValue
            };

            // Find favorite language
            var languageUsage = solutions
                .GroupBy(s => s.Language.Name)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            stats.FavoriteLanguage = languageUsage?.Key ?? "None";

            return stats;
        }
    }
}