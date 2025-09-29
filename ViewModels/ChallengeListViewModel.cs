using Laba1_2.Models;

namespace Laba1_2.ViewModels
{
    public class ChallengeListViewModel
    {
        public List<Challenge> Challenges { get; set; } = new();
        public int? SelectedDifficulty { get; set; }
        public string? SelectedLanguage { get; set; }
        public string? SearchTerm { get; set; }
        public List<Language> AvailableLanguages { get; set; } = new();

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalChallenges { get; set; }

        public string SortBy { get; set; } = "title";
        public string SortOrder { get; set; } = "asc";
    }
}