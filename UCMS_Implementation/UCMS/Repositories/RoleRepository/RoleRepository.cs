using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.RoleRepository.Abstraction;

namespace UCMS.Repositories.RoleRepository;

public class RoleRepository : IRoleRepository
{
    private readonly DataContext _context;

    public RoleRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetRoleByIdAsync(int id)
    {
        return await _context.Roles.FindAsync(id);
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task CreateRoleAsync(Role role)
    {
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();
    }
    
    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await _context.Roles.AnyAsync(r => r.Name == roleName);
    }
}