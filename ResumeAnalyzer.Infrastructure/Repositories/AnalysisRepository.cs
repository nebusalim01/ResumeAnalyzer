using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Core.Interfaces;
using ResumeAnalyzer.Core.Models;
using ResumeAnalyzer.Infrastructure.Data;

namespace ResumeAnalyzer.Infrastructure.Repositories
{
    public class AnalysisRepository : IAnalysisRepository
    {
        private readonly AppDbContext _db;

        public AnalysisRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Analysis?> GetByIdAsync(int id)
        {
            return await _db.Analyses
                .Include(a => a.JobMatch)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Analysis?> GetByResumeIdAsync(int resumeId)
        {
            return await _db.Analyses
                .Include(a => a.JobMatch)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(a => a.ResumeId == resumeId);
        }

        public async Task<IEnumerable<Analysis>> GetHistoryByUserIdAsync(int userId)
        {
            return await _db.Analyses
                .Include(a => a.Resume)
                .Include(a => a.JobMatch)
                .Where(a => a.Resume.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Analysis> AddAsync(Analysis analysis)
        {
            _db.Analyses.Add(analysis);
            await _db.SaveChangesAsync();
            return analysis;
        }
    }
}