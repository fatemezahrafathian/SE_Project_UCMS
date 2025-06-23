using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using UCMS.DTOs;
using UCMS.DTOs.ProjectDto;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.ProjectRepository.Abstarction;
using UCMS.Resources;
using UCMS.Services.FileService;
using UCMS.Services.ProjectService;
using Xunit.Abstractions;

namespace UCMS_Test.Service;

public class ProjectServiceTest
{
    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextMock = new();
    private readonly Mock<IFileService> _fileServiceMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly ProjectService _service;
    private readonly Mock<IClassRepository> _classRepoMock = new();
    private readonly Mock<IStudentClassRepository> _studentClassRepositoryMock = new();
    private readonly ITestOutputHelper _output;
    

    public ProjectServiceTest(ITestOutputHelper output)
    {
        _httpContextMock.Setup(x => x.HttpContext!.Items).Returns(new Dictionary<object, object>()!);
        _service = new ProjectService(
            _projectRepoMock.Object,
            _httpContextMock.Object,
            _mapperMock.Object,
            _classRepoMock.Object,
            _fileServiceMock.Object,
            _studentClassRepositoryMock.Object
        );
        _output = output;
    }
    [Fact]
    public async Task Should_Return_Failure_When_Class_Not_Found()
    {
        _classRepoMock.Setup(r => r.GetClassByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Class?)null);

        var user = new User { Instructor = new Instructor { Id = 1 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        var result = await _service.CreateProjectAsync(1, new CreateProjectDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.ProjectNotFound, result.Message);
    }
    [Fact]
    public async Task Should_Return_Failure_When_Instructor_Is_Invalid()
    {
        var currentClass = new Class { InstructorId = 999 };
        _classRepoMock.Setup(r => r.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(currentClass);

        var user = new User { Instructor = new Instructor { Id = 1 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        var result = await _service.CreateProjectAsync(1, new CreateProjectDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.InvalidIstructorForThisClass, result.Message);
    }
    [Fact]
    public async Task Should_Return_Failure_When_Project_Start_Before_Class()
    {
        var dto = new CreateProjectDto
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(5),
            Title = "Test"
        };

        var currentClass = new Class
        {
            InstructorId = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
        };

        var user = new User { Instructor = new Instructor { Id = 1 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepoMock.Setup(r => r.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(currentClass);

        var result = await _service.CreateProjectAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.ProjectStartDateCannotBeBeforeClassStartDate, result.Message);
    }
    [Fact]
    public async Task Should_Return_Failure_When_Project_End_After_Class()
    {
        var dto = new CreateProjectDto
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Title = "Test"
        };

        var currentClass = new Class
        {
            InstructorId = 1,
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))
        };

        var user = new User { Instructor = new Instructor { Id = 1 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepoMock.Setup(r => r.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(currentClass);

        var result = await _service.CreateProjectAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.ProjectEndDateCannotBeAfterClassEndDate, result.Message);
    }
    [Fact]
    public async Task Should_Return_Failure_When_Project_Title_Is_Duplicated()
    {
        var dto = new CreateProjectDto
        {
            Title = "Duplicate Title",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1)
        };

        var currentClass = new Class
        {
            InstructorId = 1
        };

        var user = new User { Instructor = new Instructor { Id = 1 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepoMock.Setup(r => r.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(currentClass);
        _projectRepoMock.Setup(r => r.IsProjectNameDuplicateAsync(It.IsAny<int>(), dto.Title))
            .ReturnsAsync(true);

        var result = await _service.CreateProjectAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.titleIsDuplicated, result.Message);
    }
    [Fact]
    public async Task Should_Return_Failure_When_Dto_Is_Invalid()
    {
        var dto = new CreateProjectDto
        {
            Title = "", // Invalid title
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1)
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };
        var currentClass = new Class { Id = 1, InstructorId = 1 };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepoMock.Setup(c => c.GetClassByIdAsync(1)).ReturnsAsync(currentClass);
        _projectRepoMock.Setup(p => p.IsProjectNameDuplicateAsync(1, "")).ReturnsAsync(false);

        var result = await _service.CreateProjectAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.TitleIsRequired, result.Message); // فرض بر اینکه ولیداتور همین پیام را برمی‌گرداند
    }

    [Fact]
    public async Task Should_Save_File_When_File_Provided()
    {
        var fileMock = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("fake data")), 0, 10, "Data", "test.pdf");

        var dto = new CreateProjectDto
        {
            Title = "Test Project",
            StartDate = DateTime.UtcNow.AddMinutes(1),
            EndDate = DateTime.UtcNow.AddDays(1),
            ProjectType = 1,
            GroupSize = 3,
            TotalScore = 100,
            ProjectFile = fileMock
        };

        var user = new User { Instructor = new Instructor { Id = 1 } };
        var currentClass = new Class { Id = 1, InstructorId = 1 };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepoMock.Setup(c => c.GetClassByIdAsync(1)).ReturnsAsync(currentClass);
        _projectRepoMock.Setup(p => p.IsProjectNameDuplicateAsync(1, "Test Project")).ReturnsAsync(false);
        _fileServiceMock.Setup(f => f.SaveFileAsync(fileMock, "projects")).ReturnsAsync("path/to/saved/file");
        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);
        _mapperMock.Setup(m => m.Map<Project>(dto)).Returns(new Project());
        _mapperMock.Setup(m => m.Map<GetProjectForInstructorDto>(It.IsAny<Project>())).Returns(new GetProjectForInstructorDto());

        var result = await _service.CreateProjectAsync(1, dto);

        Assert.True(result.Success);
        _fileServiceMock.Verify(f => f.SaveFileAsync(fileMock, "projects"), Times.Once);
    }

    [Fact]
    public async Task Should_Not_Save_File_When_ProjectFile_Is_Null()
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            Title = "Test Project",
            StartDate = DateTime.UtcNow.AddMinutes(1),
            EndDate = DateTime.UtcNow.AddDays(1),
            ProjectType = 1,
            GroupSize = 3,
            TotalScore = 100,
            ProjectFile = null // No file
        };

        var user = new User { Instructor = new Instructor { Id = 1 } };
        var currentClass = new Class
        {
            Id = 1,
            InstructorId = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepoMock.Setup(c => c.GetClassByIdAsync(1)).ReturnsAsync(currentClass);
        _projectRepoMock.Setup(p => p.IsProjectNameDuplicateAsync(1, "Test Project")).ReturnsAsync(false);
        _mapperMock.Setup(m => m.Map<Project>(dto)).Returns(new Project());
        _mapperMock.Setup(m => m.Map<GetProjectForInstructorDto>(It.IsAny<Project>())).Returns(new GetProjectForInstructorDto());

        // Act
        var result = await _service.CreateProjectAsync(1, dto);

        // Assert
        Assert.True(result.Success);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task Should_Create_Project_When_All_Valid()
    {
        var dto = new CreateProjectDto
        {
            Title = "Test Project",
            StartDate = DateTime.UtcNow.AddMinutes(1),
            EndDate = DateTime.UtcNow.AddDays(1),
            ProjectType = 1,
            GroupSize = 3,
            TotalScore = 100
        };

        var currentClass = new Class
        {
            Id = 1,
            InstructorId = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))
        };

        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepoMock.Setup(r => r.GetClassByIdAsync(1)).ReturnsAsync(currentClass);
        _projectRepoMock.Setup(r => r.IsProjectNameDuplicateAsync(1, dto.Title)).ReturnsAsync(false);

        _mapperMock.Setup(m => m.Map<Project>(It.IsAny<CreateProjectDto>())).Returns(new Project());
        _mapperMock.Setup(m => m.Map<GetProjectForInstructorDto>(It.IsAny<Project>()))
            .Returns(new GetProjectForInstructorDto());

        _projectRepoMock.Setup(r => r.AddAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);

        var result = await _service.CreateProjectAsync(1, dto);

        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectCreatedSuccessfully, result.Message);
        _projectRepoMock.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Once);
    }

    [Fact]
    public async Task Update_Should_Return_Failure_When_Project_Not_Found()
    {
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Project?)null);

        var result = await _service.UpdateProjectAsync(1, new PatchProjectDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.ProjectNotFound, result.Message);
        _projectRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async Task Update_Should_Return_Failure_When_Instructor_Is_Invalid()
    {
        var project = new Project
        {
            Class = new Class { InstructorId = 999 }
        };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });

        var result = await _service.UpdateProjectAsync(1, new PatchProjectDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.InvalidInstructorForThisClass, result.Message);
    }

    [Fact]
    public async Task Update_Should_Return_Failure_When_StartDate_Before_ClassStart()
    {
        var project = new Project
        {
            Class = new Class
            {
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5).Date),
                InstructorId = 1
            }
        };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });

        var dto = new PatchProjectDto { StartDate = DateTime.UtcNow };

        var result = await _service.UpdateProjectAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.ProjectStartDateCannotBeBeforeClassStartDate, result.Message);
    }

    [Fact]
    public async Task Update_Should_Return_Failure_When_Project_EndDate_Is_After_Class_EndDate()
    {
        // Arrange
        var instructorId = 1;

        var @class = new Class
        {
            InstructorId = instructorId,
            EndDate = new DateOnly(2025, 12, 31)
        };

        var project = new Project
        {
            Id = 1,
            Class = @class,
            StartDate = new DateTime(2025, 1, 1),
            EndDate = new DateTime(2025, 10, 1)
        };

        var dto = new PatchProjectDto
        {
            EndDate = new DateTime(2026, 1, 1) // After class end date
        };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(project.Id))
            .ReturnsAsync(project);

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"])
            .Returns(new User { Instructor = new Instructor { Id = instructorId } });

        // Act
        var result = await _service.UpdateProjectAsync(project.Id, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ProjectEndDateCannotBeAfterClassEndDate, result.Message);
        _projectRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Project>()), Times.Never);
    }
    
    [Fact]
    public async Task Update_Should_Return_Validation_Error_When_Dto_Is_Invalid()
    {
        // Arrange
        var dto = new PatchProjectDto
        {
            Title = "new",
            StartDate = DateTime.UtcNow.AddMinutes(10),
            EndDate = DateTime.UtcNow.AddMinutes(20),
            TotalScore = -1,
        };

        var project = new Project { Class = new Class { InstructorId = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);

        // Act
        var result = await _service.UpdateProjectAsync(1, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.TotalScoreMustBePositive, result.Message);

        _projectRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Project>()), Times.Never);
    }
    
    [Fact]
    public async Task Update_Should_Return_Failure_When_Title_Already_Exists()
    {
        var project = new Project
        {
            Id = 1,
            Class = new Class { Id = 2, InstructorId = 1 },
            ClassId = 2
        };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });


        var dto = new PatchProjectDto
        {
            Title = "Test",
            StartDate = DateTime.UtcNow.AddMinutes(1),
            EndDate = DateTime.UtcNow.AddDays(1),
            TotalScore = 100
        };
        _projectRepoMock.Setup(r => r.IsProjectNameDuplicateAsync(2, "Test")).ReturnsAsync(true);


        var result = await _service.UpdateProjectAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.titleIsDuplicated, result.Message);
    }

    [Fact]
    public async Task Update_Should_Replace_File_If_New_File_Provided()
    {
        var user = new User { Instructor = new Instructor { Id = 1 } };
        var context = new DefaultHttpContext();
        context.Items["User"] = user;
        _httpContextMock.Setup(a => a.HttpContext).Returns(context);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("new.pdf");

        var project = new Project
        {
            Id = 1,
            Title = "Old",
            Class = new Class { Id = 5, InstructorId = 1 },
            ProjectFilePath = "old/path.pdf"
        };

        var dto = new PatchProjectDto
        {
            Title = "Updated",
            ProjectFile = fileMock.Object,
        };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(1)).ReturnsAsync(project);
        _projectRepoMock.Setup(r => r.IsProjectNameDuplicateAsync(5,dto.Title)).ReturnsAsync(false);
        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.SaveFileAsync(fileMock.Object, "projects")).ReturnsAsync("new/path.pdf");
        _mapperMock.Setup(m => m.Map(dto, project));
        _projectRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);

        var result = await _service.UpdateProjectAsync(1, dto);

        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectUpdatedSuccessfully, result.Message);
        _fileServiceMock.Verify(f => f.DeleteFile("old/path.pdf"), Times.Once);
        _fileServiceMock.Verify(f => f.SaveFileAsync(fileMock.Object, "projects"), Times.Once);
    }

    [Fact]
    public async Task Update_Should_Update_Project_Successfully_Without_File()
    {
        var project = new Project
        {
            Id = 1,
            Title = "Old Title",
            Class = new Class { InstructorId = 1 },
            ProjectFilePath = null
        };

        var dto = new PatchProjectDto
        {
            Title = "New Title"
        };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(1)).ReturnsAsync(project);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });
        _projectRepoMock.Setup(r => r.IsProjectNameDuplicateAsync(project.Class.Id, dto.Title!)).ReturnsAsync(false);

        _mapperMock.Setup(m => m.Map(dto, project))
            .Callback<PatchProjectDto, Project>((src, dest) => {
                dest.Title = src.Title!;
            });

        _projectRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Project>())).Returns(Task.CompletedTask);

        var result = await _service.UpdateProjectAsync(1, dto);

        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectUpdatedSuccessfully, result.Message);
        Assert.Equal(dto.Title, project.Title);
    }
    
    [Fact]
    public async Task Update_Should_Set_UpdatedAt_After_Update()
    {
        var project = new Project
        {
            Title = "Title",
            Class = new Class { InstructorId = 1 }
        };

        var dto = new PatchProjectDto
        {
            Title = "Updated"
        };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });
        _projectRepoMock.Setup(r => r.IsProjectNameDuplicateAsync(project.Class.Id,dto.Title!)).ReturnsAsync(false);

        var before = DateTime.UtcNow;

        var result = await _service.UpdateProjectAsync(1, dto);

        Assert.True(result.Success);
        Assert.True(project.UpdatedAt >= before);
    }

    [Fact]
    public async Task DeleteProjectAsync_Should_Return_Failure_When_Project_Not_Found()
    {
        // Arrange
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync((Project?)null);

        // Act
        var result = await _service.DeleteProjectAsync(1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ProjectNotFound, result.Message);
        _fileServiceMock.Verify(f => f.DeleteFile(It.IsAny<string>()), Times.Never);
        _projectRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProjectAsync_Should_Return_Failure_When_User_Has_No_Access()
    {
        // Arrange
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 2 } });

        var project = new Project { Class = new Class { InstructorId = 1 }, ProjectFilePath = null };
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);

        // Act
        var result = await _service.DeleteProjectAsync(1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ProjectCantBeAccessed, result.Message);
        _fileServiceMock.Verify(f => f.DeleteFile(It.IsAny<string>()), Times.Never);
        _projectRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProjectAsync_Should_Delete_Project_Without_File()
    {
        // Arrange
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });

        var project = new Project { Class = new Class { InstructorId = 1 }, ProjectFilePath = null };
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _projectRepoMock.Setup(r => r.DeleteAsync(project)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteProjectAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectDeletedSuccessfully, result.Message);
        _fileServiceMock.Verify(f => f.DeleteFile(It.IsAny<string>()), Times.Never);
        _projectRepoMock.Verify(r => r.DeleteAsync(project), Times.Once);
    }

    [Fact]
    public async Task DeleteProjectAsync_Should_Delete_Project_With_File()
    {
        // Arrange
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });

        var project = new Project { Class = new Class { InstructorId = 1 }, ProjectFilePath = "path/to/file.pdf" };
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _projectRepoMock.Setup(r => r.DeleteAsync(project)).Returns(Task.CompletedTask);
        _fileServiceMock.Setup(f => f.DeleteFile(project.ProjectFilePath));

        // Act
        var result = await _service.DeleteProjectAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectDeletedSuccessfully, result.Message);
        _fileServiceMock.Verify(f => f.DeleteFile(project.ProjectFilePath), Times.Once);
        _projectRepoMock.Verify(r => r.DeleteAsync(project), Times.Once);
    }
    
    [Fact]
    public async Task GetProjectByIdForInstructorAsync_Should_Return_Failure_If_Project_Not_Found_Or_No_Access()
    {
        // Arrange
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync((Project?)null);

        // Act
        var result = await _service.GetProjectByIdForInstructorAsync(1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ProjectCantBeAccessed, result.Message);

        // Arrange project with wrong instructor
        var project = new Project { Class = new Class { InstructorId = 2 } };
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);

        // Act again
        var result2 = await _service.GetProjectByIdForInstructorAsync(1);

        // Assert
        Assert.False(result2.Success);
        Assert.Equal(Messages.ProjectCantBeAccessed, result2.Message);
    }
    
        [Fact]
    public async Task GetProjectByIdForInstructorAsync_Should_Return_Failure_When_Project_Not_Found()
    {
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync((Project?)null);

        var result = await _service.GetProjectByIdForInstructorAsync(1);

        Assert.False(result.Success);
        Assert.Equal(Messages.ProjectCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task GetProjectByIdForInstructorAsync_Should_Return_Failure_When_User_Is_Not_Instructor_Of_Project()
    {
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });
        var project = new Project { Class = new Class { InstructorId = 2 } };
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);

        var result = await _service.GetProjectByIdForInstructorAsync(1);

        Assert.False(result.Success);
        Assert.Equal(Messages.ProjectCantBeAccessed, result.Message);
    }

    [Theory]
    [InlineData("file.pdf", "application/pdf")]
    [InlineData("document.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData(null, null)]
    [InlineData("", null)]
    public async Task GetProjectByIdForInstructorAsync_Should_Return_Success_And_Set_FileContentType_Correctly(string? filePath, string? expectedContentType)
    {
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });
        var project = new Project
        {
            Class = new Class { InstructorId = 1 }
        };
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);

        var dto = new GetProjectForInstructorDto
        {
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            ProjectFilePath = filePath
        };
        _mapperMock.Setup(m => m.Map<GetProjectForInstructorDto>(project)).Returns(dto);

        var result = await _service.GetProjectByIdForInstructorAsync(1);

        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectRetrievedSuccessfully, result.Message);
        Assert.Equal(expectedContentType, result.Data.ProjectFileContentType);
    }

    [Theory]
    [InlineData(-2, -1, ProjectStatus.Completed)]
    [InlineData(1, 2, ProjectStatus.NotStarted)]
    [InlineData(-1, 1, ProjectStatus.InProgress)]
    public async Task GetProjectByIdForInstructorAsync_Should_Calculate_ProjectStatus_Correctly(int startDayOffset, int endDayOffset, ProjectStatus expectedStatus)
    {
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });

        var project = new Project { Class = new Class { InstructorId = 1 } };
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);

        var dto = new GetProjectForInstructorDto
        {
            StartDate = DateTime.UtcNow.AddDays(startDayOffset),
            EndDate = DateTime.UtcNow.AddDays(endDayOffset),
            ProjectFilePath = "file.pdf"
        };
        _mapperMock.Setup(m => m.Map<GetProjectForInstructorDto>(project)).Returns(dto);

        var result = await _service.GetProjectByIdForInstructorAsync(1);

        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectRetrievedSuccessfully, result.Message);
        Assert.Equal(expectedStatus,result.Data.ProjectStatus);
    }

    [Fact]
    public async Task GetProjectByIdForInstructorAsync_Should_Return_Success_With_Valid_Data()
    {
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });

        var project = new Project
        {
            Class = new Class { InstructorId = 1 }
        };
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);

        var dto = new GetProjectForInstructorDto
        {
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            ProjectFilePath = "file.pdf"
        };
        _mapperMock.Setup(m => m.Map<GetProjectForInstructorDto>(project)).Returns(dto);

        var result = await _service.GetProjectByIdForInstructorAsync(1);

        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectRetrievedSuccessfully, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(dto, result.Data);
    }

    [Fact]
    public async Task GetProjectByIdForInstructorAsync_Should_Return_Success_With_Mapped_Dto()
    {
        // Arrange
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Instructor = new Instructor { Id = 1 } });
        var project = new Project
        {
            Class = new Class { InstructorId = 1 }
        };
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);

        var dto = new GetProjectForInstructorDto
        {
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            ProjectFilePath = "file.pdf"
        };

        _mapperMock.Setup(m => m.Map<GetProjectForInstructorDto>(project)).Returns(dto);

        // Act
        var result = await _service.GetProjectByIdForInstructorAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectRetrievedSuccessfully, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(dto, result.Data);
        Assert.NotNull(result.Data.ProjectStatus);
        Assert.NotNull(result.Data.ProjectFileContentType); // Assuming content type calculated
    }
    
    [Fact]
    public async Task GetProjectByIdForStudentAsync_Should_Return_Failure_When_Project_Not_Found()
    {
        // Arrange
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Student = new Student { Id = 1 } });
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync((Project)null!);

        // Act
        var result = await _service.GetProjectByIdForStudentAsync(10);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ProjectCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task GetProjectByIdForStudentAsync_Should_Return_Failure_When_Student_Not_In_Class()
    {
        // Arrange
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Student = new Student { Id = 1 } });
        var project = new Project { ClassId = 5 };
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(project.ClassId, 1)).ReturnsAsync(false);

        // Act
        var result = await _service.GetProjectByIdForStudentAsync(10);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ProjectCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task GetProjectByIdForStudentAsync_Should_Return_Success_With_Correct_Data_When_Student_In_Class()
    {
        // Arrange
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(new User { Student = new Student { Id = 1 } });
        var project = new Project
        {
            Id = 10,
            ClassId = 5,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            ProjectFilePath = "file.pdf"
        };

        var dto = new GetProjectForStudentDto
        {
            Id = 10,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            ProjectFilePath = project.ProjectFilePath
        };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(10)).ReturnsAsync(project);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(project.ClassId, 1)).ReturnsAsync(true);
        _mapperMock.Setup(m => m.Map<GetProjectForStudentDto>(project)).Returns(dto);

        // Act
        var result = await _service.GetProjectByIdForStudentAsync(10);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectRetrievedSuccessfully, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Id, result.Data.Id);
        Assert.Equal(dto.ProjectFilePath, result.Data.ProjectFilePath);
    }
    
    [Fact]
    public async Task HandleDownloadProjectFileAsync_Should_Return_Failure_When_Project_Not_Found()
    {
        // Arrange
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync((Project)null!);

        // Act
        var result = await _service.HandleDownloadProjectFileAsync(1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.FileDoesNotExist, result.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleDownloadProjectFileAsync_Should_Return_Failure_When_ProjectFilePath_Is_NullOrEmpty(string filePath)
    {
        // Arrange
        var project = new Project { ProjectFilePath = filePath };
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);

        // Act
        var result = await _service.HandleDownloadProjectFileAsync(1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.FileDoesNotExist, result.Message);
    }

    [Fact]
    public async Task HandleDownloadProjectFileAsync_Should_Return_Failure_When_FileService_Returns_Null()
    {
        // Arrange
        var project = new Project { ProjectFilePath = "somepath.pdf" };
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _fileServiceMock.Setup(f => f.DownloadFile(project.ProjectFilePath)).ReturnsAsync((FileDownloadDto?)null);

        // Act
        var result = await _service.HandleDownloadProjectFileAsync(1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.FileDoesNotExist, result.Message);
    }

    [Fact]
    public async Task HandleDownloadProjectFileAsync_Should_Return_Success_When_File_Downloaded()
    {
        // Arrange
        var project = new Project { ProjectFilePath = "document.pdf" };
        var fileDto = new FileDownloadDto
        {
            FileName = "document.pdf",
            FileBytes = new byte[] { 1, 2, 3 },
            ContentType = null
        };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _fileServiceMock.Setup(f => f.DownloadFile(project.ProjectFilePath)).ReturnsAsync(fileDto);

        // Act
        var result = await _service.HandleDownloadProjectFileAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectFileDownloadedSuccessfully, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(fileDto.FileName, result.Data.FileName);
        Assert.Equal(fileDto.FileBytes, result.Data.FileBytes);
    }
    
    [Fact]
    public async Task GetProjectsForInstructor_Should_Return_Projects_List_Successfully()
    {
        // Arrange
        var user = new User { Instructor = new Instructor { Id = 10 } };
        var dto = new FilterProjectsForInstructorDto
        {
            Title = "Test Project",
            ClassTitle = "Class A",
            OrderBy = "Title",
            Descending = true
        };

        var projectEntities = new List<Project>
        {
            new Project { Id = 1, Title = "Test Project 1" },
            new Project { Id = 2, Title = "Test Project 2" }
        };

        var projectDtos = new List<GetProjectListForInstructorDto>
        {
            new GetProjectListForInstructorDto { Id = 1, Title = "Test Project 1" },
            new GetProjectListForInstructorDto { Id = 2, Title = "Test Project 2" }
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _projectRepoMock.Setup(r => r.FilterProjectsForInstructorAsync(
                user.Instructor.Id, dto.Title, dto.ClassTitle, dto.ProjectStatus, dto.OrderBy, dto.Descending))
            .ReturnsAsync(projectEntities);

        _mapperMock.Setup(m => m.Map<List<GetProjectListForInstructorDto>>(projectEntities))
            .Returns(projectDtos);

        // Act
        var result = await _service.GetProjectsForInstructor(dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectsRetrievedSuccessfully, result.Message);
        Assert.Equal(projectDtos, result.Data);

        _projectRepoMock.Verify(r => r.FilterProjectsForInstructorAsync(
            user.Instructor.Id, dto.Title, dto.ClassTitle, dto.ProjectStatus, dto.OrderBy, dto.Descending), Times.Once);

        _mapperMock.Verify(m => m.Map<List<GetProjectListForInstructorDto>>(projectEntities), Times.Once);
    }

    [Fact]
    public async Task GetProjectsForStudent_Should_Return_Projects_List_Successfully()
    {
        // Arrange
        var user = new User { Student = new Student { Id = 20 } };
        var dto = new FilterProjectsForStudentDto
        {
            Title = "Student Project",
            ClassTitle = "Class B",
            OrderBy = "StartDate",
            Descending = false
        };

        var projectEntities = new List<Project>
        {
            new Project { Id = 3, Title = "Student Project 1" },
            new Project { Id = 4, Title = "Student Project 2" }
        };

        var projectDtos = new List<GetProjectListForStudentDto>
        {
            new GetProjectListForStudentDto { Id = 3, Title = "Student Project 1" },
            new GetProjectListForStudentDto { Id = 4, Title = "Student Project 2" }
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _projectRepoMock.Setup(r => r.FilterProjectsForStudentAsync(
                user.Student.Id, dto.Title, dto.ClassTitle, dto.ProjectStatus, dto.OrderBy, dto.Descending))
            .ReturnsAsync(projectEntities);

        _mapperMock.Setup(m => m.Map<List<GetProjectListForStudentDto>>(projectEntities))
            .Returns(projectDtos);

        // Act
        var result = await _service.GetProjectsForStudent(dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectsRetrievedSuccessfully, result.Message);
        Assert.Equal(projectDtos, result.Data);

        _projectRepoMock.Verify(r => r.FilterProjectsForStudentAsync(
            user.Student.Id, dto.Title, dto.ClassTitle, dto.ProjectStatus, dto.OrderBy, dto.Descending), Times.Once);

        _mapperMock.Verify(m => m.Map<List<GetProjectListForStudentDto>>(projectEntities), Times.Once);
    }

    [Fact]
    public async Task GetProjectsOfClassForInstructorAsync_Should_Return_Failure_If_Class_Not_Found()
    {
        // Arrange
        int classId = 1;
        var user = new User { Instructor = new Instructor { Id = 10 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepoMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync((Class?)null);

        // Act
        var result = await _service.GetProjectsOfClassForInstructorAsync(classId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ClassNotFound, result.Message);

        _projectRepoMock.Verify(r => r.GetProjectsByClassIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetProjectsOfClassForInstructorAsync_Should_Return_Failure_If_User_Not_Instructor_Of_Class()
    {
        // Arrange
        int classId = 1;
        var user = new User { Instructor = new Instructor { Id = 10 } };
        var classEntity = new Class { Id = classId, InstructorId = 99 };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepoMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(classEntity);

        // Act
        var result = await _service.GetProjectsOfClassForInstructorAsync(classId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.InvalidIstructorForThisClass, result.Message);

        _projectRepoMock.Verify(r => r.GetProjectsByClassIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetProjectsOfClassForInstructorAsync_Should_Return_Projects_List_When_Valid()
    {
        // Arrange
        int classId = 1;
        var user = new User { Instructor = new Instructor { Id = 10 } };
        var classEntity = new Class { Id = classId, InstructorId = user.Instructor.Id };
        var projects = new List<Project>
        {
            new Project { Id = 1, Title = "Project 1" },
            new Project { Id = 2, Title = "Project 2" }
        };
        var projectDtos = new List<GetProjectsOfClassDto>
        {
            new GetProjectsOfClassDto { Id = 1, Title = "Project 1" },
            new GetProjectsOfClassDto { Id = 2, Title = "Project 2" }
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepoMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(classEntity);
        _projectRepoMock.Setup(r => r.GetProjectsByClassIdAsync(classId)).ReturnsAsync(projects);
        _mapperMock.Setup(m => m.Map<List<GetProjectsOfClassDto>>(projects)).Returns(projectDtos);

        // Act
        var result = await _service.GetProjectsOfClassForInstructorAsync(classId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectsRetrievedSuccessfully, result.Message);
        Assert.Equal(projectDtos, result.Data);

        _projectRepoMock.Verify(r => r.GetProjectsByClassIdAsync(classId), Times.Once);
        _mapperMock.Verify(m => m.Map<List<GetProjectsOfClassDto>>(projects), Times.Once);
    }

    [Fact]
    public async Task GetProjectsOfClassForStudentAsync_Should_Return_Failure_If_Class_Not_Found()
    {
        // Arrange
        int classId = 1;
        var user = new User { Student = new Student { Id = 20 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepoMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync((Class?)null);

        // Act
        var result = await _service.GetProjectsOfClassForStudentAsync(classId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ClassNotFound, result.Message);

        _projectRepoMock.Verify(r => r.GetProjectsByClassIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetProjectsOfClassForStudentAsync_Should_Return_Failure_If_Student_Not_In_Class()
    {
        // Arrange
        int classId = 1;
        var user = new User { Student = new Student { Id = 20 } };
        var classEntity = new Class { Id = classId, InstructorId = 10 };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepoMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(classEntity);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(classId, user.Student.Id)).ReturnsAsync(false);

        // Act
        var result = await _service.GetProjectsOfClassForStudentAsync(classId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.StudentNotInClass, result.Message);

        _projectRepoMock.Verify(r => r.GetProjectsByClassIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetProjectsOfClassForStudentAsync_Should_Return_Projects_List_When_Valid()
    {
        // Arrange
        int classId = 1;
        var user = new User { Student = new Student { Id = 20 } };
        var classEntity = new Class { Id = classId, InstructorId = 10 };
        var projects = new List<Project>
        {
            new Project { Id = 1, Title = "Project 1" },
            new Project { Id = 2, Title = "Project 2" }
        };
        var projectDtos = new List<GetProjectsOfClassDto>
        {
            new GetProjectsOfClassDto { Id = 1, Title = "Project 1" },
            new GetProjectsOfClassDto { Id = 2, Title = "Project 2" }
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepoMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(classEntity);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(classId, user.Student.Id)).ReturnsAsync(true);
        _projectRepoMock.Setup(r => r.GetProjectsByClassIdAsync(classId)).ReturnsAsync(projects);
        _mapperMock.Setup(m => m.Map<List<GetProjectsOfClassDto>>(projects)).Returns(projectDtos);

        // Act
        var result = await _service.GetProjectsOfClassForStudentAsync(classId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ProjectsRetrievedSuccessfully, result.Message);
        Assert.Equal(projectDtos, result.Data);

        _projectRepoMock.Verify(r => r.GetProjectsByClassIdAsync(classId), Times.Once);
        _mapperMock.Verify(m => m.Map<List<GetProjectsOfClassDto>>(projects), Times.Once);
    }

}