using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Laba1_2.Models;

namespace Laba1_2.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<Solution> Solutions { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<ChallengeLanguage> ChallengeLanguages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Bio)
                    .HasMaxLength(500);

                entity.HasIndex(e => e.Email)
                    .IsUnique();
            });

            builder.Entity<Challenge>(entity =>
            {
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .IsRequired();

                entity.Property(e => e.Instructions)
                    .IsRequired();

                entity.Property(e => e.TestCases)
                    .IsRequired();

                entity.Property(e => e.DifficultyLevel)
                    .HasDefaultValue(1);

                entity.Property(e => e.Points)
                    .HasDefaultValue(10);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(e => e.CreatedChallenges)
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Title);
                entity.HasIndex(e => e.DifficultyLevel);
                entity.HasIndex(e => e.IsActive);
            });

            builder.Entity<Language>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Extension)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Version)
                    .HasMaxLength(100);

                entity.Property(e => e.SyntaxHighlighting)
                    .HasMaxLength(50);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.HasIndex(e => e.Name)
                    .IsUnique();
            });

            builder.Entity<Solution>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired();

                entity.Property(e => e.SubmittedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.IsSuccessful)
                    .HasDefaultValue(false);

                entity.Property(e => e.ExecutionTimeMs)
                    .HasDefaultValue(0);

                entity.Property(e => e.PointsEarned)
                    .HasDefaultValue(0);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.Solutions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Challenge)
                    .WithMany(e => e.Solutions)
                    .HasForeignKey(e => e.ChallengeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Language)
                    .WithMany(e => e.Solutions)
                    .HasForeignKey(e => e.LanguageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.UserId, e.ChallengeId });
                entity.HasIndex(e => e.SubmittedAt);
            });

            builder.Entity<Result>(entity =>
            {
                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.ExecutionTimeMs)
                    .HasDefaultValue(0);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.User)
                    .WithMany(e => e.Results)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Challenge)
                    .WithMany(e => e.Results)
                    .HasForeignKey(e => e.ChallengeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Solution)
                    .WithMany(e => e.Results)
                    .HasForeignKey(e => e.SolutionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });

            builder.Entity<ChallengeLanguage>(entity =>
            {
                entity.HasOne(e => e.Challenge)
                    .WithMany(e => e.ChallengeLanguages)
                    .HasForeignKey(e => e.ChallengeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Language)
                    .WithMany(e => e.ChallengeLanguages)
                    .HasForeignKey(e => e.LanguageId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.ChallengeId, e.LanguageId })
                    .IsUnique();
            });

            SeedData(builder);
        }

        private void SeedData(ModelBuilder builder)
        {
            builder.Entity<Language>().HasData(
                new Language { Id = 1, Name = "C#", Extension = ".cs", Version = "12.0", IsActive = true, SyntaxHighlighting = "csharp" },
                new Language { Id = 2, Name = "Python", Extension = ".py", Version = "3.11", IsActive = true, SyntaxHighlighting = "python" },
                new Language { Id = 3, Name = "JavaScript", Extension = ".js", Version = "ES2023", IsActive = true, SyntaxHighlighting = "javascript" },
                new Language { Id = 4, Name = "Java", Extension = ".java", Version = "17", IsActive = true, SyntaxHighlighting = "java" },
                new Language { Id = 5, Name = "C++", Extension = ".cpp", Version = "20", IsActive = true, SyntaxHighlighting = "cpp" }
            );
        }
    }
}