using UCMS.DTOs;
using UCMS.DTOs.AuthDto;

namespace UCMS.Services.AuthService.Abstraction;

public interface IPasswordService
{
    public bool IsPasswordValid(string password);
    byte[] CreateSalt(); // just process, no I/O ...
    Task<byte[]> HashPasswordAsync(string password, byte[] salt); // argone2i may be loadable so async
    Task<bool> VerifyPasswordAsync(string password, byte[] salt, byte[] hashedPassword); // using HashPasswordAsync so async
    Task<ServiceResponse<string>> RequestPasswordResetAsync(ForgetPasswordDto forgetPasswordDto);
    Task<ServiceResponse<string>> TempPasswordAsync(ResetPasswordDto dto);
}