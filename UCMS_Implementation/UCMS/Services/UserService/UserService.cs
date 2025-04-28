using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using UCMS.DTOs;
using UCMS.DTOs.AuthDto;
using UCMS.DTOs.User;
using UCMS.Models;
using UCMS.Repositories.UserRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.AuthService;
using UCMS.Services.AuthService.Abstraction;
using UCMS.Services.PasswordService.Abstraction;

namespace UCMS.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<UserService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IUserRepository userRepository, IMapper mapper, IPasswordService passwordService, ILogger<UserService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _passwordService = passwordService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<List<OutputUserDto>>> GetAllUsersAsync()
        {
            List<User> users = await _userRepository.GetAllUsersAsync();
            List<OutputUserDto> responseUsers = _mapper.Map<List<OutputUserDto>>(users);

            return new ServiceResponse<List<OutputUserDto>>
            {
                Data = responseUsers,
                Success = true,
                Message = Messages.AllUsersFetchedSuccessfully
            };
        }

        public async Task<ServiceResponse<OutputUserDto>> GetUserByIdAsync(int userId)
        {
            User? user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return BuildNotFoundResponse(userId);
            }

            return BuildOutputUserDtoResponse(user, string.Format(Messages.UserFound, userId));
        }

        public ServiceResponse<OutputUserDto> GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
            Console.WriteLine(user.University.ToString());
            return BuildOutputUserDtoResponse(user, string.Format(Messages.UserFound, user.Id));
        }

        public async Task<ServiceResponse<bool>> DeleteUserAsync(int userId)
        {
            bool reuslt = await _userRepository.DeleteUserById(userId);

            ServiceResponse<bool> serviceResponse = new ServiceResponse<bool>
            {
                Data = reuslt,
                Success = reuslt,
            };

            if (reuslt)
            {
                _logger.LogInformation("User {userId} deleted successfully", userId);
                serviceResponse.Message = string.Format(Messages.DeleteUser, userId);
            }
            else serviceResponse.Message = string.Format(Messages.UserNotFound, userId);

            return serviceResponse;
        }

        public async Task<ServiceResponse<OutputUserDto>> EditUser(EditUserDto dto)
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
            User updatedUser = _mapper.Map(dto, user);

            await _userRepository.UpdateUserAsync(updatedUser);
            _logger.LogInformation("User {userId} updated successfully", user.Id);

            return BuildOutputUserDtoResponse(updatedUser, string.Format(Messages.UpdateUser, user.Id));
        }

        public async Task<ServiceResponse<bool>> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

            if (!await _passwordService.VerifyPasswordAsync(changePasswordDto.OldPassword, user.PasswordSalt, user.PasswordHash))
            {
                return new ServiceResponse<bool> { Success = false, Message = Messages.WrongPasswordMessage };
            }

            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
            {
                return new ServiceResponse<bool> { Success = false, Message = Messages.PasswordNotMatch };
            }

            if (!_passwordService.IsPasswordValid(changePasswordDto.NewPassword))
            {
                return new ServiceResponse<bool> { Success = false, Message = Messages.PasswordNotStrong };
            }

            user.PasswordSalt = _passwordService.CreateSalt();
            user.PasswordHash = await _passwordService.HashPasswordAsync(changePasswordDto.NewPassword, user.PasswordSalt);

            await _userRepository.UpdateUserAsync(user);
            return new ServiceResponse<bool>
            {
                Data = true,
                Success = true,
                Message = Messages.PasswordChange
            };
        }

        private static ServiceResponse<OutputUserDto> BuildNotFoundResponse(int userId)
        {
            return new ServiceResponse<OutputUserDto>
            {
                Success = false,
                Message = String.Format(Messages.UserNotFound, userId)
            };
        }

        private ServiceResponse<OutputUserDto> BuildOutputUserDtoResponse(User user, string message)
        {
            OutputUserDto responseUser = _mapper.Map<OutputUserDto>(user);
            return new ServiceResponse<OutputUserDto>
            {
                Data = responseUser,
                Success = true,
                Message = message
            };
        }

    }
}
