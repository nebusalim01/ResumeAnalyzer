using ResumeAnalyzer.Core.Models;

namespace ResumeAnalyzer.Core.Interfaces
{
    public interface IResumeRepository
    {
        Task<Resume?> GetByIdAsync(int id);
        Task<IEnumerable<Resume>> GetByUserIdAsync(int userId);
        Task<Resume> AddAsync(Resume resume);
        Task DeleteAsync(int id);
    }
}