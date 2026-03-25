using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.API.Services;
using ResumeAnalyzer.Core.Interfaces;
using System.Security.Claims;

namespace ResumeAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IAnalysisRepository _analysisRepository;
        private readonly IResumeRepository _resumeRepository;
        private readonly ReportService _reportService;

        public ReportController(
            IAnalysisRepository analysisRepository,
            IResumeRepository resumeRepository,
            ReportService reportService)
        {
            _analysisRepository = analysisRepository;
            _resumeRepository = resumeRepository;
            _reportService = reportService;
        }

        // GET api/report/{resumeId}
        [HttpGet("{resumeId}")]
        public async Task<IActionResult> DownloadReport(int resumeId)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Check resume belongs to user
            var resume = await _resumeRepository.GetByIdAsync(resumeId);
            if (resume == null)
                return NotFound(new { message = "Resume not found." });

            if (resume.UserId != userId)
                return Forbid();

            // Get latest analysis
            var analysis = await _analysisRepository
                .GetByResumeIdAsync(resumeId);

            if (analysis == null)
                return NotFound(new { message = "No analysis found." });

            // Generate PDF
            var pdfBytes = _reportService.GenerateReport(
                analysis, resume.FileName);

            // Return as downloadable file
            return File(pdfBytes, "application/pdf",
                $"ResumeAnalysis_{resume.FileName}_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}