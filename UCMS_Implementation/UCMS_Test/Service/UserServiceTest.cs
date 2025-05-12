using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UCMS.DTOs.User;
using UCMS.Models;
using UCMS.Repositories.UserRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.PasswordService.Abstraction;
using UCMS.Services.UserService;

namespace UCMS_Test.Service;

public class UserServiceTest
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly IMapper _mapper;
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<ILogger<UserService>> _logger = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor = new();
    private readonly UserService _sut;

    public UserServiceTest()
    {
        var services = new ServiceCollection();
        services.AddAutoMapper(typeof(UCMS.Profile.AutoMapperProfile));
        var provider = services.BuildServiceProvider();
        _mapper = provider.GetRequiredService<IMapper>();
        _sut = new UserService(_userRepo.Object, _mapper, _passwordService.Object, _logger.Object, _httpContextAccessor.Object);
    }

    private void SetUserInHttpContext(User user)
    {
        var context = new DefaultHttpContext();
        context.Items["User"] = user;
        _httpContextAccessor.Setup(h => h.HttpContext).Returns(context);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsMappedUsers()
    {
        // Arrange
        var users = new List<User> { new User(), new User() };
        _userRepo.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(users);

        // Act
        var result = await _sut.GetAllUsersAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.AllUsersFetchedSuccessfully, result.Message);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsUser_WhenFound()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "TestUser",
        };
        _userRepo.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _sut.GetUserByIdAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("TestUser", result.Data.Username);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        _userRepo.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync((User)null);

        // Act
        var result = await _sut.GetUserByIdAsync(1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(string.Format(Messages.UserNotFound, 1), result.Message);
    }

    [Fact]
    public void GetCurrentUser_ReturnsUserFromHttpContext()
    {
        // Arrange
        var user = new User 
        { 
            Id = 5,
            Email = "current@user.com"
        };
        SetUserInHttpContext(user);

        // Act
        var result = _sut.GetCurrentUser();

        // Assert
        Assert.True(result.Success);
        Assert.Equal("current@user.com", result.Data.Email);
    }

    [Fact]
    public async Task EditUser_UpdatesUserSuccessfully()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Email = "old@mail.com",
            Username = "olduser",
            FirstName = "OldFirst",
            LastName = "OldLast",
            Gender = Gender.Male,
            Address = "Old Address",
            Bio = "Old bio",
            DateOfBirth = new DateTime(1990, 1, 1),
        };

        var editDto = new EditUserDto
        {
            FirstName = "NewFirst",
            LastName = "NewLast",
            Gender = Gender.Female,
            Address = "New Address",
            Bio = "Updated bio",
            DateOfBirth = new DateTime(1995, 5, 5)
        };

        SetUserInHttpContext(existingUser);

        _userRepo.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>()))
                   .Returns(Task.CompletedTask)
                   .Verifiable();

        // Act
        var result = await _sut.EditUser(editDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(editDto.FirstName, result.Data.FirstName);
        Assert.Equal(editDto.LastName, result.Data.LastName);
        Assert.Equal(editDto.Gender, result.Data.Gender);
        Assert.Equal(editDto.Address, result.Data.Address);
        Assert.Equal(editDto.Bio, result.Data.Bio);
        Assert.Equal(editDto.DateOfBirth, result.Data.DateOfBirth);

        _userRepo.Verify(repo => repo.UpdateUserAsync(It.Is<User>(u =>
            u.FirstName == editDto.FirstName &&
            u.LastName == editDto.LastName &&
            u.Gender == editDto.Gender &&
            u.Address == editDto.Address &&
            u.Bio == editDto.Bio &&
            u.DateOfBirth == editDto.DateOfBirth
        )), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ReturnsSuccess_WhenDeleted()
    {
        // Arrange
        _userRepo.Setup(r => r.DeleteUserById(1)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteUserAsync(1);


        // Assert
        Assert.True(result.Success);
        Assert.Equal(string.Format(Messages.DeleteUser, 1), result.Message);
    }

    [Fact]
    public async Task DeleteUserAsync_ReturnsFailure_WhenNotFound()
    {
        // Arrange
        _userRepo.Setup(r => r.DeleteUserById(2)).ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteUserAsync(2);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(string.Format(Messages.UserNotFound, 2), result.Message);
    }

    [Fact]
    public async Task ChangePassword_ReturnsSuccess_WhenAllChecksPass()
    {
        // Arrange
        var user = new User { Id = 1, PasswordSalt = new byte[1], PasswordHash = new byte[1] };
        SetUserInHttpContext(user);

        var dto = new ChangePasswordDto
        {
            OldPassword = "oldpass",
            NewPassword = "newStrongPass123!",
            ConfirmNewPassword = "newStrongPass123!"
        };

        _passwordService.Setup(p => p.VerifyPasswordAsync(dto.OldPassword, user.PasswordSalt, user.PasswordHash)).ReturnsAsync(true);
        _passwordService.Setup(p => p.IsPasswordValid(dto.NewPassword)).Returns(true);
        _passwordService.Setup(p => p.CreateSalt()).Returns(new byte[256]);
        _passwordService.Setup(p => p.HashPasswordAsync(dto.NewPassword, It.IsAny<byte[]>())).ReturnsAsync(new byte[256]);

        // Act
        var result = await _sut.ChangePassword(dto);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Equal(Messages.PasswordChange, result.Message);
    }

    [Fact]
    public async Task ChangePassword_ReturnsFailure_WhenOldPasswordInvalid()
    {
        // Arrange
        var user = new User { PasswordSalt = new byte[1], PasswordHash = new byte[1] };
        SetUserInHttpContext(user);

        var dto = new ChangePasswordDto { OldPassword = "wrong", NewPassword = "new", ConfirmNewPassword = "new" };
        _passwordService.Setup(p => p.VerifyPasswordAsync(dto.OldPassword, user.PasswordSalt, user.PasswordHash)).ReturnsAsync(false);

        // Act
        var result = await _sut.ChangePassword(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.WrongPasswordMessage, result.Message);
    }

    [Fact]
    public async Task ChangePassword_ReturnsFailure_WhenPasswordMismatch()
    {
        // Arrange
        var user = new User();
        SetUserInHttpContext(user);

        var dto = new ChangePasswordDto { OldPassword = "pass", NewPassword = "123", ConfirmNewPassword = "321" };
        _passwordService.Setup(p => p.VerifyPasswordAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>())).ReturnsAsync(true);

        // Act
        var result = await _sut.ChangePassword(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PasswordNotMatch, result.Message);
    }

    [Fact]
    public async Task ChangePassword_ReturnsFailure_WhenPasswordNotStrong()
    {
        // Arrange
        var user = new User();
        SetUserInHttpContext(user);

        var dto = new ChangePasswordDto { OldPassword = "pass", NewPassword = "weak", ConfirmNewPassword = "weak" };
        _passwordService.Setup(p => p.VerifyPasswordAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>())).ReturnsAsync(true);
        _passwordService.Setup(p => p.IsPasswordValid(dto.NewPassword)).Returns(false);

        // Act
        var result = await _sut.ChangePassword(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.PasswordNotStrong, result.Message);
    }
}
