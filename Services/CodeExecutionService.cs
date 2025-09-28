using Laba1_2.Models;
using System.Diagnostics;
using System.Text.Json;

namespace Laba1_2.Services
{
    public class CodeExecutionService : ICodeExecutionService
    {
        private readonly ILogger<CodeExecutionService> _logger;

        public CodeExecutionService(ILogger<CodeExecutionService> logger)
        {
            _logger = logger;
        }

        public async Task<ExecutionResult> ExecuteCodeAsync(string code, Language language, string testCases)
        {
            var result = new ExecutionResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Parse test cases from JSON
                var testCaseList = JsonSerializer.Deserialize<List<TestCase>>(testCases);
                if (testCaseList == null || !testCaseList.Any())
                {
                    result.ErrorMessage = "No test cases provided";
                    return result;
                }

                // Execute code for each test case
                foreach (var testCase in testCaseList)
                {
                    var testResult = await ExecuteSingleTestCase(code, language, testCase);
                    result.TestResults.Add(testResult);
                }

                // Determine overall success
                result.IsSuccessful = result.TestResults.All(t => t.Passed);
                result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing code");
                result.ErrorMessage = ex.Message;
                result.IsSuccessful = false;
            }
            finally
            {
                stopwatch.Stop();
            }

            return result;
        }

        private async Task<TestCaseResult> ExecuteSingleTestCase(string code, Language language, TestCase testCase)
        {
            var testResult = new TestCaseResult
            {
                Input = testCase.Input,
                ExpectedOutput = testCase.ExpectedOutput
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // This is a simplified implementation
                // In a real application, you would execute code in a sandboxed environment
                var actualOutput = await ExecuteCodeInSandbox(code, language, testCase.Input);

                testResult.ActualOutput = actualOutput.Trim();
                testResult.Passed = testResult.ActualOutput.Equals(testCase.ExpectedOutput.Trim(), StringComparison.OrdinalIgnoreCase);
                testResult.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                testResult.ErrorMessage = ex.Message;
                testResult.Passed = false;
            }
            finally
            {
                stopwatch.Stop();
            }

            return testResult;
        }

        private async Task<string> ExecuteCodeInSandbox(string code, Language language, string input)
        {
            // WARNING: This is a simplified implementation for demonstration purposes
            // In a production environment, you should use proper sandboxing (Docker, etc.)

            switch (language.Name.ToLower())
            {
                case "python":
                    return await ExecutePython(code, input);
                case "csharp":
                case "c#":
                    return await ExecuteCSharp(code, input);
                case "javascript":
                    return await ExecuteJavaScript(code, input);
                default:
                    throw new NotSupportedException($"Language {language.Name} is not supported");
            }
        }

        private async Task<string> ExecutePython(string code, string input)
        {
            // Генеруємо унікальний тимчасовий файл
            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".py");

            try
            {
                await File.WriteAllTextAsync(tempFile, code);

                var processInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{tempFile}\"",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processInfo, EnableRaisingEvents = true };
                process.Start();

                // Надати вхідні дані (якщо є)
                if (!string.IsNullOrEmpty(input))
                {
                    await process.StandardInput.WriteAsync(input);
                }
                process.StandardInput.Close();

                // Очікуємо завершення або таймаут
                var waitTask = process.WaitForExitAsync();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));

                var completed = await Task.WhenAny(waitTask, timeoutTask);
                if (completed != waitTask)
                {
                    // таймаут — намагаємось вбити процес і викинути виняток
                    try
                    {
                        if (!process.HasExited) process.Kill();
                    }
                    catch { /* ігноруємо помилки при вбиванні процесу */ }

                    throw new TimeoutException("Code execution timed out");
                }

                // гарантуємо, що процес завершився
                await waitTask;

                // Зчитуємо вивід та помилки
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Execution error: {error}");
                }

                return output;
            }
            finally
            {
                try
                {
                    if (File.Exists(tempFile))
                        File.Delete(tempFile);
                }
                catch { /* не критично, можна логувати */ }
            }
        }

        private async Task<string> ExecuteCSharp(string code, string input)
        {
            // This would require more complex implementation with Roslyn compiler
            // For now, return a placeholder
            await Task.Delay(100); // Simulate execution time
            return "C# execution not implemented yet";
        }

        private async Task<string> ExecuteJavaScript(string code, string input)
        {
            // This would require Node.js integration
            // For now, return a placeholder
            await Task.Delay(100); // Simulate execution time
            return "JavaScript execution not implemented yet";
        }
    }

    public class TestCase
    {
        public string Input { get; set; } = string.Empty;
        public string ExpectedOutput { get; set; } = string.Empty;
    }
}