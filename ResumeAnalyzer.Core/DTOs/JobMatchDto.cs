using System;
using System.Collections.Generic;
using System.Text;

namespace ResumeAnalyzer.Core.DTOs
{
    public class JobMatchDto
    {
        public int ResumeId { get; set; }
        public string JobDescription { get; set; } = string.Empty;
    }
}
