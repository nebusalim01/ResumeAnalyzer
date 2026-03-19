using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ResumeAnalyzer.Core.DTOs;
using ResumeAnalyzer.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ResumeAnalyzer.Infrastructure.Services
{
    public class GeminiService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string API_URL =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        public GeminiService(HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"]!;
        }

        public async Task<AnalysisResultDto> AnalyzeResumeAsync(
            string resumeText, string? jobDescription = null)
        {
            var prompt = BuildPrompt(resumeText, jobDescription);

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.5,
                    maxOutputTokens = 1500
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8,
                "application/json");

            // Gemini uses API key as header
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "x-goog-api-key", _apiKey);

            var url = API_URL;
            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Gemini API error {(int)response.StatusCode}: {responseString}");
            }

            var geminiResponse = JsonSerializer.Deserialize<JsonElement>(
                responseString);

            var resultText = geminiResponse
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()!;

            return ParseResult(resultText, jobDescription != null);
        }

        private string BuildPrompt(string resumeText, string? jobDescription)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("You are an expert HR consultant and ATS resume reviewer.");
            prompt.AppendLine("Analyze the resume below and return a JSON object with these exact keys:");
            prompt.AppendLine("- ats_score: integer between 0 and 100");
            prompt.AppendLine("- skills: array of skill strings found in resume");
            prompt.AppendLine("- experience_summary: string summarizing work experience");
            prompt.AppendLine("- suggestions: array of exactly 5 improvement suggestion strings");
            prompt.AppendLine("- strengths: array of strength strings");

            if (jobDescription != null)
            {
                prompt.AppendLine("- job_match_score: integer between 0 and 100");
                prompt.AppendLine("- missing_keywords: array of keyword strings missing from resume");
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
            prompt.AppendLine("IMPORTANT: Return ONLY valid JSON. No explanation. No markdown. No code blocks.");
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

            var raw = JsonSerializer.Deserialize<JsonElement>(cleaned, options);

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