using System;
using System.Collections.Generic;
using System.Text;

namespace ResumeAnalyzer.Core.Models
{
    internal class JobMatch
    {
        public int Id { get; set; }
        public int AnalysisId { get; set; }
        public string JobDescription { get; set; } = string.Empty;
        public int MatchScore { get; set; }
        public string MissingKeywords { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Analysis Analysis { get; set; } = null!;
    }
}
