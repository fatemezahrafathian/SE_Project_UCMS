using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Konscious.Security.Cryptography;
using UCMS.DTOs.AuthDto;
using UCMS.Repositories.UserRepository.Abstraction;
using UCMS.Services.AuthService.Abstraction;
using UCMS.Services.CookieService.Abstraction;
using UCMS.Services.EmailService.Abstraction;
using UCMS.Services.TokenService.Abstraction;

namespace UCMS.Services.AuthService;

public class PasswordService: IPasswordService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 4;
    private const int MemorySize = 65536;
    private const int Parallelism = 2;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IOneTimeCodeService _oneTimeCodeService; 
    private readonly ICookieService _cookieService;
    private readonly ITokenService _tokenService;
    public PasswordService(IUserRepository userRepository, IEmailService emailService,IOneTimeCodeService oneTimeCodeService,
      ITokenService tokenService,ICookieService cookieService)
    {
        _userRepository = userRepository;
        _cookieService = cookieService;
        _tokenService = tokenService;
        _emailService = emailService;
        _oneTimeCodeService = oneTimeCodeService;
    }

    public bool IsPasswordValid(string password)
    {
        // at least 8 characters length, containing at least a capital letter, an small letter, a number and a special characters
        string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
        Regex regex = new Regex(pattern);
        return regex.IsMatch(password);
    }

    public byte[] CreateSalt()
    {
        byte[] saltBytes = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        return saltBytes;
    }

    public async Task<byte[]> HashPasswordAsync(string password, byte[] salt)
    {
        return await Task.Run(() =>
        {
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = Parallelism;
                argon2.MemorySize = MemorySize;
                argon2.Iterations = Iterations;
                return argon2.GetBytes(HashSize);
            }
        });
    }

    public async Task<bool> VerifyPasswordAsync(string password, byte[] salt, byte[] hashedPassword)
    {
        byte[] newHash = await HashPasswordAsync(password, salt);
        return newHash.SequenceEqual(hashedPassword);
    }
    public async Task<ServiceResponse<string>> RequestPasswordResetAsync(ForgetPasswordDto forgetPasswordDto)
    {
        var user = await _userRepository.GetUserByEmailAsync(forgetPasswordDto.Email);
        if (user == null)
            return new ServiceResponse<string> {Success = false, Message = Resources.Messages.UserNotFoundMessage };

        user.OneTimeCode = _oneTimeCodeService.Generate(2);
        await _userRepository.UpdateUserAsync(user);

        await _emailService.SendVerificationEmail(user.Email, $"کد شما: {user.OneTimeCode.Code}");

        return new ServiceResponse<string> {Success = true , Message = Resources.Messages.OneTimeCodeSent};

    }
    public async Task<ServiceResponse<string>> TempPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userRepository.GetUserByEmailAsync(dto.Email);
        if (user == null)
            return new ServiceResponse<string> {Success = false, Message = Resources.Messages.UserNotFoundMessage };

        if (user.OneTimeCode == null || !_oneTimeCodeService.IsValid(dto.OneTimeCode,user.OneTimeCode))
            return new ServiceResponse<string> {Success = false, Message = Resources.Messages.ExpiredCode };
        
        user.OneTimeCode = null;

        await _userRepository.UpdateUserAsync(user);
        
        var claims = new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        _cookieService.CreateCookie(_tokenService.GenerateToken(claims));

        return new ServiceResponse<string> { Success = true, Message = Resources.Messages.LoginSuccessfulMessage };
    }
}