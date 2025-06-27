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
using UCMS.Repositories.StudentExamRepository.Abstraction;
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
    private readonly IStudentExamRepository _studentExamRepository;

    public StudentClassService(IClassRepository classRepository, IStudentClassRepository studentClassRepository, IHttpContextAccessor httpContextAccessor, IPasswordService passwordService, IMapper mapper, IStudentTeamPhaseRepository studentTeamPhaseRepository, IExerciseSubmissionRepository exerciseSubmissionRepository, IExamRepository examRepository, IOptions<ScoresSetting> scoresSetting, IStudentExamRepository studentExamRepository)
    {
        _classRepository = classRepository;
        _studentClassRepository = studentClassRepository;
        _httpContextAccessor = httpContextAccessor;
        _passwordService = passwordService;
        _mapper = mapper;
        _studentTeamPhaseRepository = studentTeamPhaseRepository;
        _exerciseSubmissionRepository = exerciseSubmissionRepository;
        _examRepository = examRepository;
        _studentExamRepository = studentExamRepository;
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

    public async Task<ServiceResponse<GetClassStudentsScoresDto>> GetClassStudentsScores(int classId, FilterClassStudentsScoresDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var cls = await _classRepository.FilterClassStudentsWithRelations(classId, dto.FullName, dto.StudentNumber);
        if (cls == null)
        {
            return ServiceResponseFactory.Failure<GetClassStudentsScoresDto>(Messages.ClassNotFound);
        }

        if (cls.InstructorId != user!.Instructor!.Id)
        {
            return ServiceResponseFactory.Failure<GetClassStudentsScoresDto>(Messages.ClassCantBeAccessed);
        }

        // dto validator
        
        var getClassStudentsScoresDto = new GetClassStudentsScoresDto();
        bool firstTime = true;

        foreach (var classStudent in cls.ClassStudents)
        {
            var getClassStudentScoresDto = new GetClassStudentScoresDto()
            {
                StudentId = classStudent.StudentId,
                FullName = classStudent.Student.User.LastName + " " + classStudent.Student.User.FirstName,
                StudentNumber = classStudent.Student.StudentNumber ?? ""
            };

            double total = 0;
            foreach (var project in cls.Projects)
            {
                foreach (var phase in project.Phases)
                {
                    if (phase.PortionInTotalScore == null)
                    {
                        return ServiceResponseFactory.Failure<GetClassStudentsScoresDto>(Messages.PortionInTotalScoreMustBeSetFirst);
                    }
                    
                    var studentTeamPhase = await _studentTeamPhaseRepository.GetStudentTeamPhaseAsync(classStudent.StudentId, phase.Id);
                    if (studentTeamPhase?.Score == null)
                    {
                        getClassStudentScoresDto.Scores.Add(0);
                    }
                    else
                    {
                        var score = (double) studentTeamPhase.Score * (double)phase.PortionInTotalScore / phase.PhaseScore;
                        getClassStudentScoresDto.Scores.Add(Double.Round(score, 2));
                        total += score;
                    }

                    if (firstTime)
                    {
                        getClassStudentsScoresDto.headers.Add(new GetClassEntryPreviewDto()
                        {
                            EntryId = phase.Id,
                            EntryName = phase.Title,
                            EntryType = EntryType.Phase
                        });
                    }
                }
            }

            foreach (var exercise in cls.Exercises)
            {
                if (exercise.PortionInTotalScore == null)
                {
                    return ServiceResponseFactory.Failure<GetClassStudentsScoresDto>(Messages.PortionInTotalScoreMustBeSetFirst);
                }
                
                var exerciseSubmission = await _exerciseSubmissionRepository.GetFinalExerciseSubmissionsAsync(classStudent.StudentId, exercise.Id);
                if (exerciseSubmission?.Score == null)
                {
                    getClassStudentScoresDto.Scores.Add(0);
                }
                else
                {
                    var score = (double) exerciseSubmission.Score * (double) exercise.PortionInTotalScore / exercise.ExerciseScore;
                    getClassStudentScoresDto.Scores.Add(Double.Round(score, 2));
                    total += score;
                }

                if (firstTime)
                {
                    getClassStudentsScoresDto.headers.Add(new GetClassEntryPreviewDto()
                    {
                        EntryId = exercise.Id,
                        EntryName = exercise.Title,
                        EntryType = EntryType.Exercise
                    });
                }
            }
        
            foreach (var exam in cls.Exams)
            {
                if (exam.PortionInTotalScore == null)
                {
                    return ServiceResponseFactory.Failure<GetClassStudentsScoresDto>(Messages.PortionInTotalScoreMustBeSetFirst);
                }

                var studentExam = await _studentExamRepository.GetStudentExamAsync(classStudent.StudentId, exam.Id);
                if (studentExam?.Score == null)
                {
                    getClassStudentScoresDto.Scores.Add(0);
                }
                else
                {
                    var score = (double) studentExam.Score * (double) exam.PortionInTotalScore / exam.ExamScore;
                    getClassStudentScoresDto.Scores.Add(double.Round(score, 2));
                    total += score;
                }
                
                if (firstTime)
                {
                    getClassStudentsScoresDto.headers.Add(new GetClassEntryPreviewDto()
                    {
                        EntryId = exam.Id,
                        EntryName = exam.Title,
                        EntryType = EntryType.Exam
                    });
                }
            }

            getClassStudentScoresDto.total = double.Round(total, 2);

            firstTime = false;
            getClassStudentsScoresDto.ClassStudentScoresDtos.Add(getClassStudentScoresDto);
        }
        
        return ServiceResponseFactory.Success(getClassStudentsScoresDto, Messages.ClassesStudentsScoresFetchedSuccessfully);
    }

    public async Task<ServiceResponse<FileDownloadDto>> GetClassStudentsScoresFile(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var cls = await _classRepository.FilterClassStudentsWithRelations(classId, null, null);
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
        int headerCounter = 3;
        foreach (var classStudent in cls.ClassStudents)
        {
            double total = 0;

            worksheet.Cell(rowCounter, colCounter++).Value = classStudent.Student.User.LastName + " " + classStudent.Student.User.FirstName;
            worksheet.Cell(rowCounter, colCounter++).Value = classStudent.Student.StudentNumber;
            
            foreach (var project in cls.Projects) 
            {
                foreach (var phase in project.Phases)
                {
                    if (phase.PortionInTotalScore == null)
                    {
                        return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PortionInTotalScoreMustBeSetFirst);
                    }
                    
                    var studentTeamPhase = await _studentTeamPhaseRepository.GetStudentTeamPhaseAsync(classStudent.StudentId, phase.Id);
                    if (studentTeamPhase?.Score == null)
                    {
                        worksheet.Cell(rowCounter, colCounter++).Value = 0;
                    }
                    else
                    {
                        var score = (double) studentTeamPhase.Score * (double) phase.PortionInTotalScore / phase.PhaseScore;
                        worksheet.Cell(rowCounter, colCounter++).Value = Double.Round(score, 2);
                        total += score;
                    }

                    if (rowCounter==2)
                    {
                        worksheet.Cell(1, headerCounter++).Value = phase.Title;
                    }
                }
            }

            foreach (var exercise in cls.Exercises)
            {
                if (exercise.PortionInTotalScore == null)
                {
                    return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PortionInTotalScoreMustBeSetFirst);
                }
                
                var exerciseSubmission = await _exerciseSubmissionRepository.GetFinalExerciseSubmissionsAsync(classStudent.StudentId, exercise.Id);
                if (exerciseSubmission?.Score == null)
                {
                    worksheet.Cell(rowCounter, colCounter++).Value = 0;
                }
                else
                {
                    var score = (double) exerciseSubmission.Score * (double) exercise.PortionInTotalScore / exercise.ExerciseScore;
                    worksheet.Cell(rowCounter, colCounter++).Value = Double.Round(score, 2);
                    total += score;
                }

                if (rowCounter==2)
                {
                    worksheet.Cell(1, headerCounter++).Value = exercise.Title;
                }
            }
        
            foreach (var exam in cls.Exams)
            {
                if (exam.PortionInTotalScore == null)
                {
                    return ServiceResponseFactory.Failure<FileDownloadDto>(Messages.PortionInTotalScoreMustBeSetFirst);
                }
                
                var studentExam = await _studentExamRepository.GetStudentExamAsync(classStudent.StudentId, exam.Id);
                if (studentExam?.Score == null)
                {
                    worksheet.Cell(rowCounter, colCounter++).Value = 0;
                }
                else
                {
                    var score = (double) studentExam.Score * (double) exam.PortionInTotalScore / exam.ExamScore;
                    worksheet.Cell(rowCounter, colCounter++).Value = Double.Round(score, 2);
                    total += score;
                }
                
                if (rowCounter==2)
                {
                    worksheet.Cell(1, headerCounter++).Value = exam.Title;
                }
            }

            if (rowCounter==2)
            {
                worksheet.Cell(1, headerCounter++).Value = _scoresSetting.ColumnHeaders[2];
            }
            worksheet.Cell(rowCounter, colCounter).Value = double.Round(total, 2);

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

    public async Task<ServiceResponse<List<GetStudentClassScoreDto>>> GetStudentClassesScores(FilterStudentClassesScoresDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;

        var classes = await _classRepository.FilterStudentClassesWithRelations(user!.Student!.Id, dto.Title);
        if (!classes.Any())
        {
            return ServiceResponseFactory.Failure<List<GetStudentClassScoreDto>>(Messages.NoClassFound);
        }
        // dto validator
        var result = new List<GetStudentClassScoreDto>();
        foreach (var cls in classes)
        {
            var getStudentClassScoreDto = new GetStudentClassScoreDto()
            {
                ClassId = cls.Id,
                ClassTitle = cls.Title,
            };

            double scoreCounter = 0;
            foreach (var project in cls.Projects)
            {
                foreach (var phase in project.Phases)
                {
                    if (phase.PortionInTotalScore == null)
                    {
                        return ServiceResponseFactory.Failure<List<GetStudentClassScoreDto>>(Messages.PortionInTotalScoreMustBeSetFirst);
                    }

                    var studentTeamPhase = await _studentTeamPhaseRepository.GetStudentTeamPhaseAsync(user.Student.Id, phase.Id);
                    if (studentTeamPhase?.Score == null)
                    {
                        scoreCounter += 0;
                    }
                    else
                    {
                        var score = (double) studentTeamPhase.Score * (double) phase.PortionInTotalScore / phase.PhaseScore;
                        scoreCounter += score;
                    }
                }
            }

            foreach (var exercise in cls.Exercises)
            {
                if (exercise.PortionInTotalScore == null)
                {
                    return ServiceResponseFactory.Failure<List<GetStudentClassScoreDto>>(Messages.PortionInTotalScoreMustBeSetFirst);
                }

                var exerciseSubmission = await _exerciseSubmissionRepository.GetFinalExerciseSubmissionsAsync(user.Student.Id, exercise.Id);
                if (exerciseSubmission?.Score == null)
                {
                    scoreCounter += 0;
                }
                else
                {
                    var score = (double) exerciseSubmission.Score * (double) exercise.PortionInTotalScore / exercise.ExerciseScore;
                    scoreCounter += score;
                }
            }
        
            foreach (var exam in cls.Exams)
            {
                if (exam.PortionInTotalScore == null)
                {
                    return ServiceResponseFactory.Failure<List<GetStudentClassScoreDto>>(Messages.PortionInTotalScoreMustBeSetFirst);
                }
                
                var studentExam = await _studentExamRepository.GetStudentExamAsync(user.Student.Id, exam.Id);
                if (studentExam?.Score == null)
                {
                    scoreCounter += 0;
                }
                else
                {
                    var score = (double) studentExam.Score * (double) exam.PortionInTotalScore / exam.ExamScore;
                    scoreCounter += score;
                }
            }

            getStudentClassScoreDto.Score = Double.Round(scoreCounter, 2);
            result.Add(getStudentClassScoreDto);

        }
        
        return ServiceResponseFactory.Success(result, Messages.StudentClassesScoresfetchedSuccessfully);

    }

    public async Task<ServiceResponse<List<GetStudentClassEntityScoreDto>>> GetStudentClassScores(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var cls = await _classRepository.GetClassWithRelationsAsync(user!.Student!.Id, classId);
        // check for null
        // check access to class
        var result = new List<GetStudentClassEntityScoreDto>();

        foreach (var project in cls.Projects)
        {
            foreach (var phase in project.Phases)
            {
                if (phase.PortionInTotalScore == null)
                {
                    return ServiceResponseFactory.Failure<List<GetStudentClassEntityScoreDto>>(Messages.PortionInTotalScoreMustBeSetFirst);
                }

                var getStudentClassEntityScoreDto = new GetStudentClassEntityScoreDto()
                {
                    EntryId = phase.Id,
                    EntryType = EntryType.Phase,
                    EntryName = phase.Title,
                    TotalScore = phase.PhaseScore,
                    PartialScore = (double) phase.PortionInTotalScore
                };
                
                var studentTeamPhase = await _studentTeamPhaseRepository.GetStudentTeamPhaseAsync(user.Student.Id, phase.Id);
                if (studentTeamPhase?.Score == null)
                {
                    getStudentClassEntityScoreDto.ScoreInTotalScore = 0;
                    getStudentClassEntityScoreDto.ScoreInPartialScore = 0;
                }
                else
                {
                    getStudentClassEntityScoreDto.ScoreInTotalScore = (double)studentTeamPhase.Score;
                    var score = (double) studentTeamPhase.Score * (double) phase.PortionInTotalScore / phase.PhaseScore;
                    getStudentClassEntityScoreDto.PartialScore = Double.Round(score, 2);
                }
                
                result.Add(getStudentClassEntityScoreDto);
            }
        }

        foreach (var exercise in cls.Exercises)
        {
            if (exercise.PortionInTotalScore == null)
            {
                return ServiceResponseFactory.Failure<List<GetStudentClassEntityScoreDto>>(Messages.PortionInTotalScoreMustBeSetFirst);
            }
            
            var getStudentClassEntityScoreDto = new GetStudentClassEntityScoreDto()
            {
                EntryId = exercise.Id,
                EntryType = EntryType.Exercise,
                EntryName = exercise.Title,
                TotalScore = exercise.ExerciseScore,
                PartialScore = (double) exercise.PortionInTotalScore
            };

            var exerciseSubmission = await _exerciseSubmissionRepository.GetFinalExerciseSubmissionsAsync(user.Student.Id, exercise.Id);
            if (exerciseSubmission?.Score == null)
            {
                getStudentClassEntityScoreDto.ScoreInTotalScore = 0;
                getStudentClassEntityScoreDto.ScoreInPartialScore = 0;
            }
            else
            {
                getStudentClassEntityScoreDto.ScoreInTotalScore = (double)exerciseSubmission.Score;
                var score = (double) exerciseSubmission.Score * (double) exercise.PortionInTotalScore / exercise.ExerciseScore;
                getStudentClassEntityScoreDto.PartialScore = Double.Round(score, 2);
            }
            
            result.Add(getStudentClassEntityScoreDto);
        }
        
        foreach (var exam in cls.Exams)
        {
            if (exam.PortionInTotalScore == null)
            {
                return ServiceResponseFactory.Failure<List<GetStudentClassEntityScoreDto>>(Messages.PortionInTotalScoreMustBeSetFirst);
            }

            var getStudentClassEntityScoreDto = new GetStudentClassEntityScoreDto()
            {
                EntryId = exam.Id,
                EntryType = EntryType.Exam,
                EntryName = exam.Title,
                TotalScore = exam.ExamScore,
                PartialScore = (double) exam.PortionInTotalScore
            };

            var studentExam = await _studentExamRepository.GetStudentExamAsync(user.Student.Id, exam.Id);
            if (studentExam?.Score == null)
            {
                getStudentClassEntityScoreDto.ScoreInTotalScore = 0;
                getStudentClassEntityScoreDto.ScoreInPartialScore = 0;
            }
            else
            {
                getStudentClassEntityScoreDto.ScoreInTotalScore = (double)studentExam.Score;
                var score = (double) studentExam.Score * (double) exam.PortionInTotalScore / exam.ExamScore;
                getStudentClassEntityScoreDto.PartialScore = Double.Round(score, 2);
            }

            result.Add(getStudentClassEntityScoreDto);
        }

        return ServiceResponseFactory.Success(result, Messages.StudentClassScoresFetcehedSuccessfully);

    }
}