using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.ExamRepository;

namespace UCMS_Test.Repositories;

public class ExamRepositoryTest
{
    private DataContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldAddExamToDatabase()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repository = new ExamRepository(context);

        var exam = new Exam
        {
            Id = 1,
            Title = "Final Exam",
            Class = new Class
            {
                Id = 10,
                Title = "Math",
                PasswordHash = new byte[] { 1 },
                PasswordSalt = new byte[] { 1 }
            }
        };

        // Act
        await repository.AddAsync(exam);

        // Assert
        var addedExam = await context.Exams.FirstOrDefaultAsync(e => e.Id == 1);
        Assert.NotNull(addedExam);
        Assert.Equivalent(exam, addedExam);
    }

    [Fact]
    public async Task GetExamByIdAsync_ReturnsExam_WhenExamExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var classEntity = new Class
        {
            Id = 1,
            Title = "Physics",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 }
        };

        var exam = new Exam
        {
            Id = 10,
            Title = "Midterm",
            Class = classEntity,
        };

        context.Classes.Add(classEntity);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repository = new ExamRepository(context);

        // Act
        var result = await repository.GetExamByIdAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(exam, result);
    }

    [Fact]
    public async Task GetExamByIdAsync_ReturnsNull_WhenExamDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repository = new ExamRepository(context);

        // Act
        var result = await repository.GetExamByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetExamByIdAsync_LoadsClass()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var classEntity = new Class
        {
            Id = 1,
            Title = "Physics",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 }
        };

        var exam = new Exam
        {
            Id = 10,
            Title = "Midterm",
            Class = classEntity,
        };

        context.Classes.Add(classEntity);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repository = new ExamRepository(context);

        // Act
        var result = await repository.GetExamByIdAsync(10);

        // Assert
        Assert.NotNull(result!.Class);
        Assert.Equivalent(exam.Class, result.Class);
    }

    [Fact]
    public async Task GetExamWithClassRelationsByIdAsync_ReturnsExam_WhenExists()
    {
        var context = GetInMemoryDbContext();

        var user = new User { Id = 1, Username = "user1", Email = "u1@example.com", PasswordHash = new byte[1], PasswordSalt = new byte[1] };
        var student = new Student { Id = 2, User = user };
        var classStudent = new ClassStudent { Student = student };
    
        var @class = new Class
        {
            Id = 3,
            Title = "Test Class",
            PasswordHash = new byte[1],
            PasswordSalt = new byte[1],
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var exam = new Exam
        {
            Id = 10,
            Title = "Sample Exam",
            Class = @class
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);

        // Act
        var result = await repo.GetExamWithClassRelationsByIdAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equivalent(exam, result);
    }

    [Fact]
    public async Task GetExamWithClassRelationsByIdAsync_ReturnsNull_WhenExamDoesNotExist()
    {
        var context = GetInMemoryDbContext();
        var repo = new ExamRepository(context);

        var result = await repo.GetExamWithClassRelationsByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetExamWithClassRelationsByIdAsync_LoadsClass()
    {
        var context = GetInMemoryDbContext();

        var user = new User { Id = 1, Username = "user1", Email = "u1@example.com", PasswordHash = new byte[1], PasswordSalt = new byte[1] };
        var student = new Student { Id = 2, User = user };
        var classStudent = new ClassStudent { Student = student };
    
        var @class = new Class
        {
            Id = 3,
            Title = "Test Class",
            PasswordHash = new byte[1],
            PasswordSalt = new byte[1],
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var exam = new Exam
        {
            Id = 10,
            Title = "Sample Exam",
            Class = @class
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);

        // Act
        var result = await repo.GetExamWithClassRelationsByIdAsync(10);

        // Assert
        Assert.NotNull(result!.Class);
        Assert.Equivalent(@class, result.Class);
    }

    [Fact]
    public async Task GetExamWithClassRelationsByIdAsync_LoadsClassStudents()
    {
        var context = GetInMemoryDbContext();

        var user = new User { Id = 1, Username = "user1", Email = "u1@example.com", PasswordHash = new byte[1], PasswordSalt = new byte[1] };
        var student = new Student { Id = 2, User = user };
        var classStudent = new ClassStudent { Student = student };
    
        var @class = new Class
        {
            Id = 3,
            Title = "Test Class",
            PasswordHash = new byte[1],
            PasswordSalt = new byte[1],
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var exam = new Exam
        {
            Id = 10,
            Title = "Sample Exam",
            Class = @class
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);

        // Act
        var result = await repo.GetExamWithClassRelationsByIdAsync(10);

        // Assert
        Assert.Single(result!.Class.ClassStudents);
        Assert.Equivalent(classStudent, result.Class.ClassStudents.First());
    }

    [Fact]
    public async Task GetExamWithClassRelationsByIdAsync_LoadsStudent()
    {
        var context = GetInMemoryDbContext();

        var user = new User { Id = 1, Username = "user1", Email = "u1@example.com", PasswordHash = new byte[1], PasswordSalt = new byte[1] };
        var student = new Student { Id = 2, User = user };
        var classStudent = new ClassStudent { Student = student };
    
        var @class = new Class
        {
            Id = 3,
            Title = "Test Class",
            PasswordHash = new byte[1],
            PasswordSalt = new byte[1],
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var exam = new Exam
        {
            Id = 10,
            Title = "Sample Exam",
            Class = @class
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);

        // Act
        var result = await repo.GetExamWithClassRelationsByIdAsync(10);

        // Assert
        Assert.NotNull(result!.Class.ClassStudents.First().Student);
        Assert.Equivalent(student, result.Class.ClassStudents.First().Student);
    }
    
    [Fact]
    public async Task GetExamWithClassRelationsByIdAsync_LoadsUser()
    {
        var context = GetInMemoryDbContext();

        var user = new User { Id = 1, Username = "user1", Email = "u1@example.com", PasswordHash = new byte[1], PasswordSalt = new byte[1] };
        var student = new Student { Id = 2, User = user };
        var classStudent = new ClassStudent { Student = student };
    
        var @class = new Class
        {
            Id = 3,
            Title = "Test Class",
            PasswordHash = new byte[1],
            PasswordSalt = new byte[1],
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var exam = new Exam
        {
            Id = 10,
            Title = "Sample Exam",
            Class = @class
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);

        // Act
        var result = await repo.GetExamWithClassRelationsByIdAsync(10);

        // Assert
        Assert.NotNull(result!.Class.ClassStudents.First().Student.User);
        Assert.Equivalent(user, result.Class.ClassStudents.First().Student.User);
    }

    [Fact]
    public async Task GetExamsByClassIdAsync_ReturnsExams_WhenClassHasExams()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var @class = new Class
        {
            Id = 1,
            Title = "Math",
            PasswordHash = new byte[1],
            PasswordSalt = new byte[1]
        };

        var exams = new List<Exam>
        {
            new Exam { Id = 1, Title = "Exam 1", ClassId = 1 },
            new Exam { Id = 2, Title = "Exam 2", ClassId = 1 }
        };

        context.Classes.Add(@class);
        context.Exams.AddRange(exams);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);

        // Act
        var result = await repo.GetExamsByClassIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal(1, e.ClassId));
    }

    [Fact]
    public async Task GetExamsByClassIdAsync_ReturnsEmptyList_WhenNoExamsForClass()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ExamRepository(context);

        // Act
        var result = await repo.GetExamsByClassIdAsync(999);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExamsByClassIdAsync_ReturnsExams_WhenMultipleClasses()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var @class = new Class
        {
            Id = 1,
            Title = "Math",
            PasswordHash = new byte[1],
            PasswordSalt = new byte[1]
        };

        var exam1 = new Exam {Id = 1, Title = "Exam 1", ClassId = 1};
        var exam2 = new Exam {Id = 2, Title = "Exam 2", ClassId = 2};

        var exams = new List<Exam>
        {
            exam1, exam2
        };

        context.Classes.Add(@class);
        context.Exams.AddRange(exams);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);

        // Act
        var result = await repo.GetExamsByClassIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equivalent(exam1, result.First());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExam_WhenExamExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var exam = new Exam
        {
            Id = 1,
            Title = "Old Title",
            Class = new Class
            {
                Id = 10,
                Title = "Test Class",
                PasswordHash = new byte[1],
                PasswordSalt = new byte[1]
            }
        };

        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        // Act
        exam.Title = "Updated Title";
        var repo = new ExamRepository(context);
        await repo.UpdateAsync(exam);

        // Assert
        var updated = await context.Exams.FindAsync(1);
        Assert.NotNull(updated);
        Assert.Equal("Updated Title", updated!.Title);
    }

    [Fact]
    public async Task UpdateAsync_DoesNothing_WhenExamDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var exam = new Exam
        {
            Id = 99,
            Title = "Non-existing",
            Class = new Class
            {
                Id = 10,
                Title = "Dummy",
                PasswordHash = new byte[1],
                PasswordSalt = new byte[1]
            }
        };

        var repo = new ExamRepository(context);

        // Act
        var exception = await Record.ExceptionAsync(() => repo.UpdateAsync(exam));

        // Assert
        Assert.Null(exception);
        Assert.Empty(context.Exams);
    }

    [Fact]
    public async Task ExistsWithTitleExceptIdAsync_ReturnsTrue_WhenDuplicateTitleExists()
    {
        var context = GetInMemoryDbContext();
        var exam = new Exam
        {
            Id = 1,
            Title = "Midterm",
            ClassId = 10
        };

        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);
        var result = await repo.ExistsWithTitleExceptIdAsync("Midterm", 10, 2);

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsWithTitleExceptIdAsync_ReturnsFalse_WhenOnlyMatchingExamIsExcluded()
    {
        var context = GetInMemoryDbContext();
        var exam = new Exam
        {
            Id = 5,
            Title = "Final",
            ClassId = 11
        };

        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);
        var result = await repo.ExistsWithTitleExceptIdAsync("Final", 11, 5);

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsWithTitleExceptIdAsync_ReturnsFalse_WhenTitleExistsInDifferentClass()
    {
        var context = GetInMemoryDbContext();
        var exam = new Exam
        {
            Id = 3,
            Title = "Quiz",
            ClassId = 20
        };

        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);
        var result = await repo.ExistsWithTitleExceptIdAsync("Quiz", 21, 3); 

        Assert.False(result);
    }

    [Fact]
    public async Task ExistsWithTitleExceptIdAsync_ReturnsFalse_WhenNoMatchingTitleExists()
    {
        var context = GetInMemoryDbContext();

        var repo = new ExamRepository(context);
        var result = await repo.ExistsWithTitleExceptIdAsync("Unknown Title", 30, 99);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesExam_WhenExamExists()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var exam = new Exam
        {
            Id = 1,
            Title = "Midterm",
            ClassId = 10
        };

        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);

        // Act
        await repo.DeleteAsync(exam);

        // Assert
        var deletedExam = await context.Exams.FindAsync(1);
        Assert.Null(deletedExam);
    }
    
    [Fact]
    public async Task DeleteAsync_DoesNothing_WhenExamDoesNotExist()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var repo = new ExamRepository(context);

        var nonExistingExam = new Exam
        {
            Id = 999,
            Title = "Non-existing",
            ClassId = 11
        };

        // Act
        var exception = await Record.ExceptionAsync(() => repo.DeleteAsync(nonExistingExam));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task DeleteAsync_DoesNotRemoveOtherExams()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var exam1 = new Exam
        {
            Id = 1,
            Title = "Midterm",
            ClassId = 10
        };

        var exam2 = new Exam
        {
            Id = 2,
            Title = "Final",
            ClassId = 10
        };

        context.Exams.AddRange(exam1, exam2);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);

        // Act
        await repo.DeleteAsync(exam1);

        // Assert
        var remaining = await context.Exams.FindAsync(2);
        Assert.NotNull(remaining);
        Assert.Equivalent(exam2, remaining);
    }

    [Fact]
    public async Task GetExamsByStudentIdAsync_ReturnsExams_WhenStudentIsEnrolled()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var student = new Student { Id = 1 };
        var classStudent = new ClassStudent { StudentId = 1, Student = student };
        var @class = new Class
        {
            Id = 10,
            Title = "Math",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };
        var exam = new Exam
        {
            Id = 100,
            Title = "Midterm",
            Class = @class
        };

        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);

        // Act
        var result = await repo.GetExamsByStudentIdAsync(1);

        // Assert
        Assert.Single(result);
        Assert.Equivalent(exam, result[0]);
    }

    [Fact]
    public async Task GetExamsByStudentIdAsync_ReturnsEmpty_WhenStudentNotEnrolled()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var @class = new Class
        {
            Id = 20,
            Title = "Physics",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 }
        };

        var exam = new Exam
        {
            Id = 101,
            Title = "Final",
            Class = @class
        };

        context.Classes.Add(@class);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);

        // Act
        var result = await repo.GetExamsByStudentIdAsync(99);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExamsByStudentIdAsync_DoesNotReturnExamsFromOtherStudents()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var student = new Student { Id = 2 };
        var classStudent = new ClassStudent { StudentId = 2, Student = student };
        var @class = new Class
        {
            Id = 30,
            Title = "Chemistry",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var exam1 = new Exam
        {
            Id = 102,
            Title = "Chemistry Test",
            Class = @class
        };

        var exam2 = new Exam
        {
            Id = 103,
            Title = "Biology Test",
            ClassId = 999
        };

        context.Students.Add(student);
        context.Classes.Add(@class);
        context.Exams.AddRange(exam1, exam2);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);

        // Act
        var result = await repo.GetExamsByStudentIdAsync(2);

        // Assert
        Assert.Single(result);
        Assert.Equivalent(exam1, result[0]);
    }
    
    [Fact]
    public async Task GetExamsByInstructorIdAsync_ReturnsExamsForGivenInstructor()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var @class = new Class
        {
            Id = 1,
            InstructorId = 10,
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            Title = "Class A"
        };
        var exam = new Exam
        {
            Id = 1,
            Title = "Exam A",
            Class = @class
        };

        context.Classes.Add(@class);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);

        // Act
        var result = await repo.GetExamsByInstructorIdAsync(10);

        // Assert
        Assert.Single(result);
        Assert.Equivalent(exam, result[0]);
    }

    [Fact]
    public async Task GetExamsByInstructorIdAsync_ReturnsEmpty_WhenInstructorDoesNotMatch()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var @class = new Class
        {
            Id = 2,
            InstructorId = 20,
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            Title = "Class B"
        };
        var exam = new Exam
        {
            Id = 2,
            Title = "Exam B",
            Class = @class
        };

        context.Classes.Add(@class);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);

        // Act
        var result = await repo.GetExamsByInstructorIdAsync(99);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExamsByInstructorIdAsync_ReturnsMultipleExams()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var instructorId = 33;

        var class1 = new Class
        {
            Id = 101,
            InstructorId = instructorId,
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 },
            Title = "Class X"
        };
        var class2 = new Class
        {
            Id = 102,
            InstructorId = instructorId,
            PasswordHash = new byte[] { 2 },
            PasswordSalt = new byte[] { 2 },
            Title = "Class Y"
        };

        var exam1 = new Exam { Id = 5, Title = "Exam 1", Class = class1 };
        var exam2 = new Exam { Id = 6, Title = "Exam 2", Class = class2 };

        context.Classes.AddRange(class1, class2);
        context.Exams.AddRange(exam1, exam2);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);

        // Act
        var result = await repo.GetExamsByInstructorIdAsync(instructorId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.Title == "Exam 1");
        Assert.Contains(result, e => e.Title == "Exam 2");
    }

    [Fact]
    public async Task GetExamsCloseDeadLines_ReturnsExamsWithinDateRange()
    {
        var context = GetInMemoryDbContext();
        var classEntity = new Class
        {
            Id = 1,
            Title = "Class A",
            PasswordHash = new byte[] { 1 },
            PasswordSalt = new byte[] { 1 }
        };

        var exam = new Exam
        {
            Id = 1,
            Title = "Upcoming Exam",
            Class = classEntity,
            Date = DateTime.UtcNow.AddDays(2)
        };

        context.Classes.Add(classEntity);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);
        var lowerBound = DateTime.UtcNow.AddDays(1);
        var upperBound = DateTime.UtcNow.AddDays(3);

        var result = await repo.GetExamsCloseDeadLines(lowerBound, upperBound, CancellationToken.None);

        Assert.Single(result);
        Assert.Equivalent(exam, result[0]);
    }

    [Fact]
    public async Task GetExamsCloseDeadLines_ReturnsEmpty_WhenNoExamsInDateRange()
    {
        var context = GetInMemoryDbContext();
        var exam = new Exam
        {
            Id = 2,
            Title = "Past Exam",
            Class = new Class
            {
                Id = 2,
                Title = "Class B",
                PasswordHash = new byte[] { 2 },
                PasswordSalt = new byte[] { 2 }
            },
            Date = DateTime.UtcNow.AddDays(-10)
        };

        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);
        var lowerBound = DateTime.UtcNow;
        var upperBound = DateTime.UtcNow.AddDays(5);

        var result = await repo.GetExamsCloseDeadLines(lowerBound, upperBound, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExamsCloseDeadLines_LoadsClass()
    {
        var context = GetInMemoryDbContext();

        var user = new User { Id = 10, Username = "student1", Email = "test@test.com", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } };
        var student = new Student { Id = 20, User = user };
        var classStudent = new ClassStudent { StudentId = 2, Student = student };

        var classEntity = new Class
        {
            Id = 3,
            Title = "Class C",
            PasswordHash = new byte[] { 3 },
            PasswordSalt = new byte[] { 3 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var exam = new Exam
        {
            Id = 3,
            Title = "Linked Exam",
            Date = DateTime.UtcNow.AddDays(1),
            Class = classEntity
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(classEntity);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);
        var lowerBound = DateTime.UtcNow;
        var upperBound = DateTime.UtcNow.AddDays(2);

        var result = await repo.GetExamsCloseDeadLines(lowerBound, upperBound, CancellationToken.None);

        Assert.NotNull(result.First().Class);
        Assert.Equivalent(classEntity, result.First().Class);
    }

    [Fact]
    public async Task GetExamsCloseDeadLines_LoadsClassStudents()
    {
        var context = GetInMemoryDbContext();

        var user = new User { Id = 10, Username = "student1", Email = "test@test.com", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } };
        var student = new Student { Id = 20, User = user };
        var classStudent = new ClassStudent { StudentId = 2, Student = student };

        var classEntity = new Class
        {
            Id = 3,
            Title = "Class C",
            PasswordHash = new byte[] { 3 },
            PasswordSalt = new byte[] { 3 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var exam = new Exam
        {
            Id = 3,
            Title = "Linked Exam",
            Date = DateTime.UtcNow.AddDays(1),
            Class = classEntity
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(classEntity);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);
        var lowerBound = DateTime.UtcNow;
        var upperBound = DateTime.UtcNow.AddDays(2);

        var result = await repo.GetExamsCloseDeadLines(lowerBound, upperBound, CancellationToken.None);

        Assert.NotNull(result.First().Class);
        Assert.Equivalent(classEntity, result.First().Class);

        var loadedClassStudent = result.First().Class.ClassStudents;
        Assert.Single(loadedClassStudent);
        Assert.Equivalent(classStudent, loadedClassStudent.First());
    }

    [Fact]
    public async Task GetExamsCloseDeadLines_LoadsStudent()
    {
        var context = GetInMemoryDbContext();

        var user = new User { Id = 10, Username = "student1", Email = "test@test.com", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } };
        var student = new Student { Id = 20, User = user };
        var classStudent = new ClassStudent { StudentId = 2, Student = student };

        var classEntity = new Class
        {
            Id = 3,
            Title = "Class C",
            PasswordHash = new byte[] { 3 },
            PasswordSalt = new byte[] { 3 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var exam = new Exam
        {
            Id = 3,
            Title = "Linked Exam",
            Date = DateTime.UtcNow.AddDays(1),
            Class = classEntity
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(classEntity);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);
        var lowerBound = DateTime.UtcNow;
        var upperBound = DateTime.UtcNow.AddDays(2);

        var result = await repo.GetExamsCloseDeadLines(lowerBound, upperBound, CancellationToken.None);

        Assert.NotNull(result.First().Class);
        Assert.Equivalent(classEntity, result.First().Class);
        
        var loadedStudent = result.First().Class.ClassStudents.First().Student;
        Assert.NotNull(loadedStudent);
        Assert.Equivalent(student, loadedStudent);
    }

    [Fact]
    public async Task GetExamsCloseDeadLines_LoadsUser()
    {
        var context = GetInMemoryDbContext();

        var user = new User { Id = 10, Username = "student1", Email = "test@test.com", PasswordHash = new byte[] { 1 }, PasswordSalt = new byte[] { 1 } };
        var student = new Student { Id = 20, User = user };
        var classStudent = new ClassStudent { StudentId = 2, Student = student };

        var classEntity = new Class
        {
            Id = 3,
            Title = "Class C",
            PasswordHash = new byte[] { 3 },
            PasswordSalt = new byte[] { 3 },
            ClassStudents = new List<ClassStudent> { classStudent }
        };

        var exam = new Exam
        {
            Id = 3,
            Title = "Linked Exam",
            Date = DateTime.UtcNow.AddDays(1),
            Class = classEntity
        };

        context.Users.Add(user);
        context.Students.Add(student);
        context.Classes.Add(classEntity);
        context.Exams.Add(exam);
        await context.SaveChangesAsync();

        var repo = new ExamRepository(context);
        var lowerBound = DateTime.UtcNow;
        var upperBound = DateTime.UtcNow.AddDays(2);

        var result = await repo.GetExamsCloseDeadLines(lowerBound, upperBound, CancellationToken.None);

        Assert.NotNull(result.First().Class);
        Assert.Equivalent(classEntity, result.First().Class);
        
        var loadedUser = result.First().Class.ClassStudents.First().Student.User;
        Assert.NotNull(loadedUser);
        Assert.Equivalent(user, loadedUser);
    }

}