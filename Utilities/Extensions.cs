using Laba1_2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Laba1_2.Utilities
{
    public static class Extensions
    {
        public static string GetFullName(this User user)
        {
            return $"{user.FirstName} {user.LastName}";
        }

        public static string GetDifficultyText(this Challenge challenge)
        {
            return challenge.DifficultyLevel switch
            {
                1 or 2 or 3 => "Легкий",
                4 or 5 or 6 => "Середній",
                7 or 8 => "Важкий",
                9 or 10 => "Експертний",
                _ => "Невідомий"
            };
        }

        public static string GetStatusText(this ResultStatus status)
        {
            return status switch
            {
                ResultStatus.Passed => "Пройдено",
                ResultStatus.Failed => "Не пройдено",
                ResultStatus.Error => "Помилка",
                ResultStatus.Timeout => "Тайм-аут",
                ResultStatus.CompilationError => "Помилка компіляції",
                _ => "Невідомий"
            };
        }

        public static string GetStatusCssClass(this ResultStatus status)
        {
            return status switch
            {
                ResultStatus.Passed => "success",
                ResultStatus.Failed => "danger",
                ResultStatus.Error => "warning",
                ResultStatus.Timeout => "warning",
                ResultStatus.CompilationError => "danger",
                _ => "secondary"
            };
        }

        public static string ToTimeAgo(this DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow.Subtract(dateTime);

            return timeSpan switch
            {
                _ when timeSpan <= TimeSpan.FromSeconds(60) => "щойно",
                _ when timeSpan <= TimeSpan.FromMinutes(60) => $"{timeSpan.Minutes} хв. тому",
                _ when timeSpan <= TimeSpan.FromHours(24) => $"{timeSpan.Hours} год. тому",
                _ when timeSpan <= TimeSpan.FromDays(30) => $"{timeSpan.Days} дн. тому",
                _ => dateTime.ToString("dd.MM.yyyy")
            };
        }

        public static IEnumerable<IdentityError> AddErrorsFromResult(this ModelStateDictionary modelState, IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                modelState.AddModelError("", error.Description);
            }
            return result.Errors;
        }
    }
}