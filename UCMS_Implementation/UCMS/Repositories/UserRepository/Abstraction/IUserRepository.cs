using UCMS.Models;

namespace UCMS.Repositories.UserRepository.Abstraction;

public interface IUserRepository
{
    Task AddUserAsync(User user);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByVerificationTokenAsync(string verificationToken);
    Task UpdateUserAsync(User user);
    Task<User?> GetUserByIdAsync(int id);
}