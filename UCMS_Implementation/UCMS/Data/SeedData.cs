using UCMS.Models;
using UCMS.Services.RoleService.Abstraction;

namespace UCMS.Data;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, IRoleService roleService)
    {
        var roleNames = new[] { "Instructor", "Student" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleService.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleService.CreateRoleAsync(roleName);
            }
        }
    }
}