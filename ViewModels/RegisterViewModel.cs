using System.ComponentModel.DataAnnotations;

namespace Laba1_2.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ім'я є обов'язковим")]
        [Display(Name = "Ім'я")]
        [StringLength(50, ErrorMessage = "Ім'я не може бути довше 50 символів")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Прізвище є обов'язковим")]
        [Display(Name = "Прізвище")]
        [StringLength(50, ErrorMessage = "Прізвище не може бути довше 50 символів")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [StringLength(100, ErrorMessage = "Пароль повинен містити принаймні {2} символи", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження пароля")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Про себе")]
        [StringLength(500, ErrorMessage = "Біографія не може бути довше 500 символів")]
        public string? Bio { get; set; }

        [Required(ErrorMessage = "Необхідно прийняти умови використання")]
        [Display(Name = "Я погоджуюсь з умовами використання")]
        public bool AcceptTerms { get; set; }
    }
}