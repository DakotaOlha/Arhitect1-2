using Laba1_2.Models;
using System.ComponentModel.DataAnnotations;

namespace Laba1_2.ViewModels
{
    public class ChallengeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва завдання обов'язкова")]
        [StringLength(200, ErrorMessage = "Назва не може бути довше 200 символів")]
        [Display(Name = "Назва завдання")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Опис обов'язковий")]
        [Display(Name = "Опис завдання")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Інструкції обов'язкові")]
        [Display(Name = "Інструкції для розв'язання")]
        public string Instructions { get; set; } = string.Empty;

        [Display(Name = "Приклад вхідних даних")]
        public string? ExampleInput { get; set; }

        [Display(Name = "Приклад вихідних даних")]
        public string? ExampleOutput { get; set; }

        [Required(ErrorMessage = "Тест-кейси обов'язкові")]
        [Display(Name = "Тест-кейси (JSON формат)")]
        public string TestCases { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "Рівень складності повинен бути від 1 до 10")]
        [Display(Name = "Рівень складності (1-10)")]
        public int DifficultyLevel { get; set; } = 1;

        [Range(1, 1000, ErrorMessage = "Кількість балів повинна бути від 1 до 1000")]
        [Display(Name = "Кількість балів")]
        public int Points { get; set; } = 10;

        [Display(Name = "Активне завдання")]
        public bool IsActive { get; set; } = true;

        public List<int> SelectedLanguages { get; set; } = new();
        public Dictionary<int, string> StarterCode { get; set; } = new();

        public List<Language> AvailableLanguages { get; set; } = new();
        public List<ChallengeLanguage> ChallengeLanguages { get; set; } = new();
        public User? CreatedByUser { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}