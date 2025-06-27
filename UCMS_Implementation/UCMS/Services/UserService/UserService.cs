using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using UCMS.DTOs;
using UCMS.DTOs.ClassDto;
using UCMS.DTOs.User;
using UCMS.Models;
using UCMS.Repositories.UserRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.ImageService;
using UCMS.Services.PasswordService.Abstraction;
using UCMS.Services.Utils;

namespace UCMS.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<UserService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageService _imageService;
        private readonly UrlBuilder _urlBuilder;

        public UserService(IUserRepository userRepository, IMapper mapper, IPasswordService passwordService,
            ILogger<UserService> logger, IHttpContextAccessor httpContextAccessor, IImageService imageService,
            UrlBuilder urlBuilder )
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _passwordService = passwordService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _imageService = imageService;
            _urlBuilder = urlBuilder;
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

        public async Task<ServiceResponse<bool>> UploadProfileImageAsync(UploadProfileImageDto uploadProfileImageDto)
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

            if (uploadProfileImageDto == null || uploadProfileImageDto.ProfileImage == null)
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = Messages.ImageNotFound
                };

            if (!_imageService.IsValidImageExtension(uploadProfileImageDto.ProfileImage))
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = Messages.InvalidFormat
                };
            }

            if (!_imageService.IsValidImageSize(uploadProfileImageDto.ProfileImage))
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = Messages.InvalidSize
                };
            }

            string imagePath = await _imageService.SaveImageAsync(uploadProfileImageDto.ProfileImage, "images/users");

            if (user.ProfileImagePath != null)
                await RemoveProfileImage();

            user.ProfileImagePath = imagePath;
            await _userRepository.UpdateUserAsync(user);
            _logger.LogInformation("User {userId} updated profile image successfully", user.Id);

            return new ServiceResponse<bool>
            {
                Data = true,
                Success = true,
                Message = Messages.UploadImage
            };
        }

        public async Task<ServiceResponse<bool>> RemoveProfileImage()
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

            if (user.ProfileImagePath == null)
                throw new ArgumentException("This user has no image profile");

            _imageService.DeleteImage(user.ProfileImagePath);

            user.ProfileImagePath = null;
            await _userRepository.UpdateUserAsync(user);
            _logger.LogInformation("User {userId} removed profile image successfully", user.Id);

            return new ServiceResponse<bool>
            {
                Data = true,
                Success = true,
                Message = Messages.RemoveImage
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

            responseUser.ProfileImagePath = _urlBuilder.BuildUrl(_httpContextAccessor, responseUser.ProfileImagePath);
            return new ServiceResponse<OutputUserDto>
            {
                Data = responseUser,
                Success = true,
                Message = message
            };
        }

    }
}
