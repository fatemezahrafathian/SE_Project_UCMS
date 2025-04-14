using UCMS.DTOs;
using UCMS.DTOs.AuthDto;
using UCMS.DTOs.User;

namespace UCMS.Services.UserService;

public interface IUserService
{
     Task<ServiceResponse<OutputUserDto>> GetUserByIdAsync(int userId);
    Task<ServiceResponse<List<OutputUserDto>>> GetAllUsersAsync();
    Task<ServiceResponse<OutputUserDto>> EditUser(int id, EditUserDto dto);
    //Task<ServiceResponse<bool>> ChangePassword(ChangePasswordDto dto);
    Task<ServiceResponse<bool>> DeleteUserAsync(int userId);
}