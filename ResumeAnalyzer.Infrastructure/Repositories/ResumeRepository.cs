using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Core.Interfaces;
using ResumeAnalyzer.Core.Models;
using ResumeAnalyzer.Infrastructure.Data;

namespace ResumeAnalyzer.Infrastructure.Repositories
{
    public class ResumeRepository : IResumeRepository
    {
        private readonly AppDbContext _db;

        public ResumeRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Resume?> GetByIdAsync(int id)
        {
            return await _db.Resumes
                .Include(r => r.User)
                .Include(r => r.Analyses)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Resume>> GetByUserIdAsync(int userId)
        {
            return await _db.Resumes
                .Where(r => r.UserId == userId)
                .Include(r => r.Analyses)
                .OrderByDescending(r => r.UploadedAt)
                .ToListAsync();
        }

        public async Task<Resume> AddAsync(Resume resume)
        {
            _db.Resumes.Add(resume);
            await _db.SaveChangesAsync();
            return resume;
        }

        public async Task DeleteAsync(int id)
        {
            var resume = await _db.Resumes.FindAsync(id);
            if (resume != null)
            {
                _db.Resumes.Remove(resume);
                await _db.SaveChangesAsync();
            }
        }
    }
}