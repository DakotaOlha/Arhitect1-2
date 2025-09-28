using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Laba1_2.Models
{
    public class Challenge
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Instructions { get; set; } = string.Empty;

        public string? ExampleInput { get; set; }

        public string? ExampleOutput { get; set; }

        [Required]
        public string TestCases { get; set; } = string.Empty; // JSON format

        [Range(1, 10)]
        public int DifficultyLevel { get; set; } = 1;

        [Range(1, 1000)]
        public int Points { get; set; } = 10;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Foreign key
        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; } = null!;

        public virtual ICollection<Solution> Solutions { get; set; } = new List<Solution>();
        public virtual ICollection<Result> Results { get; set; } = new List<Result>();
        public virtual ICollection<ChallengeLanguage> ChallengeLanguages { get; set; } = new List<ChallengeLanguage>();
    }
}