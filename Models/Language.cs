using System.ComponentModel.DataAnnotations;

namespace Laba1_2.Models
{
    public class Language
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Extension { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Version { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string? SyntaxHighlighting { get; set; }

        public virtual ICollection<Solution> Solutions { get; set; } = new List<Solution>();
        public ICollection<ChallengeLanguage> ChallengeLanguages { get; set; } = new List<ChallengeLanguage>();
    }
}