using Laba1_2.Models;

namespace Laba1_2.ViewModels
{
    public class LeaderboardViewModel
    {
        public List<LeaderboardEntry> TopUsers { get; set; } = new();
        public LeaderboardEntry? CurrentUserEntry { get; set; }
        public string Period { get; set; } = "all"; 
    }

    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public User User { get; set; } = null!;
        public int TotalPoints { get; set; }
        public int ChallengesSolved { get; set; }
        public double SuccessRate { get; set; }
        public string FavoriteLanguage { get; set; } = string.Empty;
    }
}