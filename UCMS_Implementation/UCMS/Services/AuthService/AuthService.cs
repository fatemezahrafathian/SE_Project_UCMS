using System.Security.Cryptography;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using UCMS.DTOs.AuthDto;
using UCMS.Models;
using UCMS.Repositories.UserRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.AuthService.Abstraction;
using UCMS.Services.EmailService.Abstraction;

namespace UCMS.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IUserRepository userRepository, IPasswordService passwordService, IMapper mapper,
        IEmailService emailService, IUrlHelperFactory urlHelperFactory, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _mapper = mapper;
        _emailService = emailService;
        _urlHelperFactory = urlHelperFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ServiceResponse<int>> Register(RegisterDto registerDto)
    {
        if (await _userRepository.GetUserByEmailAsync(registerDto.Email) != null)
        {
            return new ServiceResponse<int> {Success = false, Message = Messages.EmailAlreadyTaken};
        }

        if (await _userRepository.GetUserByUsernameAsync(registerDto.Username) != null)
        {
            return new ServiceResponse<int> {Success = false, Message = Messages.UsernameAlreadyTaken};
        }

        // first check at front side, double due to security
        if (registerDto.Password != registerDto.ConfirmPassword)
        {
            return new ServiceResponse<int> {Success = false, Message = Messages.PasswordNotMatch};
        }

        // first check at front side, double due to security
        if (!_passwordService.IsPasswordValid(registerDto.Password))
        {
            return new ServiceResponse<int> {Success = false, Message = Messages.PasswordNotStrong};
        }

        var newUser = _mapper.Map<User>(registerDto);

        newUser.PasswordSalt = _passwordService.CreateSalt();
        newUser.PasswordHash = await _passwordService.HashPasswordAsync(registerDto.Password, newUser.PasswordSalt);

        newUser.VerificationToken = GenerateVerificationToken();
        newUser.IsConfirmed = false;

        await _userRepository.AddUserAsync(newUser);

        var confirmationLink = GenerateConfirmationLink(newUser.VerificationToken);
        await _emailService.SendVerificationEmail(newUser.Email, confirmationLink);

        return new ServiceResponse<int> // add constructor
        {
            Data = newUser.Id,
            Success = true,
            Message = Messages.RegisteredSuccessfully
        };
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
}