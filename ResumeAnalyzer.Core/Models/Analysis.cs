using System;
using System.Collections.Generic;

namespace ResumeAnalyzer.Core.Models
{
    public class Analysis
    {
        public int Id { get; set; }
        public int ResumeId { get; set; }
        public int ATSScore { get; set; }
        public string SkillsJson { get; set; } = string.Empty;
        public string SuggestionsJson { get; set; } = string.Empty;
        public string ExperienceSummary { get; set; } = string.Empty;
        public string Strengths { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Resume Resume { get; set; } = null!;
        public JobMatch? JobMatch { get; set; }
    }
}