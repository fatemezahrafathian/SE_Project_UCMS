using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using UCMS.DTOs;
using UCMS.DTOs.AuthDto;
using UCMS.Models;
using UCMS.Repositories.InstructorRepository.Abstraction;
using UCMS.Repositories.StudentRepository.Abstraction;
using UCMS.Repositories.UserRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.AuthService.Abstraction;
using UCMS.Services.CookieService.Abstraction;
using UCMS.Services.EmailService.Abstraction;
using UCMS.Services.PasswordService.Abstraction;
using UCMS.Services.TokenService.Abstraction;

namespace UCMS.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IInstructorRepository _instructorRepository;
    private readonly IPasswordService _passwordService;
    private readonly ICookieService _cookieService;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IUserRepository userRepository, IStudentRepository studentRepository, IInstructorRepository instructorRepository, IPasswordService passwordService, IMapper mapper,
        IEmailService emailService, IUrlHelperFactory urlHelperFactory, IHttpContextAccessor httpContextAccessor,
        ITokenService tokenService, ICookieService cookieService)
    {
        _userRepository = userRepository;
        _studentRepository = studentRepository;
        _instructorRepository = instructorRepository;
        _passwordService = passwordService;
        _cookieService = cookieService;
        _tokenService = tokenService;
        _mapper = mapper;
        _emailService = emailService;
        _urlHelperFactory = urlHelperFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ServiceResponse<int>> Register(RegisterDto registerDto)
    {
        if (await _userRepository.GetUserByEmailAsync(registerDto.Email) != null)
        {
            return new ServiceResponse<int> { Success = false, Message = Messages.EmailAlreadyTaken };
        }

        if (await _userRepository.GetUserByUsernameAsync(registerDto.Username) != null)
        {
            return new ServiceResponse<int> { Success = false, Message = Messages.UsernameAlreadyTaken };
        }

        // first check at front side, double due to security
        if (registerDto.Password != registerDto.ConfirmPassword)
        {
            return new ServiceResponse<int> { Success = false, Message = Messages.PasswordNotMatch };
        }

        // first check at front side, double due to security
        if (!_passwordService.IsPasswordValid(registerDto.Password))
        {
            return new ServiceResponse<int> { Success = false, Message = Messages.PasswordNotStrong };
        }

        var newUser = _mapper.Map<User>(registerDto);

        newUser.PasswordSalt = _passwordService.CreateSalt();
        newUser.PasswordHash = await _passwordService.HashPasswordAsync(registerDto.Password, newUser.PasswordSalt);

        newUser.VerificationToken = GenerateVerificationToken();
        newUser.IsConfirmed = false;

        await _userRepository.AddUserAsync(newUser);

        if (newUser.RoleId == 2)
            await CreateStudentAsync(newUser.Id);
        else if (newUser.RoleId == 1)
            await CreateInstructor(newUser.Id);

        var confirmationLink = GenerateConfirmationLink(newUser.VerificationToken);
        await _emailService.SendVerificationEmail(newUser.Email, confirmationLink);

        return new ServiceResponse<int> // add constructor
        {
            Data = newUser.Id,
            Success = true,
            Message = Messages.RegisteredSuccessfully
        };
    }

    private async Task CreateStudentAsync(int userId)
    {
        var newStudent = new Student
        {
            UserId = userId
        };

        await _studentRepository.AddStudentAsync(newStudent);
    }

    private async Task CreateInstructor(int userId)
    {
        var newInstructor = new Instructor
        {
            UserId = userId
        };

        await _instructorRepository.AddInstructorAsync(newInstructor);
    }

    private string GenerateVerificationToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    public async Task<ServiceResponse<bool>> ConfirmEmail(string token)
    {
        var user = await _userRepository.GetUserByVerificationTokenAsync(token);
        if (user == null)
            return new ServiceResponse<bool> { Success = false, Message = Messages.InvalidToken };

        user.IsConfirmed = true;
        user.VerificationToken = null;

        await _userRepository.UpdateUserAsync(user);

        return new ServiceResponse<bool> { Success = true, Message = Messages.AccountConfirmedSuccessfully };
    }

    private string GenerateConfirmationLink(string token)
    {
        var actionContext = new ActionContext(
            _httpContextAccessor.HttpContext,
            _httpContextAccessor.HttpContext.GetRouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
        );

        var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);

        string confirmationUrl = urlHelper.Action(
            "ConfirmEmail",
            "Auth",
            new { token },
            _httpContextAccessor.HttpContext.Request.Scheme
        );

        return confirmationUrl;
    }
    public async Task<ServiceResponse<string?>> Login(LoginDto loginDto)
    {
        if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
            return new ServiceResponse<string?> { Success = false, Message = Messages.InvalidInputMessage };

        var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);

        if (user is null)
            return new ServiceResponse<string?> { Success = false, Message = Messages.UserNotFoundMessage };
        if (!await _passwordService.VerifyPasswordAsync(loginDto.Password, user.PasswordSalt, user.PasswordHash))
            return new ServiceResponse<string?> { Success = false, Message = Messages.WrongPasswordMessage };
        // if (!user.IsConfirmed)
        //     return new ServiceResponse<string?> { Success = false, Message = Messages.AccountNotConfirmed};
        var claims = new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            // new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        _cookieService.CreateCookie(_tokenService.GenerateToken(claims));

        return new ServiceResponse<string?> { Success = true, Message = Messages.LoginSuccessfulMessage };
    }
    public async Task<ServiceResponse<string?>> Logout()
    {
        if (_cookieService.GetCookieValue() != null) _cookieService.DeleteTokenCookie();

        return new ServiceResponse<string?> { Success = true, Message = Messages.LogoutSuccessfulyMessage };
    }
    public async Task<ServiceResponse<string>> GetAuthorized()
    {
        var token = _cookieService.GetCookieValue();
        if (string.IsNullOrEmpty(token))
            return new ServiceResponse<string> { Success = false, Message = Messages.UnauthorizedMessage };

        int userId = _tokenService.GetUserId(_httpContextAccessor.HttpContext?.User);
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user is null)
            return new ServiceResponse<string>
            { Success = false, Message = Messages.UserNotFoundMessage };

        return new ServiceResponse<string>
        { Success = true, Message = Messages.AuthorizedMessage };
    }

}