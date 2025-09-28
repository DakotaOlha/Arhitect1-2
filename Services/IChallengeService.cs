using Laba1_2.Models;

namespace Laba1_2.Services
{
    public interface IChallengeService
    {
        Task<List<Challenge>> GetActivesChallengesAsync();
        Task<List<Challenge>> GetChallengesByDifficultyAsync(int difficulty);
        Task<Challenge?> GetChallengeByIdAsync(int id);
        Task<List<Challenge>> GetChallengesByMentorAsync(string mentorId);
        Task<bool> SubmitSolutionAsync(Solution solution);
        Task<List<Solution>> GetUserSolutionsAsync(string userId, int? challengeId = null);
        Task<UserStatistics> GetUserStatisticsAsync(string userId);
    }

    public class UserStatistics
    {
        public int TotalChallengesSolved { get; set; }
        public int TotalPoints { get; set; }
        public int EasyChallengesSolved { get; set; }
        public int MediumChallengesSolved { get; set; }
        public int HardChallengesSolved { get; set; }
        public double SuccessRate { get; set; }
        public string FavoriteLanguage { get; set; } = string.Empty;
        public DateTime LastSubmission { get; set; }
    }
}