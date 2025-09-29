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
                var testCaseList = JsonSerializer.Deserialize<List<TestCase>>(testCases);
                if (testCaseList == null || !testCaseList.Any())
                {
                    result.ErrorMessage = "No test cases provided";
                    return result;
                }

                foreach (var testCase in testCaseList)
                {
                    var testResult = await ExecuteSingleTestCase(code, language, testCase);
                    result.TestResults.Add(testResult);
                }

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

                if (!string.IsNullOrEmpty(input))
                {
                    await process.StandardInput.WriteAsync(input);
                }
                process.StandardInput.Close();

                var waitTask = process.WaitForExitAsync();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));

                var completed = await Task.WhenAny(waitTask, timeoutTask);
                if (completed != waitTask)
                {
                    try
                    {
                        if (!process.HasExited) process.Kill();
                    }
                    catch { /* ігноруємо */ }

                    throw new TimeoutException("Code execution timed out");
                }

                await waitTask;

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
                catch { /* не критично */ }
            }
        }

        private async Task<string> ExecuteCSharp(string code, string input)
        {
            await Task.Delay(100);
            return "C# execution not implemented yet";
        }

        private async Task<string> ExecuteJavaScript(string code, string input)
        {
            await Task.Delay(100);
            return "JavaScript execution not implemented yet";
        }
    }

    public class TestCase
    {
        public string Input { get; set; } = string.Empty;
        public string ExpectedOutput { get; set; } = string.Empty;
    }
}