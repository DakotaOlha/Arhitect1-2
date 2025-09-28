using Laba1_2.Models;
using Laba1_2.Services;
using System.ComponentModel.DataAnnotations;

namespace Laba1_2.ViewModels
{
    public class UserProfileViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ім'я обов'язкове")]
        [StringLength(100)]
        [Display(Name = "Ім'я")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Прізвище обов'язкове")]
        [StringLength(100)]
        [Display(Name = "Прізвище")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Біографія")]
        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }

        // Statistics
        public UserStatistics Statistics { get; set; } = new();

        // Recent solutions
        public List<Solution> RecentSolutions { get; set; } = new();

        // User roles
        public List<string> Roles { get; set; } = new();
    }
}