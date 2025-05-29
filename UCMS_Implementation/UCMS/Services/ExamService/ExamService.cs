using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.ExamDto;
using UCMS.Factories;
using UCMS.Models;
using UCMS.Repositories.ClassRepository.Abstraction;
using UCMS.Repositories.ExamRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.ExamService.Abstraction;
using UCMS.Services.FileService;

namespace UCMS.Services.ExamService;

public class ExamService:IExamService
{
     private readonly IExamRepository  _repository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IClassRepository _classRepository;
    private readonly IFileService _fileService;
    private readonly IStudentClassRepository _studentClassRepository;

    public ExamService(IExamRepository repository, IMapper mapper,IHttpContextAccessor httpContextAccessor,IClassRepository classRepository,IFileService fileService,IStudentClassRepository studentClassRepository)
    {
        _repository = repository;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _classRepository = classRepository;
        _fileService = fileService;
        _studentClassRepository = studentClassRepository;
    }

    public async Task<ServiceResponse<GetExamForInstructorDto>> CreateExamAsync(int classId,CreateExamDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var currentClass = await _classRepository.GetClassByIdAsync(classId);
        if (currentClass == null)
        {
            return ServiceResponseFactory.Failure<GetExamForInstructorDto>(Messages.ClassNotFound);
        }
        if (currentClass.InstructorId != user.Instructor.Id)
        {
            return ServiceResponseFactory.Failure<GetExamForInstructorDto>(Messages.InvalidInstructorForThisClass);
        }
        var validator = new CreateExamDtoValidator();
        var result = await validator.ValidateAsync(dto);
        var existingExams = await _repository.GetExamsByClassIdAsync(currentClass.Id);
        if (existingExams.Any(p => p.Title.Trim().ToLower() == dto.Title.Trim().ToLower()))
        {
            return ServiceResponseFactory.Failure<GetExamForInstructorDto>(Messages.ExamAlreadyExists);
        }
        if (!result.IsValid)
        {
            var errorMessage = result.Errors.First().ErrorMessage;
            return ServiceResponseFactory.Failure<GetExamForInstructorDto>(errorMessage);
        }
        var newExam = _mapper.Map<Exam>(dto);
        newExam.ClassId=currentClass.Id;
        await _repository.AddAsync(newExam);
        var phaseDto = _mapper.Map<GetExamForInstructorDto>(newExam);
        return ServiceResponseFactory.Success(phaseDto, Messages.ExamCreatedSuccessfully);
    }

    public async Task<ServiceResponse<GetExamForInstructorDto>> GetExamByIdForInstructorAsync(int examId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var exam = await _repository.GetExamByIdAsync(examId);
        
        if (exam == null || exam.Class.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<GetExamForInstructorDto>(Messages.ExamCantBeAccessed);
        }

        var dto = _mapper.Map<GetExamForInstructorDto>(exam);

        return ServiceResponseFactory.Success(dto, Messages.ExamRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<GetExamForInstructorDto>> UpdateExamAsync(int examId, PatchExamDto dto)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var existingExam = await _repository.GetExamByIdAsync(examId);
        if (existingExam == null || existingExam.Class.InstructorId !=  user?.Instructor?.Id)
            return ServiceResponseFactory.Failure<GetExamForInstructorDto>(Messages.ExamCantBeAccessed);

        var validator = new UpdateExamDtoValidator();
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return ServiceResponseFactory.Failure<GetExamForInstructorDto>(validationResult.Errors.First().ErrorMessage);
        
        var isDuplicate = await _repository.ExistsWithTitleExceptIdAsync(dto.Title, existingExam.ClassId, examId);
        if (isDuplicate)
            return ServiceResponseFactory.Failure<GetExamForInstructorDto>(Messages.ExamAlreadyExists);

        existingExam = _mapper.Map(dto, existingExam);
        existingExam.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existingExam);

        var phaseDto = _mapper.Map<GetExamForInstructorDto>(existingExam);
        return ServiceResponseFactory.Success(phaseDto, Messages.ExamUpdatedSuccessfully);
    }

    public async Task<ServiceResponse<string>>  DeleteExamAsync(int examId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var exam = await _repository.GetExamByIdAsync(examId);
        if (exam.Class.InstructorId != user!.Instructor!.Id)
            return ServiceResponseFactory.Failure<string>(Messages.ExamCantBeAccessed);
        await _repository.DeleteAsync(exam);
        return ServiceResponseFactory.Success("Exam deleted successfully", Messages.ExamDeletedSuccessfully);
    }
    public async Task<ServiceResponse<List<GetExamForInstructorDto>>> GetExamsForInstructor(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var currentclass = await _classRepository.GetClassByIdAsync(classId);

        if (currentclass == null || currentclass.InstructorId != user?.Instructor?.Id)
        {
            return ServiceResponseFactory.Failure<List<GetExamForInstructorDto>>(Messages.ProjectCantBeAccessed);
        }
        var exams = await _repository.GetExamsByClassIdAsync(classId);
        var dto =  _mapper.Map<List<GetExamForInstructorDto>>(exams);
        return ServiceResponseFactory.Success(dto,Messages.ExamsRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<GetExamForStudentDto>> GetExamByIdForStudentAsync(int examId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var exam = await _repository.GetExamByIdAsync(examId);
        
        if (exam == null || user==null || user.Student==null)
        {
            return ServiceResponseFactory.Failure<GetExamForStudentDto>(Messages.ExamCantBeAccessed);
        }
        
        if (!await _studentClassRepository.IsStudentOfClassAsync(exam.ClassId,user.Student.Id))
        {
            return ServiceResponseFactory.Failure<GetExamForStudentDto>(Messages.ExamCantBeAccessed);
        }

        var dto = _mapper.Map<GetExamForStudentDto>(exam);

        return ServiceResponseFactory.Success(dto, Messages.ExamRetrievedSuccessfully);
    }
    public async Task<ServiceResponse<List<GetExamForStudentDto>>> GetExamsForStudent(int classId)
    {
        var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
        var currentClass = await _classRepository.GetClassByIdAsync(classId);

        if (currentClass == null || user==null || user.Student==null || !await _studentClassRepository.IsStudentOfClassAsync(classId,user.Student.Id))
        {
            return ServiceResponseFactory.Failure<List<GetExamForStudentDto>>(Messages.ClassCantBeAccessed);
        }
        var exams = await _repository.GetExamsByClassIdAsync(classId);
        var dto =  _mapper.Map<List<GetExamForStudentDto>>(exams);
        return ServiceResponseFactory.Success(dto,Messages.ExamsRetrievedSuccessfully);
    }
}
   
    
