using ResumeAnalzer.Web.Models;
using System.Net.Http.Json;

namespace ResumeAnalzer.Web.Services
{
    public class ResumeService
    {
        private readonly HttpClient _httpClient;

        public ResumeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AnalysisResult?> UploadResumeAsync(
            Stream fileStream,
            string fileName,
            string? jobDescription = null)
        {
            var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(fileStream);
            content.Add(fileContent, "file", fileName);

            if (!string.IsNullOrWhiteSpace(jobDescription))
                content.Add(new StringContent(jobDescription),
                    "jobDescription");

            var response = await _httpClient.PostAsync(
                "api/resume/upload", content);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<AnalysisResult>();
        }

        public async Task<List<ResumeDto>> GetMyResumesAsync()
        {
            var result = await _httpClient
                .GetFromJsonAsync<List<ResumeDto>>("api/resume/myresumes");
            return result ?? new List<ResumeDto>();
        }

        public async Task<AnalysisResult?> GetAnalysisAsync(int resumeId)
        {
            return await _httpClient
                .GetFromJsonAsync<AnalysisResult>(
                    $"api/analysis/{resumeId}");
        }

        public async Task<List<HistoryItem>> GetHistoryAsync()
        {
            var result = await _httpClient
                .GetFromJsonAsync<List<HistoryItem>>("api/analysis/history");
            return result ?? new List<HistoryItem>();
        }

        public async Task<bool> DeleteResumeAsync(int id)
        {
            var response = await _httpClient
                .DeleteAsync($"api/resume/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}