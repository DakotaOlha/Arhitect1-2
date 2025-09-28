using Laba1_2.Models;
using System.Diagnostics;
using System.Text.Json;

namespace Laba1_2.Services
{
    public interface ICodeExecutionService
    {
        Task<ExecutionResult> ExecuteCodeAsync(string code, Language language, string testCases);
    }

    public class ExecutionResult
    {
        public bool IsSuccessful { get; set; }
        public string? Output { get; set; }
        public string? ErrorMessage { get; set; }
        public int ExecutionTimeMs { get; set; }
        public List<TestCaseResult> TestResults { get; set; } = new();
    }

    public class TestCaseResult
    {
        public string Input { get; set; } = string.Empty;
        public string ExpectedOutput { get; set; } = string.Empty;
        public string ActualOutput { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string? ErrorMessage { get; set; }
        public int ExecutionTimeMs { get; set; }
    }
}