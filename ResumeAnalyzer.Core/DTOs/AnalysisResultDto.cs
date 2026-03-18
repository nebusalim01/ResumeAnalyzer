using System;
using System.Collections.Generic;
using System.Text;

namespace ResumeAnalyzer.Core.DTOs
{
    public class AnalysisResultDto
    {
        public int AtsScore { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public List<string> Suggestions { get; set; } = new List<string>();
        public string ExperienceSummary { get; set; } = string.Empty;
        public List<string> Strengths { get; set; } = new List<string>();
        public int? JobMatchScore { get; set; }
        public List<string> MissingKeywords { get; set; } = new List<string>();
    }
}
