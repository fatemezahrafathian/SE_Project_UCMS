using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using UCMS.DTOs;
using UCMS.DTOs.AuthDto;
using UCMS.DTOs.User;
using UCMS.Models;
using UCMS.Repositories.UserRepository.Abstraction;
using UCMS.Resources;

namespace UCMS.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IMapper mapper, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
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

        public async Task<ServiceResponse<OutputUserDto>> EditUser(int userId, EditUserDto dto)
        {
            User? user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return BuildNotFoundResponse(userId);
            }

            User updatedUser = _mapper.Map(dto, user);

            await _userRepository.UpdateUserAsync(updatedUser);
            _logger.LogInformation("User {userId} updated successfully", userId);

            return BuildOutputUserDtoResponse(updatedUser, string.Format(Messages.UpdateUser, userId));
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
