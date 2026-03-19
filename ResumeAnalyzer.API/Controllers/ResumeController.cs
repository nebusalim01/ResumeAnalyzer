using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.Core.DTOs;
using ResumeAnalyzer.Core.Interfaces;
using ResumeAnalyzer.Core.Models;
using ResumeAnalyzer.Infrastructure.Services;
using System.Drawing;
using System.Security.Claims;

namespace ResumeAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResumeController : ControllerBase
    {
        private readonly IResumeRepository _resumeRepository;
        private readonly IAnalysisRepository _analysisRepository;
        private readonly IOpenAIService _openAIService;
        private readonly ResumeParserService _parserService;
        private readonly IWebHostEnvironment _environment;

        public ResumeController(
            IResumeRepository resumeRepository,
            IAnalysisRepository analysisRepository,
            IOpenAIService openAIService,
            ResumeParserService parserService,
            IWebHostEnvironment environment)
        {
            _resumeRepository = resumeRepository;
            _analysisRepository = analysisRepository;
            _openAIService = openAIService;
            _parserService = parserService;
            _environment = environment;
        }

        // POST api/resume/upload
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(
            IFormFile file,
            [FromForm] string? jobDescription = null)
        {
            // Step 1 — Validate file
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Please upload a file." });

            var allowedExtensions = new[] { ".pdf", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Only PDF and DOCX files are allowed." });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "File size must be less than 5MB." });

            // Step 2 — Get logged in user ID from token
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Step 3 — Save file to Uploads folder
            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            // Step 4 — Parse text from file
            string parsedText;
            try
            {
                parsedText = _parserService.ParseResume(filePath);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to parse file: {ex.Message}" });
            }

            // Step 5 — Save resume to database
            var resume = new Resume
            {
                UserId = userId,
                FileName = file.FileName,
                FilePath = filePath,
                ParsedText = parsedText
            };

            await _resumeRepository.AddAsync(resume);

            // Step 6 — Analyze with OpenAI
            AnalysisResultDto analysisResult;
            try
            {
                analysisResult = await _openAIService
                    .AnalyzeResumeAsync(parsedText, jobDescription);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"AI analysis failed: {ex.Message}" });
            }

            // Step 7 — Save analysis to database
            var analysis = new Analysis
            {
                ResumeId = resume.Id,
                ATSScore = analysisResult.AtsScore,
                SkillsJson = System.Text.Json.JsonSerializer
                                        .Serialize(analysisResult.Skills),
                SuggestionsJson = System.Text.Json.JsonSerializer
                                        .Serialize(analysisResult.Suggestions),
                ExperienceSummary = analysisResult.ExperienceSummary,
                Strengths = System.Text.Json.JsonSerializer
                                        .Serialize(analysisResult.Strengths)
            };

            // Step 8 — Save job match if job description provided
            if (!string.IsNullOrWhiteSpace(jobDescription) &&
                analysisResult.JobMatchScore.HasValue)
            {
                analysis.JobMatch = new JobMatch
                {
                    JobDescription = jobDescription,
                    MatchScore = analysisResult.JobMatchScore.Value,
                    MissingKeywords = System.Text.Json.JsonSerializer
                                          .Serialize(analysisResult.MissingKeywords)
                };
            }

            await _analysisRepository.AddAsync(analysis);

            return Ok(analysisResult);
        }

        // GET api/resume/myresumes
        [HttpGet("myresumes")]
        public async Task<IActionResult> GetMyResumes()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var resumes = await _resumeRepository.GetByUserIdAsync(userId);

            var result = resumes.Select(r => new ResumeDto
            {
                Id = r.Id,
                FileName = r.FileName,
                UploadedAt = r.UploadedAt,
                LatestAtsScore = r.Analyses.Any()
                    ? r.Analyses.OrderByDescending(a => a.CreatedAt)
                                .First().ATSScore
                    : null
            });

            return Ok(result);
        }

        // GET api/resume/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResume(int id)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var resume = await _resumeRepository.GetByIdAsync(id);

            if (resume == null)
                return NotFound(new { message = "Resume not found." });

            // Make sure user can only access their own resumes
            if (resume.UserId != userId)
                return Forbid();

            return Ok(new ResumeDto
            {
                Id = resume.Id,
                FileName = resume.FileName,
                UploadedAt = resume.UploadedAt,
                LatestAtsScore = resume.Analyses.Any()
                    ? resume.Analyses.OrderByDescending(a => a.CreatedAt)
                                     .First().ATSScore
                    : null
            });
        }

        // DELETE api/resume/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResume(int id)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var resume = await _resumeRepository.GetByIdAsync(id);

            if (resume == null)
                return NotFound(new { message = "Resume not found." });

            // Make sure user can only delete their own resumes
            if (resume.UserId != userId)
                return Forbid();

            // Delete file from server
            if (System.IO.File.Exists(resume.FilePath))
                System.IO.File.Delete(resume.FilePath);

            await _resumeRepository.DeleteAsync(id);

            return Ok(new { message = "Resume deleted successfully." });
        }
    }
}
