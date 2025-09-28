using Laba1_2.Models;
using System.ComponentModel.DataAnnotations;

namespace Laba1_2.ViewModels
{
    public class SolutionViewModel
    {
        public int ChallengeId { get; set; }
        public string ChallengeTitle { get; set; } = string.Empty;
        public string ChallengeDescription { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string? ExampleInput { get; set; }
        public string? ExampleOutput { get; set; }

        [Required(ErrorMessage = "Виберіть мову програмування")]
        [Display(Name = "Мова програмування")]
        public int LanguageId { get; set; }

        [Required(ErrorMessage = "Код розв'язку обов'язковий")]
        [Display(Name = "Ваш код")]
        public string Code { get; set; } = string.Empty;

        public List<ChallengeLanguage> AvailableLanguages { get; set; } = new();
        public string? StarterCode { get; set; }
    }
}