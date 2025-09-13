using Microsoft.EntityFrameworkCore;
using PRM_BE.Model;

namespace PRM_BE.Data.Repository
{
    public class UserRepository
    {
       private readonly Model.AppDbContext _context;
            public UserRepository(AppDbContext context)
            {
                _context = context;
            }

            public async Task<User?> GetByIdAsync(int id)
            {
                return await _context.Users.FindAsync(id);
            }

            public async Task<User?> GetByEmailAsync(string email)
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }

            public async Task<User?> GetByUsernameAsync(string username)
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            }

            public async Task<User> CreateAsync(User user)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }

            public async Task<bool> EmailExistsAsync(string email)
            {
                return await _context.Users.AnyAsync(u => u.Email == email);
            }

            public async Task<bool> UsernameExistsAsync(string username)
            {
                return await _context.Users.AnyAsync(u => u.UserName == username);
            } 
}
}
