using UCMS.DTOs;
using UCMS.DTOs.RoleDto;
using UCMS.Models;

namespace UCMS.Services.RoleService.Abstraction;

public interface IRoleService
{
    Task<ServiceResponse<IEnumerable<GetRoleDto>>> GetAllRolesAsync();
    Task<Role?> GetRoleByIdAsync(int id);
    Task CreateRoleAsync(string roleName);
    Task<bool> RoleExistsAsync(string roleName);
}