using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using UCMS.DTOs.ClassDto;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.ClassService.Abstraction;
using UCMS.Services.ClassService;
using UCMS.Services.ImageService;
using UCMS.Services.PasswordService.Abstraction;

namespace UCMS_Test.Service
{
    public class ClassServiceTest

    {
        private readonly Mock<IClassRepository> _classRepoMock;
        private readonly IMapper _mapper;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IImageService> _imageServiceMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<IStudentClassService> _studentClassServiceMock;
        private readonly ClassService _classService;

        public ClassServiceTest()
        {
            _classRepoMock = new Mock<IClassRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _imageServiceMock = new Mock<IImageService>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _studentClassServiceMock = new Mock<IStudentClassService>();

            // Setup AutoMapper with real profiles or simple config
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateClassDto, Class>();
                cfg.CreateMap<Class, GetClassForInstructorDto>();
                cfg.CreateMap<PatchClassDto, Class>();
                cfg.CreateMap<List<Class>, GetClassPageDto>();
            });
            _mapper = config.CreateMapper();

            // Setup HttpContextAccessor to simulate logged-in user with Instructor
            var user = new User
            {
                Instructor = new Instructor { Id = 123 }
            };
            var context = new DefaultHttpContext();
            context.Items["User"] = user;
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

            _classService = new ClassService(
                _classRepoMock.Object,
                _mapper,
                _httpContextAccessorMock.Object,
                _imageServiceMock.Object,
                _passwordServiceMock.Object,
                _studentClassServiceMock.Object);
        }

 [Fact]
public async Task CreateClass_ShouldReturnSuccess_WhenValidDto()
{
    // Arrange
    var dto = new CreateClassDto
    {
        Title = "Test Class",
        Password = "Password123!",
        ConfirmPassword = "Password123!",
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
        EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
        ProfileImage = null,
        Schedules = new List<ClassScheduleDto>()
    };

    // Mock IHttpContextAccessor to return a user with Instructor.Id
    var user = new User
    {
        Instructor = new Instructor { Id = 1 }
    };
    _httpContextAccessorMock.Setup(h => h.HttpContext.Items["User"]).Returns(user);

    // Mock PasswordService
    _passwordServiceMock.Setup(s => s.CreateSalt()).Returns(Encoding.UTF8.GetBytes("salt123"));
    _passwordServiceMock.Setup(s => s.HashPasswordAsync(dto.Password, Encoding.UTF8.GetBytes("salt123")))
        .ReturnsAsync(Encoding.UTF8.GetBytes("hashedpassword"));

    // Mock ClassRepository
    _classRepoMock.Setup(r => r.ClassCodeExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
    _classRepoMock.Setup(r => r.AddClassAsync(It.IsAny<Class>())).Returns(Task.CompletedTask);

    // Mock ImageService if validator or service uses it internally
    _imageServiceMock.Setup(i => i.SaveImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
        .ReturnsAsync("someimageurl.jpg");

    // Act
    var response = await _classService.CreateClass(dto);

    // Assert
    Assert.True(response.Success);
    Assert.NotNull(response.Data);
    Assert.Equal("Class was created successfully.", response.Message);
    _classRepoMock.Verify(r => r.AddClassAsync(It.IsAny<Class>()), Times.Once);
}

        [Fact]
        public async Task GetClassForInstructor_ShouldReturnFailure_WhenClassNotFound()
        {
            // Arrange
            _classRepoMock.Setup(c => c.GetInstructorClassByClassIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Class)null);

            // Act
            var response = await _classService.GetClassForInstructor(1);

            // Assert
            Assert.False(response.Success);
            Assert.Equal("Class not found.", response.Message);
        }

        [Fact]
        public async Task DeleteClass_ShouldReturnFailure_WhenClassNotBelongToInstructor()
        {
            // Arrange
            var classEntity = new Class { InstructorId = 999 };
            _classRepoMock.Setup(c => c.GetClassByIdAsync(1)).ReturnsAsync(classEntity);

            // Act
            var response = await _classService.DeleteClass(1);

            // Assert
            Assert.False(response.Success);
            Assert.Equal("Class can't be accessed.", response.Message);
        }

        [Fact]
        public async Task DeleteClass_ShouldReturnSuccess_WhenClassDeleted()
        {
            // Arrange
            var classEntity = new Class { InstructorId = 123 };
            _classRepoMock.Setup(c => c.GetClassByIdAsync(1)).ReturnsAsync(classEntity);
            _classRepoMock.Setup(c => c.DeleteClassAsync(classEntity)).Returns(Task.CompletedTask);

            // Act
            var response = await _classService.DeleteClass(1);

            // Assert
            Assert.True(response.Success);
            Assert.Equal("Class deleted successfully.", response.Message);
            _classRepoMock.Verify(c => c.DeleteClassAsync(classEntity), Times.Once);
        }

        [Fact]
        public async Task UpdateClassPartial_ShouldReturnSuccess_WhenDtoIsValid()
        {
            // Arrange
            var classEntity = new Class
            {
                Id = 1,
                InstructorId = 123,
                Title = "Old Title",
                Description = "Old Description",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))
            };

            _classRepoMock
                .Setup(c => c.GetInstructorClassByClassIdAsync(classEntity.Id))
                .ReturnsAsync(classEntity);

            _classRepoMock
                .Setup(c => c.UpdateClassAsync(It.IsAny<Class>()))
                .Returns(Task.CompletedTask);

            var dto = new PatchClassDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                ProfileImage = null
            };

            // Act
            var result = await _classService.UpdateClassPartial(classEntity.Id, dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Class updated successfully.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(dto.Title, result.Data.Title);
            Assert.Equal(dto.Description, result.Data.Description);
            _classRepoMock.Verify(c => c.UpdateClassAsync(It.IsAny<Class>()), Times.Once);
        }

        [Fact]
        public async Task UpdateClassPartial_ShouldReturnFailure_WhenStartDateIsAfterEndDate()
        {
            // Arrange
            var classEntity = new Class
            {
                Id = 1,
                InstructorId = 123,
                Title = "Old Title",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))
            };

            _classRepoMock
                .Setup(c => c.GetInstructorClassByClassIdAsync(classEntity.Id))
                .ReturnsAsync(classEntity);

            var dto = new PatchClassDto
            {
                Title = "Updated Title",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                ProfileImage = null
            };

            // Act
            var result = await _classService.UpdateClassPartial(classEntity.Id, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Start date cannot be later than end date.", result.Message);
            _classRepoMock.Verify(c => c.UpdateClassAsync(It.IsAny<Class>()), Times.Never);
        }

    }
}
