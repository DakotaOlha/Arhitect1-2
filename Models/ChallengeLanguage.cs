using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Laba1_2.Models
{
    // Many-to-many relationship between Challenge and Language
    public class ChallengeLanguage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ChallengeId { get; set; }

        [Required]
        public int LanguageId { get; set; }

        public string? StarterCode { get; set; } // Template code for specific language

        // Navigation properties
        [ForeignKey("ChallengeId")]
        public virtual Challenge Challenge { get; set; } = null!;

        [ForeignKey("LanguageId")]
        public virtual Language Language { get; set; } = null!;
    }
}
