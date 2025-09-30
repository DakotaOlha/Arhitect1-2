using Laba1_2.Data;
using Laba1_2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Laba1_2.Services
{
    public class DbInitializer
    {
        public static async Task InitializeAsync(
            IServiceProvider serviceProvider,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Створюємо базу даних, якщо вона не існує
            await context.Database.MigrateAsync();

            // Створюємо ролі
            await CreateRolesAsync(roleManager);

            // Створюємо тестових користувачів
            await CreateUsersAsync(userManager);

            // Створюємо тестові завдання
            await CreateChallengesAsync(context, userManager);
        }

        private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Mentor", "Student" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task CreateUsersAsync(UserManager<User> userManager)
        {
            // Створюємо адміністратора
            var adminEmail = "admin@codelearn.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Створюємо ментора
            var mentorEmail = "mentor@codelearn.com";
            var mentorUser = await userManager.FindByEmailAsync(mentorEmail);
            if (mentorUser == null)
            {
                mentorUser = new User
                {
                    UserName = mentorEmail,
                    Email = mentorEmail,
                    FirstName = "John",
                    LastName = "Mentor",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(mentorUser, "Mentor123!");
                await userManager.AddToRoleAsync(mentorUser, "Mentor");
            }

            // Створюємо студента
            var studentEmail = "student@codelearn.com";
            var studentUser = await userManager.FindByEmailAsync(studentEmail);
            if (studentUser == null)
            {
                studentUser = new User
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    FirstName = "Jane",
                    LastName = "Student",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(studentUser, "Student123!");
                await userManager.AddToRoleAsync(studentUser, "Student");
            }
        }

        private static async Task CreateChallengesAsync(ApplicationDbContext context, UserManager<User> userManager)
        {
            // Перевіряємо, чи вже є завдання
            if (await context.Challenges.AnyAsync())
            {
                return;
            }

            var mentor = await userManager.FindByEmailAsync("mentor@codelearn.com");
            if (mentor == null) return;

            var challenges = new List<Challenge>
            {
                new Challenge
                {
                    Title = "Sum of Two Numbers",
                    Description = "Напишіть функцію, яка повертає суму двох чисел.",
                    Instructions = "Створіть функцію solution(a, b), яка приймає два цілі числа та повертає їх суму.\n\nПриклад:\nВхід: a = 5, b = 3\nВихід: 8\n\nВаша функція має читати два числа з stdin (по одному на рядок) та виводити їх суму.",
                    ExampleInput = "5\n3",
                    ExampleOutput = "8",
                    TestCases = "[{\"Input\":\"2\\n3\",\"ExpectedOutput\":\"5\"},{\"Input\":\"10\\n20\",\"ExpectedOutput\":\"30\"},{\"Input\":\"-5\\n5\",\"ExpectedOutput\":\"0\"},{\"Input\":\"100\\n200\",\"ExpectedOutput\":\"300\"}]",
                    DifficultyLevel = 1,
                    Points = 10,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = mentor.Id
                },
                new Challenge
                {
                    Title = "Reverse String",
                    Description = "Напишіть функцію, яка реверсує рядок.",
                    Instructions = "Створіть функцію solution(text), яка приймає рядок та повертає його у зворотному порядку.\n\nПриклад:\nВхід: 'hello'\nВихід: 'olleh'",
                    ExampleInput = "hello",
                    ExampleOutput = "olleh",
                    TestCases = "[{\"Input\":\"hello\",\"ExpectedOutput\":\"olleh\"},{\"Input\":\"world\",\"ExpectedOutput\":\"dlrow\"},{\"Input\":\"Python\",\"ExpectedOutput\":\"nohtyP\"},{\"Input\":\"12345\",\"ExpectedOutput\":\"54321\"}]",
                    DifficultyLevel = 2,
                    Points = 15,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = mentor.Id
                },
                new Challenge
                {
                    Title = "Find Maximum",
                    Description = "Знайдіть максимальне число в масиві.",
                    Instructions = "Створіть функцію solution(numbers), яка приймає список чисел та повертає найбільше число.\n\nПриклад:\nВхід: [1, 5, 3, 9, 2]\nВихід: 9",
                    ExampleInput = "1 5 3 9 2",
                    ExampleOutput = "9",
                    TestCases = "[{\"Input\":\"1 5 3 9 2\",\"ExpectedOutput\":\"9\"},{\"Input\":\"10 20 5 15\",\"ExpectedOutput\":\"20\"},{\"Input\":\"-5 -1 -10\",\"ExpectedOutput\":\"-1\"},{\"Input\":\"100\",\"ExpectedOutput\":\"100\"}]",
                    DifficultyLevel = 2,
                    Points = 15,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = mentor.Id
                },
                new Challenge
                {
                    Title = "Palindrome Check",
                    Description = "Перевірте, чи є рядок паліндромом.",
                    Instructions = "Створіть функцію solution(text), яка перевіряє, чи є рядок паліндромом (читається однаково в обидва боки).\n\nПриклад:\nВхід: 'racecar'\nВихід: True\n\nВхід: 'hello'\nВихід: False",
                    ExampleInput = "racecar",
                    ExampleOutput = "True",
                    TestCases = "[{\"Input\":\"racecar\",\"ExpectedOutput\":\"True\"},{\"Input\":\"hello\",\"ExpectedOutput\":\"False\"},{\"Input\":\"level\",\"ExpectedOutput\":\"True\"},{\"Input\":\"python\",\"ExpectedOutput\":\"False\"}]",
                    DifficultyLevel = 3,
                    Points = 20,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = mentor.Id
                },
                new Challenge
                {
                    Title = "Fibonacci Number",
                    Description = "Обчисліть n-те число Фібоначчі.",
                    Instructions = "Створіть функцію solution(n), яка повертає n-те число Фібоначчі.\n\nПослідовність Фібоначчі: 0, 1, 1, 2, 3, 5, 8, 13...\n\nПриклад:\nВхід: 6\nВихід: 8",
                    ExampleInput = "6",
                    ExampleOutput = "8",
                    TestCases = "[{\"Input\":\"0\",\"ExpectedOutput\":\"0\"},{\"Input\":\"1\",\"ExpectedOutput\":\"1\"},{\"Input\":\"6\",\"ExpectedOutput\":\"8\"},{\"Input\":\"10\",\"ExpectedOutput\":\"55\"}]",
                    DifficultyLevel = 4,
                    Points = 25,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = mentor.Id
                }
            };

            context.Challenges.AddRange(challenges);
            await context.SaveChangesAsync();

            // Додаємо підтримку мов для кожного завдання
            foreach (var challenge in challenges)
            {
                // Python
                context.ChallengeLanguages.Add(new ChallengeLanguage
                {
                    ChallengeId = challenge.Id,
                    LanguageId = 2, // Python
                    StarterCode = "def solution(*args):\n    # Write your code here\n    pass\n\nif __name__ == \"__main__\":\n    import sys\n    args = [line.strip() for line in sys.stdin]\n    result = solution(*args)\n    print(result)"
                });

                // C#
                context.ChallengeLanguages.Add(new ChallengeLanguage
                {
                    ChallengeId = challenge.Id,
                    LanguageId = 1, // C#
                    StarterCode = "using System;\n\nclass Program\n{\n    static object Solution(params string[] args)\n    {\n        // Write your code here\n        return null;\n    }\n\n    static void Main()\n    {\n        // Read input and call solution\n    }\n}"
                });
            }

            await context.SaveChangesAsync();
        }
    }
}