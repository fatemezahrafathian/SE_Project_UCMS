using UCMS.DTOs;
using UCMS.DTOs.ExamDto;

namespace UCMS.Services.ExamService.Abstraction;

public interface IExamService
{
    Task<ServiceResponse<GetExamForInstructorDto>> CreateExamAsync(int classId, CreateExamDto dto);
    Task<ServiceResponse<GetExamForInstructorDto>> GetExamByIdForInstructorAsync(int examId);
    Task<ServiceResponse<GetExamForInstructorDto>> UpdateExamAsync(int examId, PatchExamDto dto);
    Task<ServiceResponse<string>>  DeleteExamAsync(int examId);
    Task<ServiceResponse<List<GetExamForInstructorDto>>> GetExamsOfClassForInstructor(int classId);
    Task<ServiceResponse<GetExamForStudentDto>> GetExamByIdForStudentAsync(int examId);
    Task<ServiceResponse<List<GetExamForStudentDto>>> GetExamsOfClassForStudent(int classId);
    
    Task<ServiceResponse<List<GetExamForInstructorDto>>> GetExamsForInstructor();
    
    Task<ServiceResponse<List<GetExamForStudentDto>>> GetExamsForStudent();

}