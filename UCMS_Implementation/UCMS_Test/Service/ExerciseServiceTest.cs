using UCMS.DTOs;
using UCMS.DTOs.ExerciseDto;
using UCMS.Models;
using UCMS.Resources;
using Xunit.Abstractions;

namespace UCMS_Test.Service;

using System.Text;
using UCMS.Repositories.ExerciseRepository.Abstraction;
using UCMS.Services.ExerciseService;
using Moq;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Services.FileService;

public class ExerciseServiceTest
{
    private readonly Mock<IExerciseRepository> _exerciseRepositoryMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextMock = new();
    private readonly Mock<IFileService> _fileServiceMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly ExerciseService _service;
    private readonly Mock<IStudentClassRepository> _studentClassRepositoryMock = new();
    private readonly Mock<IClassRepository> _classRepositoryMock = new();
    private readonly ITestOutputHelper _output;

    public ExerciseServiceTest( ITestOutputHelper output)
    {
        _httpContextMock.Setup(x => x.HttpContext!.Items).Returns(new Dictionary<object, object>()!);
        _service = new ExerciseService(
            _exerciseRepositoryMock.Object,
            _mapperMock.Object,
            _httpContextMock.Object,
            _classRepositoryMock.Object,
            _fileServiceMock.Object,
            _studentClassRepositoryMock.Object
        );
        _output = output;
    }
    [Fact]
    public async Task CreateExerciseAsync_ClassNotFound_ReturnsFailure()
    {
        _classRepositoryMock.Setup(x => x.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync((Class?)null);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"])
            .Returns(new User { Instructor = new Instructor { Id = 1 } });

        var result = await _service.CreateExerciseAsync(1, new CreateExerciseDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.ClassNotFound, result.Message);
    }

    [Fact]
    public async Task CreateExerciseAsync_InvalidInstructor_ReturnsFailure()
    {
        var @class = new Class { Id = 1, InstructorId = 99 };
        _classRepositoryMock.Setup(x => x.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(@class);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"])
            .Returns(new User { Instructor = new Instructor { Id = 1 } });

        var result = await _service.CreateExerciseAsync(1, new CreateExerciseDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.InvalidInstructorForThisClass, result.Message);
    }

    [Fact]
    public async Task CreateExerciseAsync_ExerciseStartsBeforeClassStart_ReturnsFailure()
    {
        var dto = new CreateExerciseDto { StartDate = DateTime.Today.AddDays(-5), EndDate = DateTime.Today };
        var @class = new Class
        {
            Id = 1,
            InstructorId = 1,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
        };

        _classRepositoryMock.Setup(x => x.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(@class);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"])
            .Returns(new User { Instructor = new Instructor { Id = 1 } });

        var result = await _service.CreateExerciseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseStartDateCannotBeBeforeClassStartDate, result.Message);
    }

    [Fact]
    public async Task CreateExerciseAsync_ExerciseEndsAfterClassEnd_ReturnsFailure()
    {
        var dto = new CreateExerciseDto { StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(10) };
        var @class = new Class
        {
            Id = 1,
            InstructorId = 1,
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
        };

        _classRepositoryMock.Setup(x => x.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(@class);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"])
            .Returns(new User { Instructor = new Instructor { Id = 1 } });

        var result = await _service.CreateExerciseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseEndDateCannotBeAfterClassEndDate, result.Message);
    }

    [Fact]
    public async Task CreateExerciseAsync_ExerciseAlreadyExists_ReturnsFailure()
    {
        var dto = new CreateExerciseDto { Title = "Test", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) };
        var @class = new Class { Id = 1, InstructorId = 1 };

        _classRepositoryMock.Setup(x => x.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(@class);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"])
            .Returns(new User { Instructor = new Instructor { Id = 1 } });

        _exerciseRepositoryMock.Setup(x => x.GetExercisesByClassIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Exercise> { new Exercise { Title = "  test " } });

        var result = await _service.CreateExerciseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseAlreadyExists, result.Message);
    }

    [Fact]
    public async Task CreateExerciseAsync_ValidationFails_ReturnsFailure()
    {
        var dto = new CreateExerciseDto { Title = "New Exercise", StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) };
        var @class = new Class { Id = 1, InstructorId = 1 };

        _classRepositoryMock.Setup(x => x.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(@class);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"])
            .Returns(new User { Instructor = new Instructor { Id = 1 } });

        _exerciseRepositoryMock.Setup(x => x.GetExercisesByClassIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Exercise>());

        var result = await _service.CreateExerciseAsync(1, dto);

        Assert.False(result.Success);
        Assert.NotNull(result.Message); // پیام خطا از Validator داخلی است
    }

    [Fact]
    public async Task Should_Save_File_When_ExerciseFile_Is_Provided()
    {
        var dto = new CreateExerciseDto
        {
            Title = "Valid Title",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            ExerciseScore =100, 
            ExerciseFile = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("file")), 0, 4, "Data", "file.pdf")
        };
        var currentClass = new Class
        {
            InstructorId = 1,
            Id = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };
        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);
        _classRepositoryMock.Setup(r => r.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(currentClass);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExercisesByClassIdAsync(currentClass.Id)).ReturnsAsync(new List<Exercise>());
        _fileServiceMock.Setup(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "exercises")).ReturnsAsync("path/to/file.pdf");
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<Exercise>(It.IsAny<CreateExerciseDto>())).Returns(new Exercise());
        _mapperMock.Setup(m => m.Map<GetExerciseForInstructorDto>(It.IsAny<Exercise>())).Returns(new GetExerciseForInstructorDto());
        
        var result = await _service.CreateExerciseAsync(1, dto);
        Assert.True(result.Success);
        Assert.Equal(Messages.ExerciseCreatedSuccessfully, result.Message);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "exercises"), Times.Once);
        _exerciseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Exercise>()), Times.Once);
    }

    [Fact]
    public async Task Should_Not_Save_File_When_ExerciseFile_Is_Null()
    {
        var dto = new CreateExerciseDto
        {
            Title = "Valid Title",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            ExerciseScore = 100,
            ExerciseFile = null
        };
        var currentClass = new Class
        {
            InstructorId = 1,
            Id = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _classRepositoryMock.Setup(r => r.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(currentClass);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExercisesByClassIdAsync(currentClass.Id)).ReturnsAsync(new List<Exercise>());
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<Exercise>(It.IsAny<CreateExerciseDto>())).Returns(new Exercise());
        _mapperMock.Setup(m => m.Map<GetExerciseForInstructorDto>(It.IsAny<Exercise>())).Returns(new GetExerciseForInstructorDto());
        

        var result = await _service.CreateExerciseAsync(1, dto);
        
        Assert.True(result.Success);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), "exercises"), Times.Never);
        _exerciseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Exercise>()), Times.Once);
    }

    [Fact]
    public async Task Should_Create_Exercise_When_All_Valid()
    {
        var dto = new CreateExerciseDto
        {
            Title = "Valid Title",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            ExerciseScore = 100
        };
        var currentClass = new Class
        {
            InstructorId = 1,
            Id = 1,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))
        };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _classRepositoryMock.Setup(r => r.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(currentClass);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExercisesByClassIdAsync(currentClass.Id)).ReturnsAsync(new List<Exercise>());
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<Exercise>(It.IsAny<CreateExerciseDto>())).Returns(new Exercise());
        _mapperMock.Setup(m => m.Map<GetExerciseForInstructorDto>(It.IsAny<Exercise>())).Returns(new GetExerciseForInstructorDto());

        var result = await _service.CreateExerciseAsync(1, dto);

        Assert.True(result.Success);
        Assert.Equal(Messages.ExerciseCreatedSuccessfully, result.Message);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        _exerciseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Exercise>()), Times.Once);
    }

    [Fact]
    public async Task UpdateExerciseAsync_ShouldReturnFailure_WhenExerciseNotFoundOrInvalidInstructor()
    {
        var user = new User { Instructor = new Instructor { Id = 1 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(It.IsAny<int>())).ReturnsAsync((Exercise?)null);

        var result = await _service.UpdateExerciseAsync(1, new PatchExerciseDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task UpdateExerciseAsync_ShouldReturnFailure_WhenStartDateBeforeClassStartDate()
    {
        var dto = new PatchExerciseDto { StartDate = DateTime.UtcNow.AddDays(-2) };
        var currentClass = new Class { InstructorId = 1, StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) };
        var exercise = new Exercise { Class = currentClass, ClassId = 1 };

        var user = new User { Instructor = new Instructor { Id = 1 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(It.IsAny<int>())).ReturnsAsync(exercise);

        var result = await _service.UpdateExerciseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseStartDateCannotBeBeforeClassStartDate, result.Message);
    }

    [Fact]
    public async Task UpdateExerciseAsync_ShouldReturnFailure_WhenEndDateAfterClassEndDate()
    {
        var dto = new PatchExerciseDto { EndDate = DateTime.UtcNow.AddDays(10) };
        var currentClass = new Class { InstructorId = 1, EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)) };
        var exercise = new Exercise { Class = currentClass, ClassId = 1 };

        var user = new User { Instructor = new Instructor { Id = 1 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(It.IsAny<int>())).ReturnsAsync(exercise);

        var result = await _service.UpdateExerciseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseEndDateCannotBeAfterClassEndDate, result.Message);
    }

    [Fact]
    public async Task UpdateExerciseAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var invalidDto = new PatchExerciseDto
        {
            Title = new string('a', 101), 
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(-2),
            ExerciseFile = CreateFakeFile("file.exe")
        };

        var currentClass = new Class { InstructorId = 1 };
        var exercise = new Exercise { Class = currentClass, ClassId = 1 };

        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(It.IsAny<int>())).ReturnsAsync(exercise);
        
        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(false);
        _fileServiceMock.Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);


        var validator = new UpdateExerciseDtoValidator(_fileServiceMock.Object);

        // Act
        var result = await _service.UpdateExerciseAsync(1, invalidDto);

        // Assert
        Assert.False(result.Success);
        
        var possibleMessages = new[]
        {
            Messages.TitleMaxLength,
            Messages.StartDateCanNotBeInPast,
            Messages.EndDateCanNotBeInPast,
            Messages.StartDateCanNotBeLaterThanEndDatte,
            Messages.InvalidFormat
        };

        Assert.Contains(result.Message, possibleMessages);
    }
    private IFormFile CreateFakeFile(string fileName)
    {
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write("Fake file content");
        writer.Flush();
        ms.Position = 0;
        return new FormFile(ms, 0, ms.Length, "file", fileName);
    }
    [Fact]
    public async Task UpdateExerciseAsync_ShouldReturnFailure_WhenTitleAlreadyExists()
    {
        var dto = new PatchExerciseDto { Title = "Duplicate Title" };
        var currentClass = new Class { InstructorId = 1, Id = 1 };
        var exercise = new Exercise { Class = currentClass, ClassId = 1 };

        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(It.IsAny<int>())).ReturnsAsync(exercise);
        _exerciseRepositoryMock.Setup(r => r.ExistsWithTitleExceptIdAsync(dto.Title!, currentClass.Id, It.IsAny<int>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateExerciseAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseAlreadyExists, result.Message);
    }

    [Fact]
    public async Task UpdateExerciseAsync_ShouldDeleteOldFileAndSaveNewFile_WhenExerciseFileProvided()
    {
        var dto = new PatchExerciseDto { ExerciseFile = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("file")), 0, 4, "Data", "file.pdf") };
        var currentClass = new Class { InstructorId = 1 };
        var exercise = new Exercise { Class = currentClass, ExerciseFilePath = "old/path/file.pdf", ClassId = 1 };

        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(It.IsAny<int>())).ReturnsAsync(exercise);
        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.SaveFileAsync(dto.ExerciseFile!, "exercises")).ReturnsAsync("new/path/file.pdf");
        _exerciseRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Exercise>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<GetExerciseForInstructorDto>(It.IsAny<Exercise>())).Returns(new GetExerciseForInstructorDto());

        var result = await _service.UpdateExerciseAsync(1, dto);
        Assert.True(result.Success);
        _fileServiceMock.Verify(f => f.DeleteFile("old/path/file.pdf"), Times.Once);
        _fileServiceMock.Verify(f => f.SaveFileAsync(dto.ExerciseFile!, "exercises"), Times.Once);
        _exerciseRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Exercise>()), Times.Once);
    }

    [Fact]
    public async Task UpdateExerciseAsync_ShouldSaveFile_WhenExerciseFileProvidedAndNoOldFile()
    {
        var dto = new PatchExerciseDto { ExerciseFile = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("file")), 0, 4, "Data", "file.pdf") };
        var currentClass = new Class { InstructorId = 1 };
        var exercise = new Exercise { Class = currentClass, ExerciseFilePath = null, ClassId = 1 };

        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(It.IsAny<int>())).ReturnsAsync(exercise);
        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);

        _fileServiceMock.Setup(f => f.SaveFileAsync(dto.ExerciseFile!, "exercises")).ReturnsAsync("new/path/file.pdf");
        _exerciseRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Exercise>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<GetExerciseForInstructorDto>(It.IsAny<Exercise>())).Returns(new GetExerciseForInstructorDto());

        var result = await _service.UpdateExerciseAsync(1, dto);

        Assert.True(result.Success);
        _fileServiceMock.Verify(f => f.DeleteFile(It.IsAny<string>()), Times.Never);
        _fileServiceMock.Verify(f => f.SaveFileAsync(dto.ExerciseFile!, "exercises"), Times.Once);
        _exerciseRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Exercise>()), Times.Once);
    }

    [Fact]
    public async Task UpdateExerciseAsync_ShouldUpdateExerciseSuccessfully_WhenNoFileProvided()
    {
        var dto = new PatchExerciseDto { Title = "Updated Title" };
        var currentClass = new Class { InstructorId = 1 };
        var exercise = new Exercise { Class = currentClass, ClassId = 1, ExerciseFilePath = null };

        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(It.IsAny<int>())).ReturnsAsync(exercise);
        _exerciseRepositoryMock.Setup(r => r.ExistsWithTitleExceptIdAsync(dto.Title!, exercise.ClassId, It.IsAny<int>())).ReturnsAsync(false);
        _exerciseRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Exercise>())).Returns(Task.CompletedTask);
        _fileServiceMock.Setup(f => f.IsValidExtension(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock.Setup(f => f.IsValidFileSize(It.IsAny<IFormFile>())).Returns(true);

        _mapperMock.Setup(m => m.Map<GetExerciseForInstructorDto>(It.IsAny<Exercise>())).Returns(new GetExerciseForInstructorDto());

        var result = await _service.UpdateExerciseAsync(1, dto);

        Assert.True(result.Success);
        _fileServiceMock.Verify(f => f.DeleteFile(It.IsAny<string>()), Times.Never);
        _fileServiceMock.Verify(f => f.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        _exerciseRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Exercise>()), Times.Once);
    }

    [Fact]
    public async Task GetExerciseByIdForInstructorAsync_Should_Return_Failure_When_Exercise_NotFound()
    {
        // Arrange
        int exerciseId = 1;
        var user = new User { Instructor = new Instructor { Id = 5 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync((Exercise)null);

        // Act
        var result = await _service.GetExerciseByIdForInstructorAsync(exerciseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseCantBeAccessed, result.Message);
    }
    
    [Fact]
    public async Task GetExerciseByIdForInstructorAsync_Should_Return_Failure_When_Instructor_IsNotOwner()
    {
        // Arrange
        int exerciseId = 1;
        var user = new User { Instructor = new Instructor { Id = 5 } };
        var exercise = new Exercise
        {
            Id = exerciseId,
            Class = new Class { InstructorId = 999 } // Instructor دیگر
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync(exercise);

        // Act
        var result = await _service.GetExerciseByIdForInstructorAsync(exerciseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseCantBeAccessed, result.Message);
    }
    
    [Fact]
    public async Task GetExerciseByIdForInstructorAsync_Should_Return_Success_When_Exercise_Valid_And_Owned()
    {
        // Arrange
        int exerciseId = 1;
        var user = new User { Instructor = new Instructor { Id = 5 } };
        var exercise = new Exercise
        {
            Id = exerciseId,
            Class = new Class { InstructorId = 5 }
        };
        var expectedDto = new GetExerciseForInstructorDto();

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync(exercise);
        _mapperMock.Setup(m => m.Map<GetExerciseForInstructorDto>(exercise)).Returns(expectedDto);

        // Act
        var result = await _service.GetExerciseByIdForInstructorAsync(exerciseId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExerciseRetrievedSuccessfully, result.Message);
        Assert.Equal(expectedDto, result.Data);
    }

    [Fact]
    public async Task DeleteExerciseAsync_Should_Return_Failure_When_Exercise_Not_Found()
    {
        // Arrange
        int exerciseId = 1;
        var user = new User { Instructor = new Instructor { Id = 10 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync((Exercise)null);

        // Act
        var result = await _service.DeleteExerciseAsync(exerciseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task DeleteExerciseAsync_Should_Return_Failure_When_Instructor_Is_Not_Owner()
    {
        // Arrange
        int exerciseId = 1;
        var user = new User { Instructor = new Instructor { Id = 10 } };
        var exercise = new Exercise { Id = exerciseId, Class = new Class { InstructorId = 99 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync(exercise);

        // Act
        var result = await _service.DeleteExerciseAsync(exerciseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task DeleteExerciseAsync_Should_Delete_Successfully_When_Valid_And_No_File()
    {
        // Arrange
        int exerciseId = 1;
        var user = new User { Instructor = new Instructor { Id = 10 } };
        var exercise = new Exercise { Id = exerciseId, Class = new Class { InstructorId = 10 }, ExerciseFilePath = null };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync(exercise);
        _exerciseRepositoryMock.Setup(r => r.DeleteAsync(exercise)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteExerciseAsync(exerciseId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExerciseDeletedSuccessfully, result.Message);
        _fileServiceMock.Verify(f => f.DeleteFile(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteExerciseAsync_Should_Delete_Successfully_With_File()
    {
        // Arrange
        int exerciseId = 1;
        var user = new User { Instructor = new Instructor { Id = 10 } };
        var exercise = new Exercise
        {
            Id = exerciseId,
            Class = new Class { InstructorId = 10 },
            ExerciseFilePath = "files/exercise1.pdf"
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync(exercise);
        _exerciseRepositoryMock.Setup(r => r.DeleteAsync(exercise)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteExerciseAsync(exerciseId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExerciseDeletedSuccessfully, result.Message);
        _fileServiceMock.Verify(f => f.DeleteFile("files/exercise1.pdf"), Times.Once);
    }

    [Fact]
    public async Task GetExercisesOfClassForInstructor_Should_Return_Failure_When_Class_Not_Found()
    {
        // Arrange
        int classId = 1;
        var user = new User { Instructor = new Instructor { Id = 10 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepositoryMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync((Class)null);

        // Act
        var result = await _service.GetExercisesOfClassForInstructor(classId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetExercisesOfClassForInstructor_Should_Return_Failure_When_Instructor_Not_Owner()
    {
        // Arrange
        int classId = 1;
        var user = new User { Instructor = new Instructor { Id = 10 } };
        var classEntity = new Class { Id = classId, InstructorId = 99 };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepositoryMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(classEntity);

        // Act
        var result = await _service.GetExercisesOfClassForInstructor(classId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetExercisesOfClassForInstructor_Should_Return_Success_When_Valid()
    {
        // Arrange
        int classId = 1;
        var user = new User { Instructor = new Instructor { Id = 10 } };
        var classEntity = new Class { Id = classId, InstructorId = 10 };
        var exercises = new List<Exercise>
        {
            new Exercise { Id = 1, Title = "Exercise 1" },
            new Exercise { Id = 2, Title = "Exercise 2" }
        };
        var expectedDtos = new List<GetExercisesForInstructorDto>
        {
            new GetExercisesForInstructorDto { exerciseId = 1, Title = "Exercise 1" },
            new GetExercisesForInstructorDto { exerciseId = 2, Title = "Exercise 2" }
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepositoryMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(classEntity);
        _exerciseRepositoryMock.Setup(r => r.GetExercisesByClassIdAsync(classId)).ReturnsAsync(exercises);
        _mapperMock.Setup(m => m.Map<List<GetExercisesForInstructorDto>>(exercises)).Returns(expectedDtos);

        // Act
        var result = await _service.GetExercisesOfClassForInstructor(classId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExercisesRetrievedSuccessfully, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal("Exercise 1", result.Data[0].Title);
    }
    
    [Fact]
    public async Task HandleDownloadExerciseFileForInstructorAsync_Should_Return_Failure_When_Exercise_Not_Found()
    {
        // Arrange
        int exerciseId = 1;
        var user = new User { Instructor = new Instructor { Id = 10 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync((Exercise)null);

        // Act
        var result = await _service.HandleDownloadExerciseFileForInstructorAsync(exerciseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task HandleDownloadExerciseFileForInstructorAsync_Should_Return_Failure_When_Instructor_Not_Owner()
    {
        // Arrange
        int exerciseId = 1;
        var exercise = new Exercise
        {
            Id = exerciseId,
            Class = new Class { InstructorId = 99 },
            ExerciseFilePath = "somefile.pdf"
        };
        var user = new User { Instructor = new Instructor { Id = 10 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync(exercise);

        // Act
        var result = await _service.HandleDownloadExerciseFileForInstructorAsync(exerciseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task HandleDownloadExerciseFileForInstructorAsync_Should_Return_Failure_When_FilePath_Is_Empty()
    {
        // Arrange
        int exerciseId = 1;
        var exercise = new Exercise
        {
            Id = exerciseId,
            Class = new Class { InstructorId = 10 },
            ExerciseFilePath = ""
        };
        var user = new User { Instructor = new Instructor { Id = 10 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync(exercise);

        // Act
        var result = await _service.HandleDownloadExerciseFileForInstructorAsync(exerciseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.FileNotFound, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task HandleDownloadExerciseFileForInstructorAsync_Should_Return_Failure_When_File_Does_Not_Exist()
    {
        // Arrange
        int exerciseId = 1;
        var filePath = "path/to/file.pdf";
        var exercise = new Exercise
        {
            Id = exerciseId,
            Class = new Class { InstructorId = 10 },
            ExerciseFilePath = filePath
        };
        var user = new User { Instructor = new Instructor { Id = 10 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync(exercise);
        _fileServiceMock.Setup(f => f.DownloadFile(filePath)).ReturnsAsync((FileDownloadDto)null);

        // Act
        var result = await _service.HandleDownloadExerciseFileForInstructorAsync(exerciseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.FileDoesNotExist, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task HandleDownloadExerciseFileForInstructorAsync_Should_Return_Success_When_All_Valid()
    {
        // Arrange
        int exerciseId = 1;
        var filePath = "path/to/file.pdf";
        var exercise = new Exercise
        {
            Id = exerciseId,
            Class = new Class { InstructorId = 10 },
            ExerciseFilePath = filePath
        };
        var user = new User { Instructor = new Instructor { Id = 10 } };
        var fileDto = new FileDownloadDto { FileName = "file.pdf", FileBytes = new byte[] { 1, 2, 3 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync(exercise);
        _fileServiceMock.Setup(f => f.DownloadFile(filePath)).ReturnsAsync(fileDto);

        // Act
        var result = await _service.HandleDownloadExerciseFileForInstructorAsync(exerciseId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExerciseFileDownloadedSuccessfully, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal("application/pdf", result.Data.ContentType); // فرض بر این است که متد GetContentTypeFromPath همین مقدار را برمی‌گرداند
    }

    [Fact]
    public async Task GetExercisesOfClassForStudent_Should_Return_Failure_When_Class_Not_Found()
    {
        // Arrange
        int classId = 1;
        var user = new User { Student = new Student { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepositoryMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync((Class)null);

        // Act
        var result = await _service.GetExercisesOfClassForStudent(classId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetExercisesOfClassForStudent_Should_Return_Failure_When_Student_Not_In_Class()
    {
        // Arrange
        int classId = 1;
        var currentClass = new Class { Id = classId };
        var user = new User { Student = new Student { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepositoryMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(currentClass);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(classId, user.Student.Id)).ReturnsAsync(false);

        // Act
        var result = await _service.GetExercisesOfClassForStudent(classId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetExercisesOfClassForStudent_Should_Return_Success_When_Student_In_Class()
    {
        // Arrange
        int classId = 1;
        var currentClass = new Class { Id = classId };
        var exercises = new List<Exercise>
        {
            new Exercise { Id = 1, ClassId = classId },
            new Exercise { Id = 2, ClassId = classId }
        };
        var user = new User { Student = new Student { Id = 1 } };
        var expectedDtos = new List<GetExercisesForStudentDto>
        {
            new GetExercisesForStudentDto { exerciseId = 1 },
            new GetExercisesForStudentDto { exerciseId = 2 }
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepositoryMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(currentClass);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(classId, user.Student.Id)).ReturnsAsync(true);
        _exerciseRepositoryMock.Setup(r => r.GetExercisesByClassIdAsync(classId)).ReturnsAsync(exercises);
        _mapperMock.Setup(m => m.Map<List<GetExercisesForStudentDto>>(exercises)).Returns(expectedDtos);

        // Act
        var result = await _service.GetExercisesOfClassForStudent(classId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExercisesRetrievedSuccessfully, result.Message);
        Assert.Equal(expectedDtos, result.Data);
    }

    [Fact]
    public async Task HandleDownloadExerciseFileForStudentAsync_Should_Return_Failure_When_Exercise_Not_Found()
    {
        // Arrange
        int exerciseId = 1;
        var user = new User { Student = new Student { Id = 1 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync((Exercise)null);

        // Act
        var result = await _service.HandleDownloadExerciseFileForStudentAsync(exerciseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task HandleDownloadExerciseFileForStudentAsync_Should_Return_Failure_When_Student_Not_Authorized()
    {
        // Arrange
        int exerciseId = 1;
        var exercise = new Exercise { Id = exerciseId, ClassId = 2, ExerciseFilePath = "path/file.pdf" };
        var user = new User { Student = new Student { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync(exercise);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(exercise.ClassId, user.Student.Id)).ReturnsAsync(false);

        // Act
        var result = await _service.HandleDownloadExerciseFileForStudentAsync(exerciseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExerciseCantBeAccessed, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task HandleDownloadExerciseFileForStudentAsync_Should_Return_Failure_When_FilePath_Is_NullOrWhitespace()
    {
        // Arrange
        int exerciseId = 1;
        var exercise = new Exercise { Id = exerciseId, ClassId = 2, ExerciseFilePath = " " };
        var user = new User { Student = new Student { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync(exercise);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(exercise.ClassId, user.Student.Id)).ReturnsAsync(true);

        // Act
        var result = await _service.HandleDownloadExerciseFileForStudentAsync(exerciseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.FileNotFound, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task HandleDownloadExerciseFileForStudentAsync_Should_Return_Failure_When_File_Does_Not_Exist()
    {
        // Arrange
        int exerciseId = 1;
        var exercise = new Exercise { Id = exerciseId, ClassId = 2, ExerciseFilePath = "path/file.pdf" };
        var user = new User { Student = new Student { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync(exercise);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(exercise.ClassId, user.Student.Id)).ReturnsAsync(true);
        _fileServiceMock.Setup(f => f.DownloadFile(exercise.ExerciseFilePath)).ReturnsAsync((FileDownloadDto)null);

        // Act
        var result = await _service.HandleDownloadExerciseFileForStudentAsync(exerciseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.FileDoesNotExist, result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task HandleDownloadExerciseFileForStudentAsync_Should_Return_Success_When_File_Exists()
    {
        // Arrange
        int exerciseId = 1;
        var exercise = new Exercise { Id = exerciseId, ClassId = 2, ExerciseFilePath = "path/file.pdf" };
        var user = new User { Student = new Student { Id = 1 } };
        var fileDownloadDto = new FileDownloadDto { FileName = "file.pdf", FileBytes = new byte[] { 1, 2, 3 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExerciseByIdAsync(exerciseId)).ReturnsAsync(exercise);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(exercise.ClassId, user.Student.Id)).ReturnsAsync(true);
        _fileServiceMock.Setup(f => f.DownloadFile(exercise.ExerciseFilePath)).ReturnsAsync(fileDownloadDto);

        // Act
        var result = await _service.HandleDownloadExerciseFileForStudentAsync(exerciseId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExerciseFileDownloadedSuccessfully, result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(fileDownloadDto, result.Data);
        Assert.NotNull(result.Data.ContentType);
    }

    [Fact]
    public async Task GetExercisesForInstructor_Should_Return_Success_With_Exercise_List()
    {
        // Arrange
        var user = new User { Instructor = new Instructor { Id = 1 } };
        var exercises = new List<Exercise>
        {
            new Exercise { Id = 1, Title = "Exercise 1" },
            new Exercise { Id = 2, Title = "Exercise 2" }
        };
        var expectedDtos = new List<GetExercisesForInstructorDto>
        {
            new GetExercisesForInstructorDto { exerciseId = 1, Title = "Exercise 1" },
            new GetExercisesForInstructorDto { exerciseId = 2, Title = "Exercise 2" }
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExercisesByInstructorIdAsync(user.Instructor.Id)).ReturnsAsync(exercises);
        _mapperMock.Setup(m => m.Map<List<GetExercisesForInstructorDto>>(exercises)).Returns(expectedDtos);

        // Act
        var result = await _service.GetExercisesForInstructor();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExercisesRetrievedSuccessfully, result.Message);
        Assert.Equal(expectedDtos, result.Data);
    }

    [Fact]
    public async Task GetExercisesForStudent_Should_Return_Success_With_Exercise_List()
    {
        // Arrange
        var user = new User { Student = new Student { Id = 1 } };
        var exercises = new List<Exercise>
        {
            new Exercise { Id = 1, Title = "Exercise 1" },
            new Exercise { Id = 2, Title = "Exercise 2" }
        };
        var expectedDtos = new List<GetExercisesForStudentDto>
        {
            new GetExercisesForStudentDto { exerciseId = 1, Title = "Exercise 1" },
            new GetExercisesForStudentDto { exerciseId = 2, Title = "Exercise 2" }
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _exerciseRepositoryMock.Setup(r => r.GetExercisesByStudentIdAsync(user.Student.Id)).ReturnsAsync(exercises);
        _mapperMock.Setup(m => m.Map<List<GetExercisesForStudentDto>>(exercises)).Returns(expectedDtos);

        // Act
        var result = await _service.GetExercisesForStudent();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExercisesRetrievedSuccessfully, result.Message);
        Assert.Equal(expectedDtos, result.Data);
    }

}