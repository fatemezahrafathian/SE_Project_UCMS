using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
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

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<List<OutputUserDto>>> GetAllUsersAsync()
        {
            List<User> users = await _userRepository.GetAllUsersAsync();
            List<OutputUserDto> responseUsers = _mapper.Map<List<OutputUserDto>>(users);

            return new ServiceResponse<List<OutputUserDto>>
            {
                Data = responseUsers,
                Success = true,
                Message = Messages.AllUsers
            };
        }

        public async Task<ServiceResponse<OutputUserDto>> GetUserByIdAsync(int userId)
        {
            User? user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return new ServiceResponse<OutputUserDto>
                {
                    Success = false,
                    Message = String.Format(Messages.UserNotFound, userId)
                };
            }

            OutputUserDto responseUser = _mapper.Map<OutputUserDto>(user);
            return new ServiceResponse<OutputUserDto>
            {
                Data = responseUser,
                Success = true,
                Message = Messages.UserFound
            };
        }
    }
}
