namespace Laba1_2.Utilities
{
    public static class Constants
    {
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Mentor = "Mentor";
            public const string Student = "Student";
        }

        public static class Policies
        {
            public const string RequireAdminRole = "RequireAdminRole";
            public const string RequireMentorRole = "RequireMentorRole";
            public const string RequireStudentRole = "RequireStudentRole";
        }

        public static class ClaimTypes
        {
            public const string Permission = "permission";
        }

        public static class Permissions
        {
            public const string ManageUsers = "manage_users";
            public const string CreateChallenges = "create_challenges";
            public const string SolveChallenges = "solve_challenges";
            public const string ViewReports = "view_reports";
        }

        public static class Messages
        {
            public const string Success = "Операція виконана успішно!";
            public const string Error = "Виникла помилка при виконанні операції.";
            public const string NotFound = "Запитаний ресурс не знайдено.";
            public const string AccessDenied = "У вас немає прав для виконання цієї дії.";
            public const string InvalidInput = "Некоректні вхідні дані.";
        }

        public static class DefaultValues
        {
            public const int DefaultPageSize = 10;
            public const int MaxPageSize = 50;
            public const int DefaultTimeout = 30; // seconds
            public const int DefaultMaxMemory = 128; // MB
        }
    }
}