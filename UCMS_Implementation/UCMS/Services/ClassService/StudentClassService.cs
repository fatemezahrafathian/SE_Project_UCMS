using AutoMapper;
using ClosedXML.Excel;
using Microsoft.Extensions.Options;
using UCMS.DTOs;
using UCMS.DTOs.ClassDto;
using UCMS.Extensions;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.ExamRepository.Abstraction;
using UCMS.Repositories.ExerciseSubmissionRepository.Abstraction;
using UCMS.Repositories.StudentTeamPhaseRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.ClassService.Abstraction;
using UCMS.Services.PasswordService.Abstraction;

namespace UCMS.Services.ClassService;

public class StudentClassService: IStudentClassService
{
    private readonly IClassRepository _classRepository;
    private readonly IStudentClassRepository _studentClassRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly IStudentTeamPhaseRepository _studentTeamPhaseRepository;
    private readonly IExerciseSubmissionRepository _exerciseSubmissionRepository;
    private readonly IExamRepository _examRepository;
    private readonly ScoresSetting _scoresSetting;

    public StudentClassService(IClassRepository classRepository, IStudentClassRepository studentClassRepository, IHttpContextAccessor httpContextAccessor, IPasswordService passwordService, IMapper mapper, IStudentTeamPhaseRepository studentTeamPhaseRepository, IExerciseSubmissionRepository exerciseSubmissionRepository, IExamRepository examRepository, IOptions<ScoresSetting> scoresSetting)
    {
        _classRepository = classRepository;
        _studentClassRepository = studentClassRepository;
        _httpContextAccessor = httpContextAccessor;
        _passwordService = passwordService;
        _mapper = mapper;
        _studentTeamPhaseRepository = studentTeamPhaseRepository;
        _exerciseSubmissionRepository = exerciseSubmissionRepository;
        _examRepository = examRepository;
        _scoresSetting = scoresSetting.Value;
    }

    private async Task<bool> IsStudentOfClass(int classId, int studentId)
    {
        return await _studentClassRepository.IsStudentOfClassAsync(classId, studentId);
    }
    public async Task<ServiceResponse<JoinClassResponseDto>> JoinClassAsync(JoinClassRequestDto request)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var classEntity = await _classRepository.GetClassByTokenAsync(request.ClassCode);
        
        if (classEntity == null)
        {
            return new ServiceResponse<JoinClassResponseDto>
            {
                Success = false,
                Message = Messages.ClassNotFound
            };
        }
        if (!await _passwordService.VerifyPasswordAsync(request.Password, classEntity.PasswordSalt, classEntity.PasswordHash))
            return new ServiceResponse<JoinClassResponseDto> { Success = false, Message = Messages.WrongPasswordMessage };
        var alreadyJoined = await IsStudentOfClass(classEntity.Id, user.Student.Id);
        if (alreadyJoined)
        {
            return new ServiceResponse<JoinClassResponseDto>
            {
                Success = false,
                Message = Messages.AlreadyJoinedClass
            };
        }
        
        var now = DateOnly.FromDateTime(DateTime.Now);
        if (classEntity.StartDate.HasValue)
        {
            if (now < classEntity.StartDate)
            {
                return new ServiceResponse<JoinClassResponseDto>
                {
                    Success = false,
                    Message = Messages.ClassCurrentlyNotActive
                };
            }
        }
        if (classEntity.EndDate.HasValue)
        {
            if (now > classEntity.EndDate)
            {
                return new ServiceResponse<JoinClassResponseDto>
                {
                    Success = false,
                    Message = Messages.ClassCurrentlyNotActive
                };
            }
        }
        

        await _studentClassRepository.AddStudentToClassAsync(classEntity.Id, user.Student.Id);
        
        return new ServiceResponse<JoinClassResponseDto>
        {
            Success = true,
            Message = Messages.ClassJoinedSuccessfully,
            Data = new JoinClassResponseDto(){classId = classEntity.Id}
        };
    }
    
    public async Task<ServiceResponse<bool>> LeaveClassAsync(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var classEntity = await _classRepository.GetClassByIdAsync(classId);
        if (classEntity == null)
            return new ServiceResponse<bool> { Success = false, Message = Messages.ClassNotFound };
        
        var isStudentOfClass = await IsStudentOfClass(classEntity.Id, user.Student.Id);
        if (!isStudentOfClass)
        {
            return new ServiceResponse<bool>
            {
                Success = false,
                Message = Messages.StudentNotInClass
            };
        }

        var success = await _studentClassRepository.RemoveStudentFromClassAsync(classId, user.Student.Id);
        if (!success)
            return new ServiceResponse<bool> { Success = false, Message = Messages.LeftClassNotSuccessfully };

        return new ServiceResponse<bool> { Success = true, Message = Messages.LeftClassSuccessfully };
    }

    public async Task<ServiceResponse<bool>> RemoveStudentFromClassAsync(int classId, int StudentId)
    {
        var instructor = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var classEntity = await _classRepository.GetClassByIdAsync(classId);
        if (classEntity == null)
            return new ServiceResponse<bool> { Success = false, Message = Messages.ClassNotFound };

        if (instructor.Id != classEntity.InstructorId)
        {
            return new ServiceResponse<bool>
            {
                Success = false,
                Message = Messages.UnauthorizedAccess
            };
        }

        var isStudentOfClass = await IsStudentOfClass(classEntity.Id, StudentId);
        if (!isStudentOfClass)
        {
            return new ServiceResponse<bool>
            {
                Success = false,
                Message = Messages.StudentNotInClass
            };
        }

        var success = await _studentClassRepository.RemoveStudentFromClassAsync(classId, StudentId);
        if (!success)
            return new ServiceResponse<bool>
                { Success = false, Message = Messages.RemoveStudentFromClassNotSuccessfully };

        return new ServiceResponse<bool> { Success = true, Message = Messages.RemoveStudentFromClassSuccessfully };
    }
    
    public async Task<ServiceResponse<List<GetStudentsOfClassforInstructorDto>>> GetStudentsOfClassByInstructorAsync(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var classEntity = await _classRepository.GetClassByIdAsync(classId);
        if (classEntity == null)
            return new ServiceResponse<List<GetStudentsOfClassforInstructorDto>> { Success = false, Message = Messages.ClassNotFound };

        if (classEntity.Instructor.UserId != user.Id)
        {
            return new ServiceResponse<List<GetStudentsOfClassforInstructorDto>>
            {
                Success = false,
                Message = Messages.UnauthorizedAccess
            };
        }
        var students = await _studentClassRepository.GetStudentsInClassAsync(classId);
        var dtoList = _mapper.Map<List<GetStudentsOfClassforInstructorDto>>(students);
        return new ServiceResponse<List<GetStudentsOfClassforInstructorDto>>
        {
            Success = true,
            Message = Messages.ListOfStudent,
            Data = dtoList
        };
    }
    public async Task<ServiceResponse<List<GetStudentsOfClassforStudentDto>>> GetStudentsOfClassByStudentAsync(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var classEntity = await _classRepository.GetClassByIdAsync(classId);
        if (classEntity == null)
            return new ServiceResponse<List<GetStudentsOfClassforStudentDto>> { Success = false, Message = Messages.ClassNotFound };

        var isStudentOfClass = await IsStudentOfClass(classEntity.Id, user.Student.Id);
        if (!isStudentOfClass)
        {
            return new ServiceResponse<List<GetStudentsOfClassforStudentDto>>
            {
                Success = false,
                Message = Messages.StudentNotInClass
            };
        }
        var students = await _studentClassRepository.GetStudentsInClassAsync(classId);
        var dtoList = _mapper.Map<List<GetStudentsOfClassforStudentDto>>(students);
        return new ServiceResponse<List<GetStudentsOfClassforStudentDto>>
        {
            Success = true,
            Message = Messages.ListOfStudent,
            Data = dtoList
        };
    }

    public async Task<int> GetStudentClassCount(int classId)
    {
        var classEntity = await _classRepository.GetClassByIdAsync(classId);
        return classEntity.ClassStudents.Count;
    }
    public async Task<ServiceResponse<GetClassForStudentDto>> GetClassForStudent(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        
        var classEntity = await _classRepository.GetStudentClassByClassIdAsync(classId);
        if (classEntity == null)
        {
            return ServiceResponseFactory.Failure<GetClassForStudentDto>(
                Messages.ClassNotFound);
        }

        var isStudentOfClass = await IsStudentOfClass(classId, user!.Student!.Id);
        if (!isStudentOfClass)
        {
            return new ServiceResponse<GetClassForStudentDto> 
            {
                Success = false,
                Message = Messages.ClassCan_tBeAccessed
            };
        }

        var responseDto = _mapper.Map<GetClassForStudentDto>(classEntity);
        responseDto.StudentCount = await GetStudentClassCount(classEntity.Id);
        return ServiceResponseFactory.Success(responseDto, Messages.ClassFetchedSuccessfully);
    }
    public async Task<ServiceResponse<GetClassPageForStudentDto>> GetClassesForStudent(PaginatedFilterClassForStudentDto dto) // search
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var classEntityQueryable = _studentClassRepository.FilterStudentClassesByStudentIdAsync(user!.Student!.Id, dto.Title, dto.IsActive);        
        var classEntityList = await classEntityQueryable.PaginateAsync(dto.Page, dto.PageSize);

        var responseDto = _mapper.Map<GetClassPageForStudentDto>(classEntityList);
        
        return ServiceResponseFactory.Success(responseDto, Messages.ClassesRetrievedSuccessfully); // ClassesFetchedSuccessfully
    }

    public async Task<ServiceResponse<GetClassStudentsScoresDto>> GetClassStudentsScores(int classId, SearchClassStudentsScoresDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var cls = await _classRepository.GetClassWithRelationsByIdAsync(classId);  // sort by last name
        if (cls == null)
        {
            return ServiceResponseFactory.Failure<GetClassStudentsScoresDto>(Messages.ClassNotFound);
        }

        if (cls.InstructorId != user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<GetClassStudentsScoresDto>(Messages.ClassCantBeAccessed);
        }

        var getClassStudentsScoresDto = new GetClassStudentsScoresDto();
        bool firstTime = true;
        
        foreach (var classStudent in cls.ClassStudents)
        {
            var getClassStudentScoresDto = new GetClassStudentScoresDto()
            {
                StudentId = classStudent.StudentId,
                FullName = classStudent.Student.User.LastName + " " + classStudent.Student.User.FirstName,
                StudentNumber = classStudent.Student.StudentNumber
            };
            
            foreach (var project in cls.Projects) // use select instead
            {
                foreach (var phase in project.Phases)
                {
                    var studentTeamPhase = await _studentTeamPhaseRepository.GetStudentTeamPhaseAsync(classStudent.StudentId, phase.Id);
                    if (studentTeamPhase == null || studentTeamPhase.Score == null)
                    {
                        getClassStudentScoresDto.Scores.Add(0);
                    }
                    else
                    {
                        getClassStudentScoresDto.Scores.Add((double) studentTeamPhase.Score); // calculate score
                    }

                    if (firstTime)
                    {
                        getClassStudentsScoresDto.headers.Add(phase.Title); // change it dto
                    }
                }
            }

            foreach (var exercise in cls.Exercises)
            {
                var exerciseSubmission = await _exerciseSubmissionRepository.GetFinalExerciseSubmissionsAsync(classStudent.StudentId, exercise.Id);
                if (exerciseSubmission == null || exerciseSubmission.Score == null)
                {
                    getClassStudentScoresDto.Scores.Add(0);
                }
                else
                {
                    getClassStudentScoresDto.Scores.Add((double) exerciseSubmission.Score);
                }

                if (firstTime)
                {
                    getClassStudentsScoresDto.headers.Add(exercise.Title);
                }
            }
        
            foreach (var exam in cls.Exams)
            {
                var studentExam = await _examRepository.GetStudentExamAsync(classStudent.StudentId, exam.Id);
                if (studentExam == null || studentExam.Score == null)
                {
                    getClassStudentScoresDto.Scores.Add(0);
                }
                else
                {
                    getClassStudentScoresDto.Scores.Add((double) studentExam.Score);
                }
                
                if (firstTime)
                {
                    getClassStudentsScoresDto.headers.Add(exam.Title);
                }
            }

            firstTime = false;
            getClassStudentsScoresDto.ClassStudentScoresDtos.Add(getClassStudentScoresDto);
        }
        
        return ServiceResponseFactory.Success(getClassStudentsScoresDto, Messages.ClassesStudentsScoresFetchedSuccessfully);
    }

    public async Task<ServiceResponse<FileDownloadDto>> GetClassStudentsScoresFile(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var cls = await _classRepository.GetClassWithRelationsByIdAsync(classId);
        if (cls == null)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ClassNotFound);
        }

        if (cls.InstructorId != user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.ClassCantBeAccessed);
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(_scoresSetting.WorksheetName);
        worksheet.RightToLeft = true;
    
        worksheet.Cell(1, 1).Value = _scoresSetting.ColumnHeaders[0];
        worksheet.Cell(1, 2).Value = _scoresSetting.ColumnHeaders[1];

        int rowCounter = 2;
        int colCounter = 1;
        int headerCounter = 2;
        foreach (var classStudent in cls.ClassStudents)
        {
            worksheet.Cell(rowCounter, colCounter++).Value = classStudent.Student.User.LastName + " " + classStudent.Student.User.FirstName;
            worksheet.Cell(rowCounter, colCounter++).Value = classStudent.Student.StudentNumber;
            
            foreach (var project in cls.Projects) // use select instead
            {
                foreach (var phase in project.Phases)
                {
                    var studentTeamPhase = await _studentTeamPhaseRepository.GetStudentTeamPhaseAsync(classStudent.StudentId, phase.Id);
                    if (studentTeamPhase == null || studentTeamPhase.Score == null)
                    {
                        worksheet.Cell(rowCounter, colCounter++).Value = 0;
                    }
                    else
                    {
                        worksheet.Cell(rowCounter, colCounter++).Value = studentTeamPhase.Score;
                    }

                    if (rowCounter==2)
                    {
                        worksheet.Cell(1, headerCounter++).Value = phase.Title;
                    }
                }
            }

            foreach (var exercise in cls.Exercises)
            {
                var exerciseSubmission = await _exerciseSubmissionRepository.GetFinalExerciseSubmissionsAsync(classStudent.StudentId, exercise.Id);
                if (exerciseSubmission == null || exerciseSubmission.Score == null)
                {
                    worksheet.Cell(rowCounter, colCounter++).Value = 0;
                }
                else
                {
                    worksheet.Cell(rowCounter, colCounter++).Value = exerciseSubmission.Score;
                }

                if (rowCounter==2)
                {
                    worksheet.Cell(1, headerCounter++).Value = exercise.Title;
                }
            }
        
            foreach (var exam in cls.Exams)
            {
                var studentExam = await _examRepository.GetStudentExamAsync(classStudent.StudentId, exam.Id);
                if (studentExam == null || studentExam.Score == null)
                {
                    worksheet.Cell(rowCounter, colCounter++).Value = 0;
                }
                else
                {
                    worksheet.Cell(rowCounter, colCounter++).Value = studentExam.Score;
                }
                
                if (rowCounter==2)
                {
                    worksheet.Cell(1, headerCounter++).Value = exam.Title;
                }
            }

            rowCounter++;
            colCounter = 1;
        }
        
        await using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Seek(0, SeekOrigin.Begin);

        var fileBytes = stream.ToArray();

        var result = new FileDownloadDto()
        {
            FileName = _scoresSetting.FileName,
            ContentType = _scoresSetting.ContentType,
            FileBytes = fileBytes
        };

        return ServiceResponseFactory.Success(result, Messages.ClassesStudentsScoresFileGeneratedSuccessfully);
    }

    public async Task<ServiceResponse<List<GetStudentClassScoreDto>>> GetStudentClassesScores(SearchStudentClassesScoresDto dto) // search
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var classes = await _classRepository.GetClassesWithRelationsAsync(user!.Student!.Id);
        // check count
        var result = new List<GetStudentClassScoreDto>();
        foreach (var cls in classes)
        {
            var getStudentClassScoreDto = new GetStudentClassScoreDto()
            {
                ClassId = cls.Id,
                ClassTitle = cls.Title,
            };

            double scoreCounter = 0;
            foreach (var project in cls.Projects) // use select instead
            {
                foreach (var phase in project.Phases)
                {
                    var studentTeamPhase = await _studentTeamPhaseRepository.GetStudentTeamPhaseAsync(user.Student.Id, phase.Id);
                    if (studentTeamPhase == null || studentTeamPhase.Score == null)
                    {
                        scoreCounter += 0;
                    }
                    else
                    {
                        scoreCounter += (double) studentTeamPhase.Score;  // calculate
                    }
                }
            }

            foreach (var exercise in cls.Exercises)
            {
                var exerciseSubmission = await _exerciseSubmissionRepository.GetFinalExerciseSubmissionsAsync(user.Student.Id, exercise.Id);
                if (exerciseSubmission == null || exerciseSubmission.Score == null)
                {
                    scoreCounter += 0;
                }
                else
                {
                    scoreCounter += (double) exerciseSubmission.Score;  // calculate
                }
            }
        
            foreach (var exam in cls.Exams)
            {
                var studentExam = await _examRepository.GetStudentExamAsync(user.Student.Id, exam.Id);
                if (studentExam == null || studentExam.Score == null)
                {
                    scoreCounter += 0;
                }
                else
                {
                    scoreCounter += (double) studentExam.Score;  // calculate
                }
            }

            getStudentClassScoreDto.Score = scoreCounter;
            result.Add(getStudentClassScoreDto);

        }
        
        return ServiceResponseFactory.Success(result, Messages.StudentClassesScoresfetchedSuccessfully);

    }

    public async Task<ServiceResponse<List<GetStudentClassEntityScoreDto>>> GetStudentClassScores(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var cls = await _classRepository.GetClassWithRelationsAsync(user!.Student!.Id, classId);
        // check for null
        var result = new List<GetStudentClassEntityScoreDto>();

        foreach (var project in cls.Projects) // use select instead
        {
            foreach (var phase in project.Phases)
            {
                var getStudentClassEntityScoreDto = new GetStudentClassEntityScoreDto()
                {
                    EntryId = phase.Id,
                    EntryType = EntryType.Phase,
                    EntryName = phase.Title
                };
                
                var studentTeamPhase = await _studentTeamPhaseRepository.GetStudentTeamPhaseAsync(user.Student.Id, phase.Id);
                if (studentTeamPhase == null || studentTeamPhase.Score == null)
                {
                    getStudentClassEntityScoreDto.PartialScore = 0;
                }
                else
                {
                    getStudentClassEntityScoreDto.PartialScore = (double) studentTeamPhase.Score;  // calculate
                }
                
                result.Add(getStudentClassEntityScoreDto);
            }
        }

        foreach (var exercise in cls.Exercises)
        {
            var getStudentClassEntityScoreDto = new GetStudentClassEntityScoreDto()
            {
                EntryId = exercise.Id,
                EntryType = EntryType.Exercise,
                EntryName = exercise.Title
            };

            var exerciseSubmission = await _exerciseSubmissionRepository.GetFinalExerciseSubmissionsAsync(user.Student.Id, exercise.Id);
            if (exerciseSubmission == null || exerciseSubmission.Score == null)
            {
                getStudentClassEntityScoreDto.PartialScore = 0;
            }
            else
            {
                getStudentClassEntityScoreDto.PartialScore = (double) exerciseSubmission.Score;  // calculate
            }
            
            result.Add(getStudentClassEntityScoreDto);
        }
        
        foreach (var exam in cls.Exams)
        {
            var getStudentClassEntityScoreDto = new GetStudentClassEntityScoreDto()
            {
                EntryId = exam.Id,
                EntryType = EntryType.Exam,
                EntryName = exam.Title
            };

            var studentExam = await _examRepository.GetStudentExamAsync(user.Student.Id, exam.Id);
            if (studentExam == null || studentExam.Score == null)
            {
                getStudentClassEntityScoreDto.PartialScore = 0;
            }
            else
            {
                getStudentClassEntityScoreDto.PartialScore = (double) studentExam.Score;  // calculate
            }

            result.Add(getStudentClassEntityScoreDto);
        }

        return ServiceResponseFactory.Success(result, Messages.StudentClassScoresFetcehedSuccessfully);

    }
}