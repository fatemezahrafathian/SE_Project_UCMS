using AutoMapper;
using ClosedXML.Excel;
using Microsoft.Extensions.Options;
using UCMS.DTOs;
using UCMS.DTOs.ExerciseSubmissionDto;
using UCMS.DTOs.PhaseSubmissionDto;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.PhaseRepository.Abstraction;
using UCMS.Repositories.PhaseSubmissionRepository;
using UCMS.Repositories.PhaseSubmissionRepository.Abstraction;
using UCMS.Repositories.StudentTeamPhaseRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.FileService;
using UCMS.Services.TeamPhaseSrvice;

namespace UCMS.Services.PhaseSubmissionSrvice;

public class PhaseSubmissionService: IPhaseSubmissionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPhaseSubmissionRepository _phaseSubmissionRepository;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;
    private readonly IPhaseRepository _phaseRepository;
    private readonly IStudentTeamPhaseRepository _studentTeamPhaseRepository;
    private readonly PhaseScoreTemplateSettings _phaseScoreTemplateSettings;

    public PhaseSubmissionService(IHttpContextAccessor httpContextAccessor, IPhaseSubmissionRepository phaseSubmissionRepository, IFileService fileService, IMapper mapper, IPhaseRepository phaseRepository, IStudentTeamPhaseRepository studentTeamPhaseRepository, IOptions<PhaseScoreTemplateSettings> templateSettingsOptions)
    {
        _httpContextAccessor = httpContextAccessor;
        _phaseSubmissionRepository = phaseSubmissionRepository;
        _fileService = fileService;
        _mapper = mapper;
        _phaseRepository = phaseRepository;
        _studentTeamPhaseRepository = studentTeamPhaseRepository;
        _phaseScoreTemplateSettings = templateSettingsOptions.Value;
    }
    
    public async Task<ServiceResponse<GetPhaseSubmissionPreviewForStudentDto>> CreatePhaseSubmission(int phaseId, CreatePhaseSubmissionDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var phase = await _phaseRepository.GetPhaseSimpleByIdAsync(phaseId);
        if (phase==null)
        {
            return ServiceResponseFactory.Failure<GetPhaseSubmissionPreviewForStudentDto>(Messages.PhaseNotFound);
        }

        var studentTeamPhase = phase.StudentTeamPhases
            .FirstOrDefault(stp=>stp.PhaseId==phaseId && 
                                 stp.StudentTeam.Student.Id==user!.Student!.Id);
        if (studentTeamPhase==null)
        {
            return ServiceResponseFactory.Failure<GetPhaseSubmissionPreviewForStudentDto>(Messages.PhaseCantBeAccessed); // StudentInNoTeamForThisPhase
        }
        
        var validator = new CreatePhaseSubmissionDtoValidator();
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<GetPhaseSubmissionPreviewForStudentDto>(errorMessage);
        }

        if (!_fileService.IsValidExtension(dto.SubmissionFile!, phase.FileFormats))
        {
            return ServiceResponseFactory.Failure<GetPhaseSubmissionPreviewForStudentDto>(Messages.InvalidFormat);
        }

        if (!_fileService.IsValidFileSize(dto.SubmissionFile!))
        {
            return ServiceResponseFactory.Failure<GetPhaseSubmissionPreviewForStudentDto>(Messages.InvalidSize);
        }

        var filePath = await _fileService.SaveFileAsync(dto.SubmissionFile!, "phase-submissions");

        var currentFinalExerciseSubmission =
            await _phaseSubmissionRepository.GetFinalPhaseSubmissionsAsync(
                studentTeamPhase.StudentTeam.TeamId, phaseId);
        
        if (currentFinalExerciseSubmission != null)
        {
            currentFinalExerciseSubmission.IsFinal = false;
            await _phaseSubmissionRepository.UpdatePhaseSubmissionAsync(currentFinalExerciseSubmission);
        }

        var newPhaseSubmission = new PhaseSubmission()
        {
            StudentTeamPhaseId = studentTeamPhase.Id,
            FilePath = filePath,
        };
            
        await _phaseSubmissionRepository.AddPhaseSubmissionAsync(newPhaseSubmission);
        
        var newPhaseSubmissionDto = _mapper.Map<GetPhaseSubmissionPreviewForStudentDto>(newPhaseSubmission);
        newPhaseSubmissionDto.FileType = Path.GetExtension((string?) filePath)?.TrimStart('.').ToLower() ?? "unknown";

        return ServiceResponseFactory.Success(newPhaseSubmissionDto, Messages.PhaseSubmissionCreatedSuccessfully);
    }

    public async Task<ServiceResponse<FileDownloadDto>> GetPhaseSubmissionFileForInstructor(int phaseSubmissionId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var phaseSubmission = await _phaseSubmissionRepository.GetPhaseSubmissionForInstructorByIdAsync(phaseSubmissionId);
        if (phaseSubmission==null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseSubmissionNotFound);
        }
        
        if (phaseSubmission.StudentTeamPhase.Phase.Project.Class.InstructorId!=user!.Instructor!.Id || !phaseSubmission.IsFinal)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseSubmissionCanNotBeAccessed);
        }

        var dto = await _fileService.DownloadFile2(phaseSubmission.FilePath);
        if (dto == null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        }

        return ServiceResponseFactory.Success(dto, Messages.PhaseSubmissionFileFetchedSuccessfully);
    }
    
    public async Task<ServiceResponse<FileDownloadDto>> GetPhaseSubmissionFileForStudent(int phaseSubmissionId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var phaseSubmission = await _phaseSubmissionRepository.GetPhaseSubmissionForStudentByIdAsync(phaseSubmissionId);
        if (phaseSubmission == null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseSubmissionNotFound);
        }
        
        if (!await _studentTeamPhaseRepository.AnyStudentTeamPhaseAsync(user!.Student!.Id, phaseSubmission.StudentTeamPhase.StudentTeam.TeamId, phaseSubmission.StudentTeamPhase.PhaseId))
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseSubmissionCanNotBeAccessed);
        }

        var dto = await _fileService.DownloadFile2(phaseSubmission.FilePath);
        if (dto == null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        }

        return ServiceResponseFactory.Success(dto, Messages.PhaseSubmissionFileFetchedSuccessfully);
    }


    public async Task<ServiceResponse<FileDownloadDto>> GetPhaseSubmissionFiles(int phaseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var phase = await _phaseRepository.GetPhaseByIdAsync(phaseId);
        if (phase == null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseNotFound);
        }
        
        if (phase.Project.Class.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseCantBeAccessed);
        }

        var phaseSubmissions = await _phaseSubmissionRepository.GetPhaseSubmissionsAsync(phaseId);

        var filePaths = phaseSubmissions
            .Select(s => s.FilePath)
            .ToList();

        var zipFile = await _fileService.ZipFiles(filePaths);
        if (zipFile==null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.FileDoesNotExist);
        }

        return ServiceResponseFactory.Success(zipFile, Messages.PhaseSubmissionFilesFetchedSuccessfully);
    }

    public async Task<ServiceResponse<List<GetPhaseSubmissionPreviewForInstructorDto>>> GetPhaseSubmissionsForInstructor(SortPhaseSubmissionsForInstructorDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var phase = await _phaseRepository.GetPhaseByIdAsync(dto.PhaseId);
        if (phase == null)
        {
            return ServiceResponseFactory.Failure<List<GetPhaseSubmissionPreviewForInstructorDto>>(Messages.PhaseNotFound);
        }
        
        if (phase.Project.Class.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<List<GetPhaseSubmissionPreviewForInstructorDto>>(Messages.PhaseCantBeAccessed);
        }

        var validator = new SortPhaseSubmissionForInstructorDtoValidator();
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<List<GetPhaseSubmissionPreviewForInstructorDto>>(errorMessage);
        }

        var phaseSubmissions = await _phaseSubmissionRepository.GetPhaseSubmissionsForInstructorByPhaseIdAsync(dto.PhaseId, dto.SortBy, dto.SortOrder);

        var phaseSubmissionDtos = _mapper.Map<List<GetPhaseSubmissionPreviewForInstructorDto>>(phaseSubmissions);
        
        var submissionDict = phaseSubmissions.ToDictionary(s => s.Id, s => s.FilePath);

        foreach (var dtoItem in phaseSubmissionDtos)
        {
            if (submissionDict.TryGetValue(dtoItem.Id, out var filePath))
            {
                dtoItem.FileType = Path.GetExtension((string?) filePath)?.TrimStart('.').ToLower() ?? "unknown";
            }
        }
        
        return ServiceResponseFactory.Success(phaseSubmissionDtos, Messages.PhaseSubmissionsFetchedSuccessfully);
    }

    public async Task<ServiceResponse<List<GetPhaseSubmissionPreviewForStudentDto>>> GetPhaseSubmissionsForStudent(SortPhaseSubmissionsStudentDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var phase = await _phaseRepository.GetPhaseSimpleByIdAsync(dto.PhaseId);
        if (phase==null)
        {
            return ServiceResponseFactory.Failure<List<GetPhaseSubmissionPreviewForStudentDto>>(Messages.PhaseNotFound);
        }

        var studentTeamPhase = phase.StudentTeamPhases
            .FirstOrDefault(stp=>stp.PhaseId==dto.PhaseId && 
                                 stp.StudentTeam.Student.Id==user!.Student!.Id);
        if (studentTeamPhase==null)
        {
            return ServiceResponseFactory.Failure<List<GetPhaseSubmissionPreviewForStudentDto>>(Messages.PhaseCantBeAccessed); // StudentInNoTeamForThisPhase
        }
        
        var validator = new SortPhaseSubmissionForStudentDtoValidator();
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<List<GetPhaseSubmissionPreviewForStudentDto>>(errorMessage);
        }

        var phaseSubmissions = await _phaseSubmissionRepository.GetPhaseSubmissionsForStudentByPhaseIdAsync(studentTeamPhase.StudentTeam.TeamId, dto.PhaseId, dto.SortBy, dto.SortOrder);

        var phaseSubmissionDtos = _mapper.Map<List<GetPhaseSubmissionPreviewForStudentDto>>(phaseSubmissions);
        
        var submissionDict = phaseSubmissions.ToDictionary(s => s.Id, s => s.FilePath);

        foreach (var dtoItem in phaseSubmissionDtos)
        {
            if (submissionDict.TryGetValue(dtoItem.Id, out var filePath))
            {
                dtoItem.FileType = Path.GetExtension((string?) filePath)?.TrimStart('.').ToLower() ?? "unknown";
            }
        }
        
        return ServiceResponseFactory.Success(phaseSubmissionDtos, Messages.PhaseSubmissionsFetchedSuccessfully);
    }

    public async Task<ServiceResponse<List<GetStudentTeamPhasePreviewDto>>> GetTeamPhaseMembers(int phaseId, int teamId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var phase = await _phaseRepository.GetPhaseByIdAsync(phaseId);
        if (phase == null)
        {
            return ServiceResponseFactory.Failure<List<GetStudentTeamPhasePreviewDto>>(Messages.PhaseNotFound);
        }
        
        if (phase.Project.Class.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<List<GetStudentTeamPhasePreviewDto>>(Messages.PhaseCantBeAccessed);
        }

        var studentTeamPhases = await _studentTeamPhaseRepository.GetStudentTeamPhasesByPhaseAndTeamIdAsync(phaseId, teamId);
        if (studentTeamPhases.Count == 0)
        {
            return ServiceResponseFactory.Failure<List<GetStudentTeamPhasePreviewDto>>(Messages.NoSuchTeamForThisPhase);
        }
        
        var studentTeamPhasesDtos = _mapper.Map<List<GetStudentTeamPhasePreviewDto>>(studentTeamPhases);
        
        return ServiceResponseFactory.Success(studentTeamPhasesDtos, Messages.StudentTeamPhasesFetchedSuccessfully);
        
    }

    public async Task<ServiceResponse<FileDownloadDto>> GetPhaseScoreTemplateFile(int phaseId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var phase = await _phaseRepository.GetPhaseWithRelationsByIdAsync(phaseId);
        
        if (phase == null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseNotFound);
        }
        
        if (phase.Project.Class.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PhaseCantBeAccessed);
        }
        
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(_phaseScoreTemplateSettings.WorksheetName);
        worksheet.RightToLeft = true;
    
        worksheet.Cell(1, 1).Value = _phaseScoreTemplateSettings.ColumnHeaders[0];
        worksheet.Cell(1, 2).Value = _phaseScoreTemplateSettings.ColumnHeaders[1];
        worksheet.Cell(1, 3).Value = _phaseScoreTemplateSettings.ColumnHeaders[2];

        var students = phase.Project.Class.ClassStudents
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
            FileName = _phaseScoreTemplateSettings.FileName,
            ContentType = _phaseScoreTemplateSettings.ContentType,
            FileBytes = fileBytes
        };

        return ServiceResponseFactory.Success(result, Messages.PhaseScoreTemplateFileGeneratedSuccessfully);
    }

    public async Task<ServiceResponse<string>> UpdateFinalPhaseSubmission(int phaseSubmissionId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var phaseSubmission = await _phaseSubmissionRepository.GetPhaseSubmissionForStudentByIdAsync(phaseSubmissionId);
        if (phaseSubmission == null)
        {
            return ServiceResponseFactory.Failure<string>(Messages.PhaseSubmissionNotFound);
        }

        if (!await _studentTeamPhaseRepository.AnyStudentTeamPhaseAsync(user!.Student!.Id, phaseSubmission.StudentTeamPhase.StudentTeam.TeamId, phaseSubmission.StudentTeamPhase.PhaseId))
        {
            return ServiceResponseFactory.Failure<string>(Messages.PhaseSubmissionCanNotBeAccessed);
        }
        
        if (phaseSubmission.IsFinal)
        {
            return ServiceResponseFactory.Failure<string>(Messages.PhaseSubmissionMarkedAsFinalAlready);
        }
        
        var currentFinalPhaseSubmission =
            await _phaseSubmissionRepository.GetFinalPhaseSubmissionsAsync(phaseSubmission.StudentTeamPhase.StudentTeam.TeamId, phaseSubmission.StudentTeamPhase.PhaseId);
        
        if (currentFinalPhaseSubmission != null)
        {
            currentFinalPhaseSubmission.IsFinal = false;
            await _phaseSubmissionRepository.UpdatePhaseSubmissionAsync(currentFinalPhaseSubmission);
        }

        phaseSubmission.IsFinal = true;
        await _phaseSubmissionRepository.UpdatePhaseSubmissionAsync(phaseSubmission);

        return ServiceResponseFactory.Success<string>(Messages.PhaseSubmissionMarkedAsFinalSuccessfully);
    }

    public async Task<ServiceResponse<string>> UpdatePhaseSubmissionScore(int studentTeamPhaseId, UpdatePhaseSubmissionScoreDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var studentTeamPhase = await _studentTeamPhaseRepository.GetStudentTeamPhaseByIdAsync(studentTeamPhaseId);
        if (studentTeamPhase==null)
        {
            return ServiceResponseFactory.Failure<string>(Messages.PhaseSubmissionNotFound);
        }
        
        if (studentTeamPhase.Phase.Project.Class.InstructorId!=user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<string>(Messages.PhaseSubmissionCanNotBeAccessed);
        }

        if (dto.Score < 0 || dto.Score > studentTeamPhase.Phase.PhaseScore)
        {
            return ServiceResponseFactory.Failure<string>(Messages.InvalidScore);
        }
        
        studentTeamPhase.Score = dto.Score;
        await _studentTeamPhaseRepository.UpdateStudentTeamPhaseAsync(studentTeamPhase);

        return ServiceResponseFactory.Success<string>(Messages.StudentTeamPhaseScoreUpdatedSuccessfully);

    }

    public async Task<ServiceResponse<List<GetScoreFileValidationResultDto>>> UpdatePhaseSubmissionScores(int phaseId, IFormFile scoreFile)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
    
        var phase = await _phaseRepository.GetPhaseByIdAsync(phaseId);
        if (phase==null)
        {
            return ServiceResponseFactory.Failure<List<GetScoreFileValidationResultDto>>(Messages.PhaseNotFound);
        }
        
        if (phase.Project.Class.InstructorId!=user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<List<GetScoreFileValidationResultDto>>(Messages.PhaseCantBeAccessed);
        }
    
        XLWorkbook workbook;
        IXLWorksheet worksheet;
    
        try
        {
            workbook = new XLWorkbook(scoreFile.OpenReadStream());
            worksheet = workbook.Worksheet(_phaseScoreTemplateSettings.WorksheetName);
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
                1 => _phaseScoreTemplateSettings.ColumnHeaders[0],
                2 => _phaseScoreTemplateSettings.ColumnHeaders[1],
                _ => _phaseScoreTemplateSettings.ColumnHeaders[2]
            };
    
            var actualHeader = worksheet.Cell(1, i).GetString().Trim();
    
            if (actualHeader != expectedHeader)
            {
                return ServiceResponseFactory.Failure<List<GetScoreFileValidationResultDto>>(Messages.InvalidTemplateFileFormat);
            }
        }
    
        var validationResults = new List<GetScoreFileValidationResultDto>();
        var studentTeamPhases = new List<StudentTeamPhase>();
        var teamToHasPhaseSubmission = new Dictionary<int, bool>();
    
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
            var score = worksheet.Cell(row, 3).GetDouble();  // what if being null
            
            var errors = new List<string>(); 
            
            // check student to be in class 
            
            var studentTeamPhase = await _studentTeamPhaseRepository.GetStudentTeamPhaseByStudentNumber(phaseId, studentNumber);
            if (studentTeamPhase!=null && !teamToHasPhaseSubmission.ContainsKey(studentTeamPhase.StudentTeam.TeamId))
            {
                if (await _phaseSubmissionRepository.AnyPhaseSubmissionForTeam(studentTeamPhase.StudentTeam.TeamId))
                {
                    teamToHasPhaseSubmission.Add(studentTeamPhase.StudentTeam.TeamId, true);
                }
                else
                {
                    teamToHasPhaseSubmission.Add(studentTeamPhase.StudentTeam.TeamId,false);
                }
            }
            
            // separate errors
            if ((studentTeamPhase==null && score>0) || 
                (studentTeamPhase!=null && !teamToHasPhaseSubmission[studentTeamPhase.StudentTeam.TeamId] && score>0) || 
                score < 0 || 
                score > phase.PhaseScore)
            {
                errors.Add(Messages.InvalidScore);
            }
            
            validationResults.Add(new GetScoreFileValidationResultDto
            {
                RowNumber = row,
                IsValid = !errors.Any(),
                Errors = errors
            });
    
            if (studentTeamPhase != null)
            {
                studentTeamPhase.Score = score;
                studentTeamPhases.Add(studentTeamPhase);
            }
            
            row++;
        }
        
        if (validationResults.Any(r => !r.IsValid))
        {
            return ServiceResponseFactory.Failure(validationResults, Messages.PhaseSubmissionScoresCanNotBeUpdated);
        }
        
        await _studentTeamPhaseRepository.UpdateRangeStudentTeamPhaseAsync(studentTeamPhases);
    
        return ServiceResponseFactory.Success<List<GetScoreFileValidationResultDto>>(Messages.ExerciseSubmissionScoresUpdatedSuccessfully);
    
    }
}