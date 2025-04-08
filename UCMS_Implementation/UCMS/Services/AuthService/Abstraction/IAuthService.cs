using UCMS.DTOs.AuthDto;

namespace UCMS.Services.AuthService.Abstraction;

public interface IAuthService
{
    Task<ServiceResponse<int>> Register(RegisterDto registerDto);
    Task<ServiceResponse<bool>> ConfirmEmail(string token);
    Task<ServiceResponse<string?>> Login(LoginDto request);
}