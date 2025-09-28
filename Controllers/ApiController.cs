using Laba1_2.Models;
using Laba1_2.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laba1_2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApiController : ControllerBase
    {
        private readonly IChallengeService _challengeService;
        private readonly ICodeExecutionService _codeExecutionService;

        public ApiController(
            IChallengeService challengeService,
            ICodeExecutionService codeExecutionService)
        {
            _challengeService = challengeService;
            _codeExecutionService = codeExecutionService;
        }

        [HttpGet("challenges")]
        public async Task<IActionResult> GetChallenges(int? difficulty = null)
        {
            try
            {
                var challenges = difficulty.HasValue
                    ? await _challengeService.GetChallengesByDifficultyAsync(difficulty.Value)
                    : await _challengeService.GetActivesChallengesAsync();

                return Ok(challenges);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("challenges/{id}")]
        public async Task<IActionResult> GetChallenge(int id)
        {
            try
            {
                var challenge = await _challengeService.GetChallengeByIdAsync(id);
                if (challenge == null)
                    return NotFound();

                return Ok(challenge);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteCode([FromBody] ExecuteCodeRequest request)
        {
            try
            {
                var language = new Language
                {
                    Name = request.Language,
                    Extension = GetLanguageExtension(request.Language)
                };

                var result = await _codeExecutionService.ExecuteCodeAsync(
                    request.Code, language, request.TestCases);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static string GetLanguageExtension(string languageName)
        {
            return languageName.ToLower() switch
            {
                "python" => ".py",
                "csharp" or "c#" => ".cs",
                "javascript" => ".js",
                "java" => ".java",
                "cpp" or "c++" => ".cpp",
                _ => ".txt"
            };
        }
    }

    public class ExecuteCodeRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string TestCases { get; set; } = string.Empty;
    }
}