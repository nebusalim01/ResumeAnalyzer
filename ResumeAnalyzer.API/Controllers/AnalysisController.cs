using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.Core.DTOs;
using ResumeAnalyzer.Core.Interfaces;
using ResumeAnalyzer.Core.Models;
using System.Security.Claims;
using System.Text.Json;

namespace ResumeAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisRepository _analysisRepository;
        private readonly IResumeRepository _resumeRepository;
        private readonly IOpenAIService _openAIService;

        public AnalysisController(
            IAnalysisRepository analysisRepository,
            IResumeRepository resumeRepository,
            IOpenAIService openAIService)
        {
            _analysisRepository = analysisRepository;
            _resumeRepository = resumeRepository;
            _openAIService = openAIService;
        }

        // GET api/analysis/{resumeId}
        [HttpGet("{resumeId}")]
        public async Task<IActionResult> GetAnalysis(int resumeId)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Check resume belongs to user
            var resume = await _resumeRepository.GetByIdAsync(resumeId);
            if (resume == null)
                return NotFound(new { message = "Resume not found." });

            if (resume.UserId != userId)
                return Forbid();

            // Get latest analysis for this resume
            var analysis = await _analysisRepository.GetByResumeIdAsync(resumeId);
            if (analysis == null)
                return NotFound(new { message = "No analysis found for this resume." });

            return Ok(MapToDto(analysis));
        }

        // GET api/analysis/history
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var analyses = await _analysisRepository
                .GetHistoryByUserIdAsync(userId);

            var result = analyses.Select(a => new
            {
                AnalysisId = a.Id,
                ResumeId = a.ResumeId,
                FileName = a.Resume.FileName,
                ATSScore = a.ATSScore,
                CreatedAt = a.CreatedAt,
                JobMatchScore = a.JobMatch?.MatchScore
            });

            return Ok(result);
        }

        // POST api/analysis/jobmatch
        [HttpPost("jobmatch")]
        public async Task<IActionResult> JobMatch([FromBody] JobMatchDto dto)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Check resume belongs to user
            var resume = await _resumeRepository.GetByIdAsync(dto.ResumeId);
            if (resume == null)
                return NotFound(new { message = "Resume not found." });

            if (resume.UserId != userId)
                return Forbid();

            // Analyze with job description
            AnalysisResultDto analysisResult;
            try
            {
                analysisResult = await _openAIService.AnalyzeResumeAsync(
                    resume.ParsedText, dto.JobDescription);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { message = $"AI analysis failed: {ex.Message}" });
            }

            // Save new analysis with job match
            var analysis = new Analysis
            {
                ResumeId = resume.Id,
                ATSScore = analysisResult.AtsScore,
                SkillsJson = JsonSerializer.Serialize(analysisResult.Skills),
                SuggestionsJson = JsonSerializer.Serialize(analysisResult.Suggestions),
                ExperienceSummary = analysisResult.ExperienceSummary,
                Strengths = JsonSerializer.Serialize(analysisResult.Strengths),
                JobMatch = new JobMatch
                {
                    JobDescription = dto.JobDescription,
                    MatchScore = analysisResult.JobMatchScore ?? 0,
                    MissingKeywords = JsonSerializer
                        .Serialize(analysisResult.MissingKeywords)
                }
            };

            await _analysisRepository.AddAsync(analysis);

            return Ok(analysisResult);
        }

        // ── Helper ───────────────────────────────────────────
        private AnalysisResultDto MapToDto(Analysis analysis)
        {
            return new AnalysisResultDto
            {
                AtsScore = analysis.ATSScore,
                ExperienceSummary = analysis.ExperienceSummary,
                Skills = DeserializeList(analysis.SkillsJson),
                Suggestions = DeserializeList(analysis.SuggestionsJson),
                Strengths = DeserializeList(analysis.Strengths),
                JobMatchScore = analysis.JobMatch?.MatchScore,
                MissingKeywords = analysis.JobMatch != null
                    ? DeserializeList(analysis.JobMatch.MissingKeywords)
                    : new List<string>()
            };
        }

        private List<string> DeserializeList(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json)
                    ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}