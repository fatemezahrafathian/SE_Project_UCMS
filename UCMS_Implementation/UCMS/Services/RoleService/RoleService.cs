using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.RoleDto;
using UCMS.Models;
using UCMS.Repositories.RoleRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.RoleService.Abstraction;

namespace UCMS.Services.RoleService;

public class RoleService: IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IMapper _mapper;

    public RoleService(IRoleRepository roleRepository, IMapper mapper)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResponse<IEnumerable<GetRoleDto>>> GetAllRolesAsync()
    {
        var roles = await _roleRepository.GetAllRolesAsync();
        var roleDtos = _mapper.Map<IEnumerable<GetRoleDto>>(roles);
        return new ServiceResponse<IEnumerable<GetRoleDto>>
        {
            Data = roleDtos,
            Success = true,
            Message = Messages.AllRolesFetchedSuccessfully
        };
    }

    public async Task<Role?> GetRoleByIdAsync(int id)
    {
        return await _roleRepository.GetRoleByIdAsync(id);
    }

    public async Task CreateRoleAsync(string roleName)
    {
        var role = new Role
        {
            Name = roleName
        };
        await _roleRepository.CreateRoleAsync(role);
    }

    public Task<bool> RoleExistsAsync(string roleName)
    {
        return _roleRepository.RoleExistsAsync(roleName);
    }
}