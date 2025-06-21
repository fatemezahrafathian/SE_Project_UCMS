using System.Text;

namespace UCMS_Test.Service;
using Moq;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using UCMS.DTOs.PhaseDto;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.PhaseRepository.Abstraction;
using UCMS.Repositories.ProjectRepository.Abstarction;
using UCMS.Services.FileService;
using UCMS.Services.PhaseService;
using UCMS.Resources;

public class PhaseServiceTest
{
    private readonly Mock<IPhaseRepository> _phaseRepoMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextMock = new();
    private readonly Mock<IFileService> _fileServiceMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly PhaseService _service;
    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly Mock<IStudentClassRepository> _studentClassRepositoryMock = new();
    

    public PhaseServiceTest()
    {
        _httpContextMock.Setup(x => x.HttpContext!.Items).Returns(new Dictionary<object, object>()!);
        _service = new PhaseService(
            _phaseRepoMock.Object,
            _mapperMock.Object,
            _httpContextMock.Object,
            _projectRepoMock.Object,
            _fileServiceMock.Object,
            _studentClassRepositoryMock.Object
        );
    }
    [Fact]
    public async Task Should_Return_Failure_When_Project_Not_Found()
    {
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Project?)null);
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        var result = await _service.CreatePhaseAsync(1, new CreatePhaseDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.ProjectNotFound, result.Message);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Never);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task Should_Return_Failure_When_Instructor_Is_Invalid()
    {
        var project = new Project { Class = new Class { InstructorId = 999 } };
        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        var result = await _service.CreatePhaseAsync(1, new CreatePhaseDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.InvalidIstructorForThisProject, result.Message);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Never);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task Should_Return_Failure_When_Phase_Start_Before_Project()
    {
        var dto = new CreatePhaseDto { StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(2) };
        var project = new Project { StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(10), Class = new Class { InstructorId = 1 } };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        var result = await _service.CreatePhaseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseStartTimeCannotBeBeforeProjectStartTime, result.Message);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Never);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task Should_Return_Failure_When_Phase_End_After_Project()
    {
        var dto = new CreatePhaseDto
        {
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(11)
        };
        var project = new Project
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 }
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        var result = await _service.CreatePhaseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseEndTimeCannotBeAfterProjectEndTime, result.Message);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Never);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task Should_Return_Failure_When_Phase_Title_Already_Exists()
    {
        var dto = new CreatePhaseDto
        {
            Title = "Phase 1",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 10
        };

        var project = new Project
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 },
            Id = 1
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhasesByProjectIdAsync(project.Id))
            .ReturnsAsync(new List<Phase> { new Phase { Title = "Phase 1" } });

        var result = await _service.CreatePhaseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.PhaseAlreadyExists, result.Message);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Never);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
    }
    [Fact]
    public async Task Should_Return_Failure_When_Dto_Is_Invalid()
    {
        var dto = new CreatePhaseDto
        {
            Title = "",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 10
        };

        var project = new Project
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 },
            Id = 1
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhasesByProjectIdAsync(project.Id)).ReturnsAsync(new List<Phase>());

        var result = await _service.CreatePhaseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.TitleIsRequired, result.Message);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Never);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);

    }
    [Fact]
    public async Task Should_Save_File_When_PhaseFile_Is_Provided()
    {
        // Arrange
        var user = new User { Instructor = new Instructor { Id = 1 } };
        var project = new Project
        {
            Id = 1,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 }
        };

        var fileMock = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("file")), 0, 10, "Data", "test.pdf");
        
        var dto = new CreatePhaseDto
        {
            Title = "Test",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 5,
            PhaseFile = fileMock
        };
        
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _projectRepoMock.Setup(x => x.GetProjectByIdAsync(1)).ReturnsAsync(project);


        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "phases"))
            .ReturnsAsync("path/to/file.pdf");

        _phaseRepoMock.Setup(x => x.GetPhasesByProjectIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Phase>());
        _phaseRepoMock.Setup(x => x.AddAsync(It.IsAny<Phase>())).Returns(Task.CompletedTask);
        
        _mapperMock.Setup(m => m.Map<Phase>(It.IsAny<CreatePhaseDto>())).Returns(new Phase());
        _mapperMock.Setup(m => m.Map<GetPhaseForInstructorDto>(It.IsAny<Phase>())).Returns(new GetPhaseForInstructorDto());

        // Act
        var result = await _service.CreatePhaseAsync(1, dto);

        // Assert
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "phases"), Times.Once);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Once);
        Assert.True(result.Success);
        Assert.Equal(Messages.PhaseCreatedSuccessfully, result.Message);
    }
    [Fact]
    public async Task Should_Not_Save_File_When_PhaseFile_Is_Null()
    {
        // Arrange
        var dto = new CreatePhaseDto
        {
            Title = "Test",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 5,
            PhaseFile = null
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };
        var project = new Project
        {
            Id = 1,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 }
        };
        
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _projectRepoMock.Setup(x => x.GetProjectByIdAsync(1)).ReturnsAsync(project);

        _phaseRepoMock.Setup(x => x.GetPhasesByProjectIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Phase>());
        _phaseRepoMock.Setup(x => x.AddAsync(It.IsAny<Phase>())).Returns(Task.CompletedTask);
        
        _mapperMock.Setup(m => m.Map<Phase>(It.IsAny<CreatePhaseDto>())).Returns(new Phase());
        _mapperMock.Setup(m => m.Map<GetPhaseForInstructorDto>(It.IsAny<Phase>())).Returns(new GetPhaseForInstructorDto());

        // Act
        var result = await _service.CreatePhaseAsync(1, dto);

        // Assert
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "phases"), Times.Never);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Once);
        Assert.True(result.Success);
    }
    [Fact]
    public async Task Should_Return_Validation_Error_And_Not_Save_Or_Add_When_Invalid()
    {
        // Arrange
        var user = new User { Instructor = new Instructor { Id = 1 } };
        var project = new Project
        {
            Id = 1,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 }
        };

        var dto = new CreatePhaseDto
        {
            Title = "",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 5,
            PhaseFile = null 
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);

        _projectRepoMock.Setup(x => x.GetProjectByIdAsync(1)).ReturnsAsync(project);

        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);
        
        _phaseRepoMock.Setup(x => x.GetPhasesByProjectIdAsync(project.Id)).ReturnsAsync(new List<Phase>());
        _phaseRepoMock.Setup(x => x.AddAsync(It.IsAny<Phase>())).Returns(Task.CompletedTask);
        
        _mapperMock.Setup(m => m.Map<Phase>(It.IsAny<CreatePhaseDto>())).Returns(new Phase());
        _mapperMock.Setup(m => m.Map<GetPhaseForInstructorDto>(It.IsAny<Phase>())).Returns(new GetPhaseForInstructorDto());

        // Act
        var result = await _service.CreatePhaseAsync(1, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.TitleIsRequired, result.Message);

        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        _phaseRepoMock.Verify(r => r.AddAsync(It.IsAny<Phase>()), Times.Never);
    }
    [Fact]
    public async Task Should_Create_Phase_When_All_Valid()
    {
        var dto = new CreatePhaseDto
        {
            Title = "Valid Title",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            PhaseScore = 10
        };

        var project = new Project
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(10),
            Class = new Class { InstructorId = 1 },
            Id = 1
        };

        var user = new User { Instructor = new Instructor { Id = 1 } };

        _projectRepoMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<int>())).ReturnsAsync(project);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _phaseRepoMock.Setup(r => r.GetPhasesByProjectIdAsync(project.Id)).ReturnsAsync(new List<Phase>());
        _fileServiceMock.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "phases"))
            .ReturnsAsync("saved/file/path.pdf");

        _mapperMock.Setup(m => m.Map<Phase>(It.IsAny<CreatePhaseDto>())).Returns(new Phase());
        _mapperMock.Setup(m => m.Map<GetPhaseForInstructorDto>(It.IsAny<Phase>())).Returns(new GetPhaseForInstructorDto());

        var result = await _service.CreatePhaseAsync(1, dto);

        Assert.True(result.Success);
        Assert.Equal(Messages.PhaseCreatedSuccessfully, result.Message);
    }
}
