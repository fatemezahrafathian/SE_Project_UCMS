using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UCMS.DTOs.Instructor;
using UCMS.Models;
using UCMS.Repositories.InstructorRepository.Abstraction;
using UCMS.Repositories.UserRepository.Abstraction;
using UCMS.Services.InstructorService;
using UCMS.Services.UserService;
using UCMS.Services.Utils;
using Xunit;

namespace UCMS_Test.Service;

public class InstructorServiceTest
{
    private readonly Mock<IInstructorRepository> _mockInstructorRepo = new();
    private readonly Mock<IUserRepository> _mockUserRepo = new();
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<InstructorService>> _logger = new();
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
    private readonly Mock<UrlBuilder> _urlBuilderMock = new();
    private readonly InstructorService _sut;

    public InstructorServiceTest()
    {
        var services = new ServiceCollection();
        services.AddAutoMapper(typeof(UCMS.Profile.AutoMapperProfile));
        var provider = services.BuildServiceProvider();
        _mapper = provider.GetRequiredService<IMapper>();

        _sut = new InstructorService(
            _mockInstructorRepo.Object,
            _mapper,
            _mockHttpContextAccessor.Object,
            _logger.Object,
            _urlBuilderMock.Object,
            _mockUserRepo.Object
            );
    }

    private void SetHttpContextWithUser(int userId)
    {
        var context = new DefaultHttpContext();
        context.Items["User"] = new User { Id = userId };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(context);
    }

    [Fact]
    public async Task EditInstructor_ReturnsUpdatedInstructor_WhenValidInput()
    {
        // Arrange
        int userId = 1;
        SetHttpContextWithUser(userId);

        var instructor = new Instructor
        {
            Id = 1,
            UserId = userId,
            EmployeeCode = "E001",
            Department = "Math",
            Rank = InstructorRank.Associate,
        };

        var editDto = new EditInstructorDto
        {
            EmployeeCode = "E002",
            Department = "Physics",
            University = 2,
            Rank = 1
        };

        _mockInstructorRepo.Setup(r => r.GetInstructorByUserIdAsync(userId)).ReturnsAsync(instructor);
        _mockInstructorRepo.Setup(r => r.UpdateInstructorAsync(It.IsAny<Instructor>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.EditInstructor(editDto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("E002", result.Data.EmployeeCode);
        Assert.Equal("Physics", result.Data.Department);
        Assert.Equal("Associate", result.Data.Rank);
    }

    [Fact]
    public async Task EditInstructor_ReturnsFailure_WhenInstructorNotFound()
    {
        // Arrange
        int userId = 1;
        SetHttpContextWithUser(userId);

        var editDto = new EditInstructorDto
        {
            EmployeeCode = "E002",
            Department = "Physics",
            University = 2,
            Rank = 3
        };

        _mockInstructorRepo.Setup(r => r.GetInstructorByUserIdAsync(userId)).ReturnsAsync((Instructor?)null);

        // Act
        var result = await _sut.EditInstructor(editDto);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetCurrentInstructor_ReturnsInstructor_WhenFound()
    {
        // Arrange
        int userId = 1;
        SetHttpContextWithUser(userId);

        var instructor = new Instructor { Id = 1, UserId = userId };
        _mockInstructorRepo.Setup(r => r.GetInstructorByUserIdAsync(userId)).ReturnsAsync(instructor);

        // Act
        var result = await _sut.GetCurrentInstructor();

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetCurrentInstructor_ReturnsFailure_WhenNotFound()
    {
        // Arrange
        int userId = 1;
        SetHttpContextWithUser(userId);
        _mockInstructorRepo.Setup(r => r.GetInstructorByUserIdAsync(userId)).ReturnsAsync((Instructor?)null);

        // Act
        var result = await _sut.GetCurrentInstructor();

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
    }
}
