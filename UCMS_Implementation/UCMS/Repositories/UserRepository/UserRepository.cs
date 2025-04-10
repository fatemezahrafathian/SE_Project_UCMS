using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.UserRepository.Abstraction;

namespace UCMS.Repositories.UserRepository;

public class UserRepository: IUserRepository
{
    private readonly DataContext _context;
    
    public UserRepository(DataContext context)
    {
        _context = context;
    }
    
    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .Where(u => u.Id == id)
            .Include(u => u.Role)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetUserByVerificationTokenAsync(string verificationToken)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == verificationToken);
    }

    public async Task UpdateUserAsync(User user) // to be implemented
    {
        // if (user == null)
        //     throw new ArgumentNullException(nameof(user));
        //
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

}