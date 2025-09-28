using Laba1_2.Models;

namespace Laba1_2.ViewModels
{
    public class StudentDashboardViewModel
    {
        public User User { get; set; } = null!;
        public List<Challenge> RecommendedChallenges { get; set; } = new();
        public List<Solution> RecentSolutions { get; set; } = new();
        public int TotalChallengesSolved { get; set; }
        public int TotalPoints { get; set; }
        public string FavoriteLanguage { get; set; } = string.Empty;
        public Dictionary<int, int> ChallengesByDifficulty { get; set; } = new();
    }

    public class MentorDashboardViewModel
    {
        public User User { get; set; } = null!;
        public List<Challenge> MyyChallenges { get; set; } = new();
        public int TotalChallengesCreated { get; set; }
        public int TotalStudentsSolved { get; set; }
        public List<Solution> RecentSubmissions { get; set; } = new();
        public Dictionary<string, int> PopularLanguages { get; set; } = new();
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalChallenges { get; set; }
        public int TotalSolutions { get; set; }
        public int ActiveUsers { get; set; }
        public List<User> RecentUsers { get; set; } = new();
        public List<Challenge> RecentChallenges { get; set; } = new();
        public Dictionary<string, int> UsersByRole { get; set; } = new();
        public Dictionary<string, int> SolutionsByLanguage { get; set; } = new();
    }
}