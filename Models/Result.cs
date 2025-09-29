using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Laba1_2.Models
{
    public enum ResultStatus
    {
        Passed,
        Failed,
        Error,
        Timeout,
        CompilationError
    }

    public class Result
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public ResultStatus Status { get; set; }

        public string? Input { get; set; }

        public string? ExpectedOutput { get; set; }

        public string? ActualOutput { get; set; }

        public string? ErrorMessage { get; set; }

        public int ExecutionTimeMs { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int ChallengeId { get; set; }

        [Required]
        public int SolutionId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ChallengeId")]
        public virtual Challenge Challenge { get; set; } = null!;

        [ForeignKey("SolutionId")]
        public virtual Solution Solution { get; set; } = null!;
    }
}