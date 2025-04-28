using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using UCMS.DTOs;
using UCMS.DTOs.AuthDto;
using UCMS.Models;
using UCMS.Repositories.InstructorRepository.Abstraction;
using UCMS.Repositories.StudentRepository.Abstraction;
using UCMS.Repositories.UserRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.AuthService;
using UCMS.Services.CookieService.Abstraction;
using UCMS.Services.EmailService.Abstraction;
using UCMS.Services.PasswordService.Abstraction;
using UCMS.Services.TokenService.Abstraction;

namespace UCMS_Test;

public class AuthServiceTest
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IStudentRepository> _mockStudentRepository;
    private readonly Mock<IInstructorRepository> _mockInstructorRepository;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<ICookieService> _mockCookieService;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IUrlHelperFactory> _mockUrlHelperFactory;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly AuthService _sut;


    public AuthServiceTest()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockStudentRepository = new Mock<IStudentRepository>();
        _mockInstructorRepository = new Mock<IInstructorRepository>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockCookieService = new Mock<ICookieService>();
        _mockTokenService = new Mock<ITokenService>();
        _mockMapper = new Mock<IMapper>();
        _mockEmailService = new Mock<IEmailService>();
        _mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _sut = new AuthService(_mockUserRepository.Object, _mockStudentRepository.Object, _mockInstructorRepository.Object, _mockPasswordService.Object, _mockMapper.Object, _mockEmailService.Object, _mockUrlHelperFactory.Object, _mockHttpContextAccessor.Object, _mockTokenService.Object, _mockCookieService.Object);
    }
    
    
    [Fact]
    public async Task Register_ReturnsFailure_WhenEmailAlreadyExists()
    {
        // Arrange
        var registerDto = new RegisterDto { Email = "test@example.com", Username = "testuser", Password = "StrongPass123!", ConfirmPassword = "StrongPass123!" };
        _mockUserRepository.Setup(repo => repo.GetUserByEmailAsync(registerDto.Email)).ReturnsAsync(new User());
        var expected = new ServiceResponse<int> { Success = false, Message = Messages.EmailAlreadyTaken };
        
        // Act
        var actual = await _sut.Register(registerDto);
        
        // Assert
        Assert.Equivalent(expected, actual);
    }
    
    
    [Fact]
    public async Task Register_ReturnsFailure_WhenUsernameAlreadyExists()
    {
        // Arrange
        var registerDto = new RegisterDto { Email = "test@example.com", Username = "testuser", Password = "StrongPass123!", ConfirmPassword = "StrongPass123!" };
        _mockUserRepository.Setup(repo => repo.GetUserByUsernameAsync(registerDto.Username)).ReturnsAsync(new User());
        var expected = new ServiceResponse<int> { Success = false, Message = Messages.UsernameAlreadyTaken };

        // Act
        var actual = await _sut.Register(registerDto);

        // Assert
        Assert.Equivalent(expected, actual);
    }
    
    [Fact]
    public async Task Register_ReturnsFailure_WhenPasswordsDoNotMatch()
    {
        // Arrange
        var registerDto = new RegisterDto { Email = "test@example.com", Username = "testuser", Password = "StrongPass123!", ConfirmPassword = "StrongPass124!" };
        var expected = new ServiceResponse<int> { Success = false, Message = Messages.PasswordNotMatch };

        // Act
        var actual = await _sut.Register(registerDto);

        // Assert
        Assert.Equivalent(expected, actual);
    }
    
    [Fact]
    public async Task Register_ReturnsFailure_WhenPasswordIsWeak()
    {
        // Arrange
        var registerDto = new RegisterDto { Email = "test@example.com", Username = "testuser", Password = "12345", ConfirmPassword = "12345" };
        _mockPasswordService.Setup(service => service.IsPasswordValid(registerDto.Password)).Returns(false);
        var expected = new ServiceResponse<int> { Success = false, Message = Messages.PasswordNotStrong };

        // Act
        var actual = await _sut.Register(registerDto);

        // Assert
        Assert.Equivalent(expected, actual);
    }
    
    
    // [Fact]
    // public async Task Register_ReturnsSuccess_WhenUserIsRegisteredSuccessfully()
    // {
    //     // Arrange
    //     var registerDto = new RegisterDto 
    //     { 
    //         Email = "test@example.com", 
    //         Username = "testuser", 
    //         Password = "StrongPass123!", 
    //         ConfirmPassword = "StrongPass123!" 
    //     };
    //
    //     var newUser = new User 
    //     { 
    //         Id = 1, 
    //         Email = registerDto.Email, 
    //         Username = registerDto.Username,
    //         VerificationToken = "generated-token" // مقداردهی توکن برای جلوگیری از null exception
    //     };
    //
    //     var expected = new ServiceResponse<int> 
    //     { 
    //         Data = newUser.Id, 
    //         Success = true, 
    //         Message = Messages.RegisteredSuccessfully 
    //         
    //     };
    //
    //     _mockUserRepository.Setup(repo => repo.GetUserByEmailAsync(registerDto.Email)).ReturnsAsync((User)null);
    //     _mockUserRepository.Setup(repo => repo.GetUserByUsernameAsync(registerDto.Username)).ReturnsAsync((User)null);
    //
    //     _mockPasswordService.Setup(service => service.IsPasswordValid(registerDto.Password)).Returns(true);
    //     _mockPasswordService.Setup(service => service.CreateSalt()).Returns(Encoding.UTF8.GetBytes("salt123"));
    //     _mockPasswordService.Setup(service => service.HashPasswordAsync(registerDto.Password, Encoding.UTF8.GetBytes("salt123"))).ReturnsAsync(Encoding.UTF8.GetBytes("hashedpassword"));
    //
    //     _mockMapper.Setup(mapper => mapper.Map<User>(registerDto)).Returns(newUser);
    //
    //     _mockUserRepository.Setup(repo => repo.AddUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
    //
    //     _mockUserRepository
    //         .Setup(repo => repo.AddUserAsync(It.Is<User>(u => !string.IsNullOrEmpty(u.VerificationToken))))
    //         .Returns(Task.CompletedTask);
    //
    //     _mockEmailService
    //         .Setup(service => service.SendVerificationEmail(
    //             It.Is<string>(email => email == registerDto.Email),
    //             It.Is<string>(link => link.Contains("token="))
    //         ))
    //         .Returns(Task.CompletedTask);
    //
    //     // Act
    //     var actual = await _sut.Register(registerDto);
    //
    //     // Assert
    //     Assert.Equivalent(expected, actual);
    //
    //     _mockUserRepository.Verify(repo => repo.AddUserAsync(It.Is<User>(u => !string.IsNullOrEmpty(u.VerificationToken))), Times.Once);
    //     
    //     _mockEmailService.Verify(service => service.SendVerificationEmail(
    //         It.Is<string>(email => email == registerDto.Email),
    //         It.Is<string>(link => 
    //                 link.StartsWith("http") &&
    //                 link.Contains("Auth/ConfirmEmail") &&
    //                 link.Contains($"token={newUser.VerificationToken}")
    //         )
    //     ), Times.Once);
    // }

}