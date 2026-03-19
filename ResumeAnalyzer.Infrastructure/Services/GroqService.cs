using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ResumeAnalyzer.Core.DTOs;
using ResumeAnalyzer.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ResumeAnalyzer.Infrastructure.Services
{
    public class GroqService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string API_URL =
            "https://api.groq.com/openai/v1/chat/completions";

        public GroqService(HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Groq:ApiKey"]!;
        }

        public async Task<AnalysisResultDto> AnalyzeResumeAsync(
            string resumeText, string? jobDescription = null)
        {
            var prompt = BuildPrompt(resumeText, jobDescription);

            var requestBody = new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "You are an expert HR consultant and ATS resume reviewer. " +
                                  "Always respond with valid JSON only. " +
                                  "No explanation, no markdown, no code blocks."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.5,
                max_tokens = 1500
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsync(API_URL, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Log full error for debugging
                Console.WriteLine($"Groq Error: {responseString}");
                throw new Exception(
                    $"Groq API error {(int)response.StatusCode}: {responseString}");
            }

            var groqResponse = JsonSerializer.Deserialize<JsonElement>(
                responseString);

            var resultText = groqResponse
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()!;

            return ParseResult(resultText, jobDescription != null);
        }

        private string BuildPrompt(string resumeText, string? jobDescription)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("Analyze the resume and return JSON with these exact keys:");
            prompt.AppendLine("- ats_score: integer 0-100");
            prompt.AppendLine("- skills: array of strings");
            prompt.AppendLine("- experience_summary: string");
            prompt.AppendLine("- suggestions: array of exactly 5 strings");
            prompt.AppendLine("- strengths: array of strings");

            if (jobDescription != null)
            {
                prompt.AppendLine("- job_match_score: integer 0-100");
                prompt.AppendLine("- missing_keywords: array of strings");
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