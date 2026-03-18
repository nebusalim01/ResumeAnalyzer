using System;
using System.Collections.Generic;
using System.Text;

namespace ResumeAnalyzer.Core.DTOs
{
    public class ResumeDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public int? LatestAtsScore { get; set; }
    }
}
