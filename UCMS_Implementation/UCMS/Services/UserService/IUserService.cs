using UCMS.DTOs;
using UCMS.DTOs.AuthDto;
using UCMS.DTOs.User;
using UCMS.Models;

namespace UCMS.Services.UserService;

public interface IUserService
{
    Task<ServiceResponse<OutputUserDto>> GetUserByIdAsync(int userId);
    Task<ServiceResponse<List<OutputUserDto>>> GetAllUsersAsync();
    Task<ServiceResponse<OutputUserDto>> EditUser(User user, EditUserDto dto);
    ServiceResponse<OutputUserDto> GetCurrentUser(User user);
    Task<ServiceResponse<bool>> ChangePassword(User user, ChangePasswordDto dto);
    Task<ServiceResponse<bool>> DeleteUserAsync(int userId);
}