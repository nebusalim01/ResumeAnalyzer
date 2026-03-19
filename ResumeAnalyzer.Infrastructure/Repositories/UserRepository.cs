using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Core.Interfaces;
using ResumeAnalyzer.Core.Models;
using ResumeAnalyzer.Infrastructure.Data;

namespace ResumeAnalyzer.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _db.Users.FindAsync(id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User> AddAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<bool> ExistsAsync(string email)
        {
            return await _db.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
    }
}