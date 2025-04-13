using Microsoft.AspNetCore.Mvc;
using UCMS.Services.RoleService.Abstraction;

namespace UCMS.Controllers;

[Route("api/role")]
[ApiController]
public class RoleController: ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoles()
    {
        var response = await _roleService.GetAllRolesAsync();
        return Ok(response);
    }
}