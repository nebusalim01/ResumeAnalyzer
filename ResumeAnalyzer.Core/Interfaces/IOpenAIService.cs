using ResumeAnalyzer.Core.DTOs;

namespace ResumeAnalyzer.Core.Interfaces
{
    public interface IOpenAIService
    {
        Task<AnalysisResultDto> AnalyzeResumeAsync(string resumeText, string? jobDescription = null);
    }
}