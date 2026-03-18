using ResumeAnalyzer.Core.Models;

namespace ResumeAnalyzer.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<User> AddAsync(User user);
        Task<bool> ExistsAsync(string email);
    }
}