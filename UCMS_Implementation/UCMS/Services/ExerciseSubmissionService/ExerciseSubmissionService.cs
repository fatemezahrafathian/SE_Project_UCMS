using AutoMapper;
using ClosedXML.Excel;
using Microsoft.Extensions.Options;
using UCMS.DTOs;
using UCMS.DTOs.ExerciseSubmissionDto;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.ExerciseRepository.Abstraction;
using UCMS.Repositories.ExerciseSubmissionRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.ExerciseSubmissionService.Abstraction;
using UCMS.Services.FileService;

namespace UCMS.Services.ExerciseSubmissionService;

public class ExerciseSubmissionService: IExerciseSubmissionService
{
    private readonly IExerciseSubmissionRepository _exerciseSubmissionRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IFileService _fileService;
    private readonly IExerciseRepository _exerciseRepository;
    private readonly ExerciseScoreTemplateSettings _exerciseScoreTemplateSettings;
    private readonly IMapper _mapper;
    
    public ExerciseSubmissionService(IExerciseSubmissionRepository exerciseSubmissionRepository, IHttpContextAccessor httpContextAccessor, IFileService fileService, IExerciseRepository exerciseRepository, IMapper mapper, IOptions<ExerciseScoreTemplateSettings> templateSettingsOptions)
    {
        _exerciseSubmissionRepository = exerciseSubmissionRepository;
        _httpContextAccessor = httpContextAccessor;
        _fileService = fileService;
        _exerciseRepository = exerciseRepository;
        _mapper = mapper;
        _exerciseScoreTemplateSettings = templateSettingsOptions.Value;
    }

    public async Task<ServiceResponse<GetExerciseSubmissionPreviewForStudentDto>> CreateExerciseSubmission(int exerciseId, CreateExerciseSubmissionDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var exercise = await _exerciseRepository.GetExerciseWithRelationsByIdAsync(exerciseId);
        if (exercise==null)
        {
            return ServiceResponseFactory.Failure<GetExerciseSubmissionPreviewForStudentDto>(Messages.ExerciseNotFound);
        }

        if (exercise.Class.ClassStudents.All(cs => cs.Student.Id != user!.Student!.Id))
        {
            return ServiceResponseFactory.Failure<GetExerciseSubmissionPreviewForStudentDto>(Messages.CanNotaccessExercise);
        }

        var validator = new CreateExerciseSubmissionDtoValidator();
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<GetExerciseSubmissionPreviewForStudentDto>(errorMessage);
        }

        if (!_fileService.IsValidExtension(dto.SubmissionFile!, exercise.FileFormats))
        {
            return ServiceResponseFactory.Failure<GetExerciseSubmissionPreviewForStudentDto>(Messages.InvalidFormat);
        }

        if (!_fileService.IsValidFileSize(dto.SubmissionFile!))
        {
            return ServiceResponseFactory.Failure<GetExerciseSubmissionPreviewForStudentDto>(Messages.InvalidSize);
        }
        
        var filePath = await _fileService.SaveFileAsync(dto.SubmissionFile!, "exercise-submissions");

        var currentFinalExerciseSubmission =
            await _exerciseSubmissionRepository.GetFinalExerciseSubmissionsAsync(
                user!.Student!.Id, exerciseId);
        
        if (currentFinalExerciseSubmission != null)
        {
            currentFinalExerciseSubmission.IsFinal = false;
            await _exerciseSubmissionRepository.UpdateExerciseSubmissionAsync(currentFinalExerciseSubmission);
        }

        var newExerciseSubmission = new ExerciseSubmission()
        {
            ExerciseId = exerciseId,
            StudentId = user.Student!.Id,
            FilePath = filePath
        };
        
        await _exerciseSubmissionRepository.AddExerciseSubmissionAsync(newExerciseSubmission);

        var newExerciseSubmissionDto = _mapper.Map<GetExerciseSubmissionPreviewForStudentDto>(newExerciseSubmission);
        newExerciseSubmissionDto.FileType = Path.GetExtension((string?) filePath)?.TrimStart('.').ToLower() ?? "unknown";

        return ServiceResponseFactory.Success(newExerciseSubmissionDto, Messages.ExerciseSubmissionCreatedSuccessfully);
        
    }

    public async Task<ServiceResponse<FileDownloadDto>> GetExerciseSubmissionFileForInstructor(int exerciseSubmissionId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var exerciseSubmission = await _exerciseSubmissionRepository.GetExerciseSubmissionForInstructorByIdAsync(exerciseSubmissionId);
        if (exerciseSubmission==null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ExerciseSubmissionNotFound);
        }
        
        if (exerciseSubmission.Exercise.Class.InstructorId!=user!.Instructor!.Id || !exerciseSubmission.IsFinal)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ExerciseSubmissionCanNotBeAccessed);
        }

        var dto = await _fileService.DownloadFile2(exerciseSubmission.FilePath);
        if (dto == null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        }

        return ServiceResponseFactory.Success(dto, Messages.ExerciseSubmissionFileFetchedSuccessfully);
        
    }

    public async Task<ServiceResponse<FileDownloadDto>> GetExerciseSubmissionFileForStudent(int exerciseSubmissionId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var exerciseSubmission = await _exerciseSubmissionRepository.GetExerciseSubmissionByIdAsync(exerciseSubmissionId);
        if (exerciseSubmission==null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ExerciseSubmissionNotFound);
        }

        if (exerciseSubmission.StudentId != user!.Student!.Id)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ExerciseSubmissionCanNotBeAccessed);
        }

        var dto = await _fileService.DownloadFile2(exerciseSubmission.FilePath);
        if (dto == null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        }

        return ServiceResponseFactory.Success(dto, Messages.ExerciseSubmissionFileFetchedSuccessfully);
        
    }

    public async Task<ServiceResponse<FileDownloadDto>> GetExerciseSubmissionFiles(int exerciseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var exercise = await _exerciseRepository.GetExerciseByIdAsync(exerciseId);
        if (exercise == null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ExerciseNotFound);
        }

        if (exercise.Class.InstructorId != user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.CanNotaccessExercise);
        }

        var submissions = await _exerciseSubmissionRepository.GetExerciseSubmissionsAsync(exerciseId);
        if (submissions.Count == 0)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.NoExerciseSubmissionFound);
        }

        var namedFilePaths = submissions.ToDictionary(
            s => $"{s.Student.User.LastName} {s.Student.User.FirstName}-{s.Student.StudentNumber}{Path.GetExtension(s.FilePath)}",
            s => s.FilePath
        );

        var zipFile = await _fileService.ZipFiles(namedFilePaths, $"{exercise.Title}-submissions");
        if (zipFile == null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        }

        return ServiceResponseFactory.Success(zipFile, Messages.ExerciseSubmissionFilesFetchedSuccessfully);
    }


    public async Task<ServiceResponse<List<GetExerciseSubmissionPreviewForInstructorDto>>> GetExerciseSubmissionsForInstructor(SortExerciseSubmissionsForInstructorDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var exercise = await _exerciseRepository.GetExerciseByIdAsync(dto.ExerciseId);
        if (exercise==null)
        {
            return ServiceResponseFactory.Failure<List<GetExerciseSubmissionPreviewForInstructorDto>>(Messages.ExerciseNotFound);
        }
        
        if (exercise.Class.InstructorId!=user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<List<GetExerciseSubmissionPreviewForInstructorDto>>(Messages.CanNotaccessExercise);
        }
        
        var validator = new SortExerciseSubmissionForInstructorDtoValidator();
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<List<GetExerciseSubmissionPreviewForInstructorDto>>(errorMessage);
        }

        var submissions = await _exerciseSubmissionRepository.GetExerciseSubmissionsForInstructorByExerciseIdAsync(dto.ExerciseId, dto.SortBy, dto.SortOrder);

        var submissionDtos = _mapper.Map<List<GetExerciseSubmissionPreviewForInstructorDto>>(submissions);
        
        var submissionDict = submissions.ToDictionary(s => s.Id, s => s.FilePath);

        foreach (var dtoItem in submissionDtos)
        {
            if (submissionDict.TryGetValue(dtoItem.Id, out var filePath))
            {
                dtoItem.FileType = Path.GetExtension((string?) filePath)?.TrimStart('.').ToLower() ?? "unknown";
            }
        }
        
        return ServiceResponseFactory.Success(submissionDtos, Messages.ExerciseSubmissionsFetchedSuccessfully);

    }

    public async Task<ServiceResponse<List<GetExerciseSubmissionPreviewForStudentDto>>> GetExerciseSubmissionsForStudent(SortExerciseSubmissionsStudentDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var exercise = await _exerciseRepository.GetExerciseWithRelationsByIdAsync(dto.ExerciseId);
        if (exercise==null)
        {
            return ServiceResponseFactory.Failure<List<GetExerciseSubmissionPreviewForStudentDto>>(Messages.ExerciseNotFound);
        }

        if (exercise.Class.ClassStudents.All(cs => cs.Student.Id != user!.Student!.Id))
        {
            return ServiceResponseFactory.Failure<List<GetExerciseSubmissionPreviewForStudentDto>>(Messages.CanNotaccessExercise);
        }
        
        var validator = new SortExerciseSubmissionForStudentDtoValidator();
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<List<GetExerciseSubmissionPreviewForStudentDto>>(errorMessage);
        }

        var submissions = await _exerciseSubmissionRepository.GetExerciseSubmissionsForStudentByExerciseIdAsync(user!.Student!.Id, dto.ExerciseId, dto.SortBy, dto.SortOrder);

        var submissionDtos = _mapper.Map<List<GetExerciseSubmissionPreviewForStudentDto>>(submissions);
        
        var submissionDict = submissions.ToDictionary(s => s.Id, s => s.FilePath);

        foreach (var dtoItem in submissionDtos)
        {
            if (submissionDict.TryGetValue(dtoItem.Id, out var filePath))
            {
                dtoItem.FileType = Path.GetExtension((string?) filePath)?.TrimStart('.').ToLower() ?? "unknown";
            }
        }
        
        return ServiceResponseFactory.Success(submissionDtos, Messages.PhaseSubmissionsFetchedSuccessfully);
        
    }

    public async Task<ServiceResponse<FileDownloadDto>> GetExerciseScoreTemplateFile(int exerciseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var exercise = await _exerciseRepository.GetExerciseWithClassRelationsByIdAsync(exerciseId);
        if (exercise==null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ExerciseNotFound);
        }
        
        if (exercise.Class.InstructorId!=user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.CanNotaccessExercise);
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(_exerciseScoreTemplateSettings.WorksheetName);
        worksheet.RightToLeft = true;
    
        worksheet.Cell(1, 1).Value = _exerciseScoreTemplateSettings.ColumnHeaders[0];
        worksheet.Cell(1, 2).Value = _exerciseScoreTemplateSettings.ColumnHeaders[1];
        worksheet.Cell(1, 3).Value = _exerciseScoreTemplateSettings.ColumnHeaders[2];

        var students = exercise.Class.ClassStudents
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

    public async Task<ServiceResponse<string>> UpdateFinalExerciseSubmission(int exerciseSubmissionId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var exerciseSubmission = await _exerciseSubmissionRepository.GetExerciseSubmissionByIdAsync(exerciseSubmissionId);
        if (exerciseSubmission==null)
        {
            return ServiceResponseFactory.Failure<string>(Messages.ExerciseSubmissionNotFound);
        }
        
        if (exerciseSubmission.StudentId!=user!.Student!.Id)
        {
            return ServiceResponseFactory.Failure<string>(Messages.ExerciseSubmissionCanNotBeAccessed);
        }
        
        if (exerciseSubmission.IsFinal)
        {
            return ServiceResponseFactory.Failure<string>(Messages.ExerciseSubmissionMarkedAsFinalAlready);
        }
        
        var currentFinalExerciseSubmission =
            await _exerciseSubmissionRepository.GetFinalExerciseSubmissionsAsync(
                user!.Student!.Id, exerciseSubmission.ExerciseId);
        
        if (currentFinalExerciseSubmission != null)
        {
            currentFinalExerciseSubmission.IsFinal = false;
            await _exerciseSubmissionRepository.UpdateExerciseSubmissionAsync(currentFinalExerciseSubmission);
        }

        exerciseSubmission.IsFinal = true;
        await _exerciseSubmissionRepository.UpdateExerciseSubmissionAsync(exerciseSubmission);

        return ServiceResponseFactory.Success<string>(Messages.ExerciseSubmissionMarkedAsFinalSuccessfully);

    }

    public async Task<ServiceResponse<string>> UpdateExerciseSubmissionScore(int exerciseSubmissionId,
        UpdateExerciseSubmissionScoreDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var exerciseSubmission = await _exerciseSubmissionRepository.GetExerciseSubmissionForInstructorByIdAsync(exerciseSubmissionId);
        if (exerciseSubmission==null)
        {
            return ServiceResponseFactory.Failure<string>(Messages.ExerciseSubmissionNotFound);
        }
        
        if (exerciseSubmission.Exercise.Class.InstructorId!=user!.Instructor!.Id || !exerciseSubmission.IsFinal)
        {
            return ServiceResponseFactory.Failure<string>(Messages.ExerciseSubmissionCanNotBeAccessed);
        }

        if (dto.Score < 0 || dto.Score > exerciseSubmission.Exercise.ExerciseScore)
        {
            return ServiceResponseFactory.Failure<string>(Messages.InvalidScore);
        }

        exerciseSubmission.Score = dto.Score;
        await _exerciseSubmissionRepository.UpdateExerciseSubmissionAsync(exerciseSubmission);

        return ServiceResponseFactory.Success<string>(Messages.ExerciseSubmissionScoreUpdatedSuccessfully);

    }

    // read all once
    public async Task<ServiceResponse<List<GetScoreFileValidationResultDto>>> UpdateExerciseSubmissionScores(int exerciseId, IFormFile scoreFile) // move code to excel file service
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
    
        var exercise = await _exerciseRepository.GetExerciseByIdAsync(exerciseId);
        if (exercise==null)
        {
            return ServiceResponseFactory.Failure<List<GetScoreFileValidationResultDto>>(Messages.ExerciseNotFound);
        }
        
        if (exercise.Class.InstructorId!=user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<List<GetScoreFileValidationResultDto>>(Messages.CanNotaccessExercise);
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
        var exerciseSubmissions = new List<ExerciseSubmission>();
    
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
    
            // check student not in class
            
            var exerciseSubmission = await _exerciseSubmissionRepository.GetExerciseSubmissionByStudentNumber(exerciseId, studentNumber);
            if (exerciseSubmission == null && score > 0 || score < 0 || score > exercise.ExerciseScore)
            {
                errors.Add(Messages.InvalidScore);
            }
            
            validationResults.Add(new GetScoreFileValidationResultDto
            {
                RowNumber = row,
                IsValid = !errors.Any(),
                Errors = errors
            });
    
            if (exerciseSubmission != null)
            {
                exerciseSubmission.Score = score;
                exerciseSubmissions.Add(exerciseSubmission);
            }
            
            row++;
        }
        
        if (validationResults.Any(r => !r.IsValid))
        {
            return ServiceResponseFactory.Failure(validationResults, Messages.ExerciseSubmissionScoresCanNotBeUpdated);
        }
        
        await _exerciseSubmissionRepository.UpdateRangeExerciseSubmissionAsync(exerciseSubmissions);
    
        return ServiceResponseFactory.Success<List<GetScoreFileValidationResultDto>>(Messages.ExerciseSubmissionScoresUpdatedSuccessfully);
    
    }
}