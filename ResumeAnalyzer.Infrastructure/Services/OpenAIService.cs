using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ResumeAnalyzer.Core.DTOs;
using ResumeAnalyzer.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ResumeAnalyzer.Infrastructure.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string API_URL =
            "https://api.openai.com/v1/chat/completions";

        public OpenAIService(HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenAI:ApiKey"]!;
        }

        public async Task<AnalysisResultDto> AnalyzeResumeAsync(
    string resumeText, string? jobDescription = null)
        {
            // ── MOCK RESPONSE FOR TESTING ──────────────────
            await Task.Delay(2000); // Simulate API call delay

            return new AnalysisResultDto
            {
                AtsScore = 75,
                ExperienceSummary = "Experienced software developer with strong " +
                    "background in .NET and web technologies. " +
                    "Demonstrated ability to work in agile teams and deliver " +
                    "high quality solutions on time.",
                Skills = new List<string>
        {
            "C#", ".NET", "SQL Server", "HTML", "CSS",
            "JavaScript", "Git", "REST API", "Blazor"
        },
                Suggestions = new List<string>
        {
            "Add more quantifiable achievements to your experience section.",
            "Include relevant certifications at the top of your resume.",
            "Use more industry-specific keywords for better ATS scoring.",
            "Add a professional summary at the beginning of your resume.",
            "Include links to your GitHub or portfolio projects."
        },
                Strengths = new List<string>
        {
            "Strong technical skill set",
            "Relevant work experience",
            "Clear and well organized format",
            "Good educational background"
        },
                JobMatchScore = jobDescription != null ? 68 : null,
                MissingKeywords = jobDescription != null
                    ? new List<string>
                    {
                "Azure", "Docker", "Microservices",
                "Agile", "Scrum", "CI/CD"
                    }
                    : new List<string>()
            };
            // ── END MOCK RESPONSE ──────────────────────────
        }

        private string BuildPrompt(string resumeText, string? jobDescription)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("Analyze the resume and return JSON with these exact keys:");
            prompt.AppendLine("ats_score: integer 0-100");
            prompt.AppendLine("skills: array of strings");
            prompt.AppendLine("experience_summary: string");
            prompt.AppendLine("suggestions: array of 5 strings");
            prompt.AppendLine("strengths: array of strings");

            if (jobDescription != null)
            {
                prompt.AppendLine("job_match_score: integer 0-100");
                prompt.AppendLine("missing_keywords: array of strings");
            }

            prompt.AppendLine();
            prompt.AppendLine("Resume Text:");
            prompt.AppendLine(resumeText);

            if (jobDescription != null)
            {
                prompt.AppendLine();
                prompt.AppendLine("Job Description:");
                prompt.AppendLine(jobDescription);
            }

            prompt.AppendLine();
            prompt.AppendLine("Return ONLY valid JSON. Nothing else.");
            return prompt.ToString();
        }

        private AnalysisResultDto ParseResult(string resultText,
            bool hasJobMatch)
        {
            var cleaned = resultText
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var raw = JsonSerializer.Deserialize<JsonElement>(
                cleaned, options);

            var result = new AnalysisResultDto
            {
                AtsScore = raw.GetProperty("ats_score").GetInt32(),
                ExperienceSummary = raw.GetProperty(
                    "experience_summary").GetString()!,
                Skills = ParseStringArray(raw, "skills"),
                Suggestions = ParseStringArray(raw, "suggestions"),
                Strengths = ParseStringArray(raw, "strengths")
            };

            if (hasJobMatch)
            {
                result.JobMatchScore = raw.GetProperty(
                    "job_match_score").GetInt32();
                result.MissingKeywords = ParseStringArray(
                    raw, "missing_keywords");
            }

            return result;
        }

        private List<string> ParseStringArray(JsonElement element,
            string propertyName)
        {
            var list = new List<string>();
            if (element.TryGetProperty(propertyName, out var arrayElement))
            {
                foreach (var item in arrayElement.EnumerateArray())
                {
                    var val = item.GetString();
                    if (!string.IsNullOrWhiteSpace(val))
                        list.Add(val);
                }
            }
            return list;
        }
    }
}