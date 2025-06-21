using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using UCMS.DTOs.ExamDto;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.ExamRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.ExamService;
using Xunit.Abstractions;

namespace UCMS_Test.Service;

public class ExamServiceTest
{
    private readonly Mock<IExamRepository> _examRepositoryMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly ExamService _service;
    private readonly Mock<IStudentClassRepository> _studentClassRepositoryMock = new();
    private readonly Mock<IClassRepository> _classRepositoryMock = new();
    private readonly ITestOutputHelper _output;

    public ExamServiceTest( ITestOutputHelper output)
    {
        _httpContextMock.Setup(x => x.HttpContext!.Items).Returns(new Dictionary<object, object>()!);
        _service = new ExamService(
            _examRepositoryMock.Object,
            _mapperMock.Object,
            _httpContextMock.Object,
            _classRepositoryMock.Object,
            _studentClassRepositoryMock.Object
        );
        _output = output;
    }
    [Fact]
    public async Task CreateExamAsync_ClassNotFound_ReturnsFailure()
    {
        _classRepositoryMock.Setup(x => x.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync((Class?)null);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"])
            .Returns(new User { Instructor = new Instructor { Id = 1 } });

        var result = await _service.CreateExamAsync(1, new CreateExamDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.ClassNotFound, result.Message);
    }

    [Fact]
    public async Task CreateExamAsync_InvalidInstructor_ReturnsFailure()
    {
        var @class = new Class { Id = 1, InstructorId = 99 };
        _classRepositoryMock.Setup(x => x.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(@class);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"])
            .Returns(new User { Instructor = new Instructor { Id = 1 } });

        var result = await _service.CreateExamAsync(1, new CreateExamDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.InvalidInstructorForThisClass, result.Message);
    }

    [Fact]
    public async Task CreateExamAsync_ExamDateAfterClassEnd_ReturnsFailure()
    {
        var dto = new CreateExamDto { Date = DateTime.Today.AddDays(10) };
        var @class = new Class
        {
            Id = 1,
            InstructorId = 1,
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5))
        };

        _classRepositoryMock.Setup(x => x.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(@class);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"])
            .Returns(new User { Instructor = new Instructor { Id = 1 } });

        var result = await _service.CreateExamAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.ExamEndDateCannotBeAfterClassEndDate, result.Message);
    }

    [Fact]
    public async Task CreateExamAsync_ExamAlreadyExists_ReturnsFailure()
    {
        var dto = new CreateExamDto { Title = "Test Exam", Date = DateTime.Today };
        var @class = new Class { Id = 1, InstructorId = 1 };

        _classRepositoryMock.Setup(x => x.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(@class);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"])
            .Returns(new User { Instructor = new Instructor { Id = 1 } });

        _examRepositoryMock.Setup(x => x.GetExamsByClassIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Exam> { new Exam { Title = " test exam " } });

        var result = await _service.CreateExamAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.ExamAlreadyExists, result.Message);
    }

    [Fact]
    public async Task CreateExamAsync_ValidationFails_ReturnsFailure()
    {
        var dto = new CreateExamDto { Title = "", Date = DateTime.Today };
        var @class = new Class { Id = 1, InstructorId = 1 };

        _classRepositoryMock.Setup(x => x.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(@class);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"])
            .Returns(new User { Instructor = new Instructor { Id = 1 } });

        _examRepositoryMock.Setup(x => x.GetExamsByClassIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Exam>());

        var result = await _service.CreateExamAsync(1, dto);

        Assert.False(result.Success);
        Assert.NotNull(result.Message);
    }

    [Fact]
    public async Task CreateExamAsync_Success_ReturnsSuccess()
    {
        var dto = new CreateExamDto { Title = "Valid Exam", Date = DateTime.Today.AddDays(1), ExamScore = 100};
        var @class = new Class
        {
            Id = 1,
            InstructorId = 1,
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5))
        };

        _classRepositoryMock.Setup(x => x.GetClassByIdAsync(It.IsAny<int>())).ReturnsAsync(@class);
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"])
            .Returns(new User { Instructor = new Instructor { Id = 1 } });

        _examRepositoryMock.Setup(x => x.GetExamsByClassIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Exam>());
        _examRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Exam>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<Exam>(dto)).Returns(new Exam());
        _mapperMock.Setup(m => m.Map<GetExamForInstructorDto>(It.IsAny<Exam>())).Returns(new GetExamForInstructorDto());

        var result = await _service.CreateExamAsync(1, dto);

        Assert.True(result.Success);
        Assert.Equal(Messages.ExamCreatedSuccessfully, result.Message);
        _examRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Exam>()), Times.Once);
    }

    [Fact]
    public async Task UpdateExamAsync_ShouldReturnFailure_WhenExamNotFoundOrInvalidInstructor()
    {
        var user = new User { Instructor = new Instructor { Id = 1 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamByIdAsync(It.IsAny<int>())).ReturnsAsync((Exam?)null);

        var result = await _service.UpdateExamAsync(1, new PatchExamDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.ExamCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task UpdateExamAsync_ShouldReturnFailure_WhenExamNotBelongToUserInstructor()
    {
        var user = new User { Instructor = new Instructor { Id = 1 } };
        var exam = new Exam { Class = new Class { InstructorId = 2 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamByIdAsync(It.IsAny<int>())).ReturnsAsync(exam);

        var result = await _service.UpdateExamAsync(1, new PatchExamDto());

        Assert.False(result.Success);
        Assert.Equal(Messages.ExamCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task UpdateExamAsync_ShouldReturnFailure_WhenDateIsAfterClassEndDate()
    {
        var dto = new PatchExamDto { Date = DateTime.UtcNow.AddDays(10) };
        var currentClass = new Class { InstructorId = 1, EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)) };
        var exam = new Exam { Class = currentClass, ClassId = 1 };

        var user = new User { Instructor = new Instructor { Id = 1 } };
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamByIdAsync(It.IsAny<int>())).ReturnsAsync(exam);

        var result = await _service.UpdateExamAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.ExamEndDateCannotBeAfterClassEndDate, result.Message);
    }

    [Fact]
    public async Task UpdateExamAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var invalidDto = new PatchExamDto
        {
            Title = new string('a', 101), 
            ExamLocation = new string('b', 600), 
            ExamScore = -5, 
            Date = DateTime.UtcNow.AddDays(-1) 
        };

        var currentClass = new Class { InstructorId = 1 };
        var exam = new Exam { Class = currentClass, ClassId = 1 };
        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamByIdAsync(It.IsAny<int>())).ReturnsAsync(exam);

        // Act
        var result = await _service.UpdateExamAsync(1, invalidDto);

        // Assert
        Assert.False(result.Success);
        
        var expectedMessages = new[]
        {
            Messages.TitleMaxLength,
            Messages.DescriptionMaxLength,
            Messages.ExamScoreMustBePositive,
            Messages.DateCanNotBeInPast
        };

        Assert.Contains(result.Message, expectedMessages);
    }
    
    [Fact]
    public async Task UpdateExamAsync_ShouldReturnFailure_WhenTitleAlreadyExists()
    {
        var dto = new PatchExamDto { Title = "Duplicate Title" };
        var currentClass = new Class { InstructorId = 1, Id = 1 };
        var exam = new Exam { Class = currentClass, ClassId = 1 };

        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamByIdAsync(It.IsAny<int>())).ReturnsAsync(exam);
        _examRepositoryMock.Setup(r => r.ExistsWithTitleExceptIdAsync(dto.Title!, currentClass.Id, It.IsAny<int>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateExamAsync(1, dto);

        Assert.False(result.Success);
        Assert.Equal(Messages.ExamAlreadyExists, result.Message);
    }

    [Fact]
    public async Task UpdateExamAsync_ShouldUpdateExamSuccessfully()
    {
        var dto = new PatchExamDto { Title = "Updated Title", Date = DateTime.UtcNow.AddDays(1) };
        var currentClass = new Class { InstructorId = 1, Id = 1 };
        var exam = new Exam { Class = currentClass, ClassId = 1 };

        var user = new User { Instructor = new Instructor { Id = 1 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamByIdAsync(It.IsAny<int>())).ReturnsAsync(exam);
        _examRepositoryMock.Setup(r => r.ExistsWithTitleExceptIdAsync(dto.Title!, currentClass.Id, It.IsAny<int>())).ReturnsAsync(false);
        _examRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Exam>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map(dto, exam)).Returns(exam);
        _mapperMock.Setup(m => m.Map<GetExamForInstructorDto>(It.IsAny<Exam>())).Returns(new GetExamForInstructorDto());

        var result = await _service.UpdateExamAsync(1, dto);

        Assert.True(result.Success);
        Assert.Equal(Messages.ExamUpdatedSuccessfully, result.Message);
        _examRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Exam>()), Times.Once);
    }
    
    [Fact]
    public async Task GetExamByIdForInstructorAsync_Should_Return_Failure_When_Exam_NotFound()
    {
        // Arrange
        int examId = 1;
        var user = new User { Instructor = new Instructor { Id = 5 } };
        
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamByIdAsync(examId)).ReturnsAsync((Exam)null);

        // Act
        var result = await _service.GetExamByIdForInstructorAsync(examId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExamCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task GetExamByIdForInstructorAsync_Should_Return_Failure_When_Instructor_IsNotOwner()
    {
        // Arrange
        int examId = 1;
        var user = new User { Instructor = new Instructor { Id = 5 } };
        var exam = new Exam
        {
            Id = examId,
            Class = new Class { InstructorId = 999 } // Instructor دیگر
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamByIdAsync(examId)).ReturnsAsync(exam);

        // Act
        var result = await _service.GetExamByIdForInstructorAsync(examId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExamCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task GetExamByIdForInstructorAsync_Should_Return_Success_When_Exam_Valid_And_Owned()
    {
        // Arrange
        int examId = 1;
        var user = new User { Instructor = new Instructor { Id = 5 } };
        var exam = new Exam
        {
            Id = examId,
            Class = new Class { InstructorId = 5 }
        };
        var expectedDto = new GetExamForInstructorDto();

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamByIdAsync(examId)).ReturnsAsync(exam);
        _mapperMock.Setup(m => m.Map<GetExamForInstructorDto>(exam)).Returns(expectedDto);

        // Act
        var result = await _service.GetExamByIdForInstructorAsync(examId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExamRetrievedSuccessfully, result.Message);
        Assert.Equal(expectedDto, result.Data);
    }
    
    [Fact]
    public async Task DeleteExamAsync_Should_Return_Failure_When_Exam_Not_Found()
    {
        // Arrange
        int examId = 1;
        var user = new User { Instructor = new Instructor { Id = 10 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamByIdAsync(examId)).ReturnsAsync((Exam)null);

        // Act
        var result = await _service.DeleteExamAsync(examId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExamCantBeAccessed, result.Message);
    }
    
    [Fact]
    public async Task DeleteExamAsync_Should_Return_Failure_When_Instructor_Is_Not_Owner()
    {
        // Arrange
        int examId = 1;
        var user = new User { Instructor = new Instructor { Id = 10 } };
        var exam = new Exam { Id = examId, Class = new Class { InstructorId = 99 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamByIdAsync(examId)).ReturnsAsync(exam);

        // Act
        var result = await _service.DeleteExamAsync(examId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExamCantBeAccessed, result.Message);
    }
    
    [Fact]
    public async Task DeleteExamAsync_Should_Delete_Successfully_When_Valid()
    {
        // Arrange
        int examId = 1;
        var user = new User { Instructor = new Instructor { Id = 10 } };
        var exam = new Exam { Id = examId, Class = new Class { InstructorId = 10 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamByIdAsync(examId)).ReturnsAsync(exam);
        _examRepositoryMock.Setup(r => r.DeleteAsync(exam)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteExamAsync(examId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExamDeletedSuccessfully, result.Message);
        Assert.Equal("Exam deleted successfully", result.Data);
        _examRepositoryMock.Verify(r => r.DeleteAsync(exam), Times.Once);
    }
    
    [Fact]
    public async Task GetExamsOfClassForInstructor_Should_Return_Failure_When_Class_Not_Found()
    {
        // Arrange
        int classId = 1;
        var user = new User { Instructor = new Instructor { Id = 5 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepositoryMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync((Class)null);

        // Act
        var result = await _service.GetExamsOfClassForInstructor(classId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExamCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task GetExamsOfClassForInstructor_Should_Return_Failure_When_User_Is_Not_Owner_Of_Class()
    {
        // Arrange
        int classId = 1;
        var user = new User { Instructor = new Instructor { Id = 5 } };
        var currentClass = new Class { Id = classId, InstructorId = 999 }; // دیگران

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepositoryMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(currentClass);

        // Act
        var result = await _service.GetExamsOfClassForInstructor(classId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExamCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task GetExamsOfClassForInstructor_Should_Return_Success_When_Class_Is_Valid_And_Owned()
    {
        // Arrange
        int classId = 1;
        var user = new User { Instructor = new Instructor { Id = 5 } };
        var currentClass = new Class { Id = classId, InstructorId = 5 };

        var exams = new List<Exam>
        {
            new Exam { Id = 1 },
            new Exam { Id = 2 }
        };

        var expectedDtos = new List<GetExamForInstructorDto>
        {
            new GetExamForInstructorDto(),
            new GetExamForInstructorDto()
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepositoryMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(currentClass);
        _examRepositoryMock.Setup(r => r.GetExamsByClassIdAsync(classId)).ReturnsAsync(exams);
        _mapperMock.Setup(m => m.Map<List<GetExamForInstructorDto>>(exams)).Returns(expectedDtos);

        // Act
        var result = await _service.GetExamsOfClassForInstructor(classId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExamsRetrievedSuccessfully, result.Message);
        Assert.Equal(expectedDtos, result.Data);
    }
    
    [Fact]
    public async Task GetExamByIdForStudentAsync_Should_Return_Failure_When_Exam_Not_Found()
    {
        // Arrange
        int examId = 1;
        var user = new User { Student = new Student { Id = 10 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamByIdAsync(examId)).ReturnsAsync((Exam)null);

        // Act
        var result = await _service.GetExamByIdForStudentAsync(examId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExamCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task GetExamByIdForStudentAsync_Should_Return_Failure_When_Student_Is_Not_In_Class()
    {
        // Arrange
        int examId = 1;
        var user = new User { Student = new Student { Id = 10 } };
        var exam = new Exam { Id = examId, ClassId = 100 };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamByIdAsync(examId)).ReturnsAsync(exam);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(exam.ClassId, user.Student.Id))
            .ReturnsAsync(false);

        // Act
        var result = await _service.GetExamByIdForStudentAsync(examId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExamCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task GetExamByIdForStudentAsync_Should_Return_Success_When_Student_Is_In_Class_And_Exam_Exists()
    {
        // Arrange
        int examId = 1;
        var user = new User { Student = new Student { Id = 10 } };
        var exam = new Exam { Id = examId, ClassId = 100 };
        var expectedDto = new GetExamForStudentDto();

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamByIdAsync(examId)).ReturnsAsync(exam);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(exam.ClassId, user.Student.Id))
            .ReturnsAsync(true);
        _mapperMock.Setup(m => m.Map<GetExamForStudentDto>(exam)).Returns(expectedDto);

        // Act
        var result = await _service.GetExamByIdForStudentAsync(examId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExamRetrievedSuccessfully, result.Message);
        Assert.Equal(expectedDto, result.Data);
    }

    [Fact]
    public async Task GetExamsOfClassForStudent_Should_Return_Failure_When_Class_Not_Found()
    {
        // Arrange
        int classId = 1;
        var user = new User { Student = new Student { Id = 10 } };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepositoryMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync((Class)null);

        // Act
        var result = await _service.GetExamsOfClassForStudent(classId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExamCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task GetExamsOfClassForStudent_Should_Return_Failure_When_Student_Not_In_Class()
    {
        // Arrange
        int classId = 1;
        var user = new User { Student = new Student { Id = 10 } };
        var currentClass = new Class { Id = classId };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepositoryMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(currentClass);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(classId, user.Student.Id)).ReturnsAsync(false);

        // Act
        var result = await _service.GetExamsOfClassForStudent(classId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(Messages.ExamCantBeAccessed, result.Message);
    }

    [Fact]
    public async Task GetExamsOfClassForStudent_Should_Return_Success_When_Student_Is_In_Class()
    {
        // Arrange
        int classId = 1;
        var user = new User { Student = new Student { Id = 10 } };
        var currentClass = new Class { Id = classId };
        var exams = new List<Exam> { new Exam { Id = 1 }, new Exam { Id = 2 } };
        var expectedDto = new List<GetExamForStudentDto> { new GetExamForStudentDto(), new GetExamForStudentDto() };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _classRepositoryMock.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(currentClass);
        _studentClassRepositoryMock.Setup(r => r.IsStudentOfClassAsync(classId, user.Student.Id)).ReturnsAsync(true);
        _examRepositoryMock.Setup(r => r.GetExamsByClassIdAsync(classId)).ReturnsAsync(exams);
        _mapperMock.Setup(m => m.Map<List<GetExamForStudentDto>>(exams)).Returns(expectedDto);

        // Act
        var result = await _service.GetExamsOfClassForStudent(classId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExamsRetrievedSuccessfully, result.Message);
        Assert.Equal(expectedDto, result.Data);
    }

    [Fact]
    public async Task GetExamsForInstructor_Should_Return_Success_When_Exams_Exist()
    {
        // Arrange
        var user = new User { Instructor = new Instructor { Id = 5 } };
        var exams = new List<Exam>
        {
            new Exam { Id = 1 },
            new Exam { Id = 2 }
        };
        var expectedDtos = new List<GetExamForInstructorDto>
        {
            new GetExamForInstructorDto(),
            new GetExamForInstructorDto()
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamsByInstructorIdAsync(user.Instructor.Id)).ReturnsAsync(exams);
        _mapperMock.Setup(m => m.Map<List<GetExamForInstructorDto>>(exams)).Returns(expectedDtos);

        // Act
        var result = await _service.GetExamsForInstructor();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExamsRetrievedSuccessfully, result.Message);
        Assert.Equal(expectedDtos, result.Data);
    }

    [Fact]
    public async Task GetExamsForInstructor_Should_Return_Empty_List_When_No_Exams()
    {
        // Arrange
        var user = new User { Instructor = new Instructor { Id = 5 } };
        var exams = new List<Exam>();
        var expectedDtos = new List<GetExamForInstructorDto>();

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamsByInstructorIdAsync(user.Instructor.Id)).ReturnsAsync(exams);
        _mapperMock.Setup(m => m.Map<List<GetExamForInstructorDto>>(exams)).Returns(expectedDtos);

        // Act
        var result = await _service.GetExamsForInstructor();

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Data);
        Assert.Equal(Messages.ExamsRetrievedSuccessfully, result.Message);
    }

    [Fact]
    public async Task GetExamsForInstructor_Should_Throw_When_User_Is_Null()
    {
        // Arrange
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(null);

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _service.GetExamsForInstructor());
    }

    [Fact]
    public async Task GetExamsForStudent_Should_Return_Success_When_Exams_Exist()
    {
        // Arrange
        var user = new User { Student = new Student { Id = 10 } };
        var exams = new List<Exam>
        {
            new Exam { Id = 1 },
            new Exam { Id = 2 }
        };
        var expectedDtos = new List<GetExamForStudentDto>
        {
            new GetExamForStudentDto(),
            new GetExamForStudentDto()
        };

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamsByStudentIdAsync(user.Student.Id)).ReturnsAsync(exams);
        _mapperMock.Setup(m => m.Map<List<GetExamForStudentDto>>(exams)).Returns(expectedDtos);

        // Act
        var result = await _service.GetExamsForStudent();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExamsRetrievedSuccessfully, result.Message);
        Assert.Equal(expectedDtos, result.Data);
    }

    [Fact]
    public async Task GetExamsForStudent_Should_Return_EmptyList_When_No_Exams()
    {
        // Arrange
        var user = new User { Student = new Student { Id = 10 } };
        var exams = new List<Exam>();
        var expectedDtos = new List<GetExamForStudentDto>();

        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(user);
        _examRepositoryMock.Setup(r => r.GetExamsByStudentIdAsync(user.Student.Id)).ReturnsAsync(exams);
        _mapperMock.Setup(m => m.Map<List<GetExamForStudentDto>>(exams)).Returns(expectedDtos);

        // Act
        var result = await _service.GetExamsForStudent();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(Messages.ExamsRetrievedSuccessfully, result.Message);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetExamsForStudent_Should_Throw_When_User_Is_Null()
    {
        // Arrange
        _httpContextMock.Setup(x => x.HttpContext!.Items["User"]).Returns(null);

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _service.GetExamsForStudent());
    }

}