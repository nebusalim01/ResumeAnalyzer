using System;
using System.Collections.Generic;
using System.Text;

namespace ResumeAnalyzer.Core.Models
{
    internal class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // One user can have many resumes
        public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    }
}
