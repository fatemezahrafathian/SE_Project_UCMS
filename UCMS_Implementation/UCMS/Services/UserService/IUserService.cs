using UCMS.DTOs;
using UCMS.DTOs.AuthDto;
using UCMS.DTOs.User;
using UCMS.Models;

namespace UCMS.Services.UserService;

public interface IUserService
{
    Task<ServiceResponse<OutputUserDto>> GetUserByIdAsync(int userId);
    Task<ServiceResponse<List<OutputUserDto>>> GetAllUsersAsync();
    Task<ServiceResponse<OutputUserDto>> EditUser(EditUserDto dto);
    ServiceResponse<OutputUserDto> GetCurrentUser();
    Task<ServiceResponse<bool>> ChangePassword(ChangePasswordDto dto);
    Task<ServiceResponse<bool>> DeleteUserAsync(int userId);
    Task<ServiceResponse<bool>> UploadProfileImageAsync(UploadProfileImageDto uploadProfileImageDto);
    Task<ServiceResponse<bool>> RemoveProfileImage();
}