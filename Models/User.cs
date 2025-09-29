using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Laba1_2.Models
{
    public class User : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastLoginAt { get; set; }

        public int TotalScore { get; set; } = 0;

        public int SolvedChallenges { get; set; } = 0;

        public virtual ICollection<Solution> Solutions { get; set; } = new List<Solution>();
        public virtual ICollection<Result> Results { get; set; } = new List<Result>();
        public virtual ICollection<Challenge> CreatedChallenges { get; set; } = new List<Challenge>();
    }
}
