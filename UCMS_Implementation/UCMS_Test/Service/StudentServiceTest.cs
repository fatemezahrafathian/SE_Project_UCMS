using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UCMS.DTOs.Student;
using UCMS.Models;
using UCMS.Repositories.StudentRepository.Abstraction;
using UCMS.Repositories.UserRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.StudentService;
using UCMS.Services.Utils;

namespace UCMS_Test.Service;
public class StudentServiceTest
{
    private readonly Mock<IStudentRepository> _mockStudentRepo = new();
    private readonly Mock<IUserRepository> _mockUserRepo = new();
    private readonly IMapper _mapper;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
    private readonly Mock<ILogger<StudentService>> _mockLogger = new();
    private readonly Mock<UrlBuilder> _urlBuilderMock = new();
    private readonly StudentService _sut;

    public StudentServiceTest()
    {
        var services = new ServiceCollection();
        services.AddAutoMapper(typeof(UCMS.Profile.AutoMapperProfile));
        var provider = services.BuildServiceProvider();

        _mapper = provider.GetRequiredService<IMapper>();

        _sut = new StudentService(
            _mockStudentRepo.Object,
            _mapper,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object,
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
    public async Task GetCurrentStudent_ReturnsStudent_WhenFound()
    {
        // Arrange
        int userId = 1;
        SetHttpContextWithUser(userId);

        var student = new Student { UserId = userId };
        var dto = new StudentProfileDto();

        _mockStudentRepo.Setup(r => r.GetStudentByUserIdAsync(userId))
            .ReturnsAsync(student);

        // Act
        var result = await _sut.GetCurrentStudent();

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task EditStudentAsync_ReturnsUpdatedStudent_WhenValidInput()
    {
        // Arrange
        var user = new User { Id = 1 };
        var student = new Student
        {
            Id = 1,
            UserId = 1,
            StudentNumber = "123",
            Major = "Engineering",
            EnrollmentYear = 2020,
            EducationLevel = EducationLevel.Bachelor
        };

        var editDto = new EditStudentDto
        {
            StudentNumber = "456",
            Major = "Computer Science",
            EnrollmentYear = 2021,
            EducationLevel = (int)EducationLevel.Master
        };

        var context = new DefaultHttpContext();
        context.Items["User"] = user;
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);

        _mockStudentRepo.Setup(r => r.GetStudentByUserIdAsync(user.Id)).ReturnsAsync(student);
        _mockStudentRepo.Setup(r => r.UpdateStudentAsync(It.IsAny<Student>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.EditStudentAsync(editDto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("456", result.Data.StudentNumber);
        Assert.Equal("Computer Science", result.Data.Major);
        Assert.Equal(2021, result.Data.EnrollmentYear);
        Assert.Equal("Master", result.Data.EducationLevel);
    }

    [Fact]
    public async Task GetAllStudents_ReturnsList()
    {
        // Arrange
        var students = new List<Student> { new Student(), new Student() };
        var dtos = new List<StudentPreviewDto> { new StudentPreviewDto(), new StudentPreviewDto() };

        _mockStudentRepo.Setup(r => r.GetAllStudentsAsync()).ReturnsAsync(students);

        // Act
        var result = await _sut.GetAllStudents();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.AllUsersFetchedSuccessfully, result.Message);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task EditStudentAsync_ReturnsFailure_WhenStudentNotFound()
    {
        // Arrange
        var user = new User { Id = 999 };
        var editDto = new EditStudentDto
        {
            StudentNumber = "999",
            Major = "Ghost Major",
            EnrollmentYear = 2099,
            EducationLevel = (int)EducationLevel.Doctorate
        };

        var context = new DefaultHttpContext();
        context.Items["User"] = user;
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);

        _mockStudentRepo.Setup(r => r.GetStudentByUserIdAsync(user.Id)).ReturnsAsync((Student?)null);

        // Act
        var result = await _sut.EditStudentAsync(editDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(string.Format(Messages.UserNotFound, user.Id), result.Message);
        Assert.Null(result.Data);
    }

}
