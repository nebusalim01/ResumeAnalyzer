using System;
using System.Collections.Generic;

namespace ResumeAnalyzer.Core.Models
{
    public class Resume
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ParsedText { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<Analysis> Analyses { get; set; } = new List<Analysis>();
    }
}