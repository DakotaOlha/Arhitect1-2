using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Laba1_2.Models
{
    public class Solution
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Code { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public bool IsSuccessful { get; set; } = false;

        public int ExecutionTimeMs { get; set; } = 0;

        public string? ErrorMessage { get; set; }

        [Range(0, 1000)]
        public int PointsEarned { get; set; } = 0;

        // Foreign keys
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int ChallengeId { get; set; }

        [Required]
        public int LanguageId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ChallengeId")]
        public virtual Challenge Challenge { get; set; } = null!;

        [ForeignKey("LanguageId")]
        public virtual Language Language { get; set; } = null!;

        public virtual ICollection<Result> Results { get; set; } = new List<Result>();
    }
}

