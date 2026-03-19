namespace ResumeAnalzer.Web.Models
{
    public class ResumeDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public int? LatestAtsScore { get; set; }
    }

    public class AnalysisResult
    {
        public int AtsScore { get; set; }
        public List<string> Skills { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public string ExperienceSummary { get; set; } = string.Empty;
        public List<string> Strengths { get; set; } = new();
        public int? JobMatchScore { get; set; }
        public List<string> MissingKeywords { get; set; } = new();
    }

    public class JobMatchRequest
    {
        public int ResumeId { get; set; }
        public string JobDescription { get; set; } = string.Empty;
    }

    public class HistoryItem
    {
        public int AnalysisId { get; set; }
        public int ResumeId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public int ATSScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? JobMatchScore { get; set; }
    }
}