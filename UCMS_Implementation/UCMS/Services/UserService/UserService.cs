using AutoMapper;
using UCMS.DTOs.User;
using UCMS.Models;
using UCMS.Repositories.UserRepository.Abstraction;

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
                Message = "All Users"
            };
          
        }
    }
}
