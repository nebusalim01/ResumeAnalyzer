using ResumeAnalyzer.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResumeAnalyzer.Core.Interfaces
{
    public interface IAnalysisRepository
    {
        Task<Analysis?> GetByIdAsync(int id);
        Task<Analysis?> GetByResumeIdAsync(int resumeId);
        Task<IEnumerable<Analysis>> GetHistoryByUserIdAsync(int userId);
        Task<Analysis> AddAsync(Analysis analysis);
    }
}
