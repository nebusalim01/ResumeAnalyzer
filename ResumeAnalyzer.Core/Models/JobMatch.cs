using System;

namespace ResumeAnalyzer.Core.Models
{
    public class JobMatch
    {
        public int Id { get; set; }
        public int AnalysisId { get; set; }
        public string JobDescription { get; set; } = string.Empty;
        public int MatchScore { get; set; }
        public string MissingKeywords { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Analysis Analysis { get; set; } = null!;
    }
}