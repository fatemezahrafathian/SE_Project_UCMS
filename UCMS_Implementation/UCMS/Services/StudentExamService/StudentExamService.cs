using ClosedXML.Excel;
using Microsoft.Extensions.Options;
using UCMS.DTOs;
using UCMS.DTOs.ExerciseSubmissionDto;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.ExamRepository.Abstraction;
using UCMS.Repositories.StudentExamRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.ExerciseSubmissionService;
using UCMS.Services.StudentExamService.Abstraction;

namespace UCMS.Services.StudentExamService;

public class StudentExamService: IStudentExamService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ExerciseScoreTemplateSettings _exerciseScoreTemplateSettings;
    private readonly IExamRepository _examRepository;
    private readonly IStudentExamRepository _studentExamRepository;

    public StudentExamService(IOptions<ExerciseScoreTemplateSettings> templateSettingsOptions, IHttpContextAccessor httpContextAccessor, IStudentExamRepository studentExamRepository, IExamRepository examRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _studentExamRepository = studentExamRepository;
        _examRepository = examRepository;
        _exerciseScoreTemplateSettings = templateSettingsOptions.Value;
    }

    public async Task<ServiceResponse<FileDownloadDto>> GetExamScoreTemplateFile(int exerciseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var exam = await _examRepository.GetExamWithClassRelationsByIdAsync(exerciseId);
        if (exam==null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ExerciseNotFound);
        }
        
        if (exam.Class.InstructorId!=user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.CanNotaccessExercise);
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(_exerciseScoreTemplateSettings.WorksheetName);
        worksheet.RightToLeft = true;
    
        worksheet.Cell(1, 1).Value = _exerciseScoreTemplateSettings.ColumnHeaders[0];
        worksheet.Cell(1, 2).Value = _exerciseScoreTemplateSettings.ColumnHeaders[1];
        worksheet.Cell(1, 3).Value = _exerciseScoreTemplateSettings.ColumnHeaders[2];

        var students = exam.Class.ClassStudents
            .OrderBy(cs=>cs.Student.User.LastName + " " + cs.Student.User.FirstName);
        
        int row = 2;
        foreach (var classStudent in students)
        {
            var student = classStudent.Student;
            var userInfo = student.User;

            worksheet.Cell(row, 1).Value = $"{userInfo.LastName} {userInfo.FirstName}";
            worksheet.Cell(row, 2).Value = student.StudentNumber;
            row++;
        }
        
        await using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Seek(0, SeekOrigin.Begin);

        var fileBytes = stream.ToArray();

        var result = new FileDownloadDto()
        {
            FileName = _exerciseScoreTemplateSettings.FileName,
            ContentType = _exerciseScoreTemplateSettings.ContentType,
            FileBytes = fileBytes
        };

        return ServiceResponseFactory.Success(result, Messages.ExerciseScoreTemplateFileGeneratedSuccessfully);
    }
    
    public async Task<ServiceResponse<List<GetScoreFileValidationResultDto>>> UpdateExamScores(int examId, IFormFile scoreFile)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
    
        var exam = await _examRepository.GetExamByIdAsync(examId);
        if (exam==null)
        {
            return ServiceResponseFactory.Failure<List<GetScoreFileValidationResultDto>>(Messages.ExamNotFound);
        }
        
        if (exam.Class.InstructorId!=user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<List<GetScoreFileValidationResultDto>>(Messages.ExamCantBeAccessed);
        }
    
        XLWorkbook workbook;
        IXLWorksheet worksheet;
    
        try
        {
            workbook = new XLWorkbook(scoreFile.OpenReadStream());
            worksheet = workbook.Worksheet(_exerciseScoreTemplateSettings.WorksheetName);
        }
        catch
        {
            return ServiceResponseFactory.Failure<List<GetScoreFileValidationResultDto>>(Messages.InvalidTemplateFileFormat);
        }
    
        worksheet.RightToLeft = true;
    
        for (int i = 1; i <= 3; i++)
        {
            var expectedHeader = i switch
            {
                1 => _exerciseScoreTemplateSettings.ColumnHeaders[0],
                2 => _exerciseScoreTemplateSettings.ColumnHeaders[1],
                _ => _exerciseScoreTemplateSettings.ColumnHeaders[2]
            };
    
            var actualHeader = worksheet.Cell(1, i).GetString().Trim();
    
            if (actualHeader != expectedHeader)
            {
                return ServiceResponseFactory.Failure<List<GetScoreFileValidationResultDto>>(Messages.InvalidTemplateFileFormat);
            }
        }
    
        var validationResults = new List<GetScoreFileValidationResultDto>();
        var studentExams = new List<StudentExam>();
    
        int totalColumns = 3; // not nullable
        int row = 2;
    
        bool RowHasAnyData(IXLWorksheet sheet, int rowNum, int totalCols)
        {
            return Enumerable.Range(1, totalCols)
                .Any(col => !string.IsNullOrWhiteSpace(sheet.Cell(rowNum, col).GetString()));
        }
        
        while (RowHasAnyData(worksheet, row, totalColumns))
        {
            // var studentName = worksheet.Cell(row, 1).GetString().Trim();
            var studentNumber = worksheet.Cell(row, 2).GetString().Trim();
            var score = worksheet.Cell(row, 3).GetDouble();
            
            var errors = new List<string>();
    
            var studentExam = await _studentExamRepository.GetStudentExamsByStudentNumberAsync(examId, studentNumber);
            if (studentExam==null && score>0 || score < 0 || score > exam.ExamScore)
            {
                errors.Add(Messages.InvalidScore);
            }
            
            validationResults.Add(new GetScoreFileValidationResultDto
            {
                RowNumber = row,
                IsValid = !errors.Any(),
                Errors = errors
            });
    
            if (studentExam != null)
            {
                studentExam.Score = score;
                studentExams.Add(studentExam);
            }
            
            row++;
        }
        
        if (validationResults.Any(r => !r.IsValid))
        {
            return ServiceResponseFactory.Failure(validationResults, Messages.ExamScoresCanNotBeUpdated);
        }
        
        await _studentExamRepository.UpdateRangeStudentExamAsync(studentExams);
    
        return ServiceResponseFactory.Success<List<GetScoreFileValidationResultDto>>(Messages.ExamScoresUpdatedSuccessfully);
    
    }
    
}