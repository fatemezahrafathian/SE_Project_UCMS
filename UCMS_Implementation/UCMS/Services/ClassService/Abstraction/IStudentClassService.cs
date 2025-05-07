using UCMS.DTOs;
using UCMS.DTOs.ClassDto;

namespace UCMS.Services.ClassService.Abstraction;

public interface IStudentClassService
{
    Task<ServiceResponse<JoinClassResponseDto>> JoinClassAsync(JoinClassRequestDto request);
    Task<ServiceResponse<bool>> LeaveClassAsync(int classId);
    Task<ServiceResponse<bool>> RemoveStudentFromClassAsync(int classId, int studentId);
    Task<ServiceResponse<List<GetStudentsOfClassforInstructorDto>>> GetStudentsOfClassByInstructorAsync(int classId);
    Task<ServiceResponse<List<GetStudentsOfClassforStudentDto>>> GetStudentsOfClassByStudentAsync(int classId);
    Task<ServiceResponse<GetClassForStudentDto>> GetClassForStudent(int classId);
    Task<int> GetStudentClassCount(int classId);
}