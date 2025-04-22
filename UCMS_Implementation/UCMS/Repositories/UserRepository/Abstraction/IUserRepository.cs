using UCMS.Models;

namespace UCMS.Repositories.UserRepository.Abstraction;

public interface IUserRepository
{
    Task AddUserAsync(User user);
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByVerificationTokenAsync(string verificationToken);

    Task<bool> DeleteUserById(int id);
    Task UpdateUserAsync(User user);
}