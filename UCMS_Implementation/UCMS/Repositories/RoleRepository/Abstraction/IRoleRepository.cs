using UCMS.Models;

namespace UCMS.Repositories.RoleRepository.Abstraction;

public interface IRoleRepository
{
    Task<Role?> GetRoleByIdAsync(int id);
    Task<IEnumerable<Role>> GetAllRolesAsync();
    Task CreateRoleAsync(Role role);
    Task<bool> RoleExistsAsync(string roleName);
}