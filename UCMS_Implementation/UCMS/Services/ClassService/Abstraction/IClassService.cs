using UCMS.DTOs;
using UCMS.DTOs.ClassDto;
using UCMS.Models;

namespace UCMS.Services.ClassService.Abstraction;

public interface IClassService
{
    Task<ServiceResponse<GetClassForInstructorDto>> CreateClass(CreateClassDto dto);
    Task<ServiceResponse<GetClassForInstructorDto>> GetClassForInstructor(int classId);
    Task<ServiceResponse<GetClassForStudentDto>> GetClassForStudent(int classId);
    Task<ServiceResponse<List<GetClassPreviewForInstructorDto>>> FilterClassesOfInstructor(PaginatedFilterClassForInstructorDto dto);
    Task<ServiceResponse<string>> DeleteClass(int classId); // return strinig is not good
    Task<ServiceResponse<GetClassForInstructorDto>> PartialUpdateClass(int classId, PatchClassDto dto);
    Task<ServiceResponse<JoinClassResponseDto>> JoinClassAsync(JoinClassRequestDto request);
    Task<bool> IsStudentOfClass(int classId, int studentId);
    Task<ServiceResponse<bool>> LeaveClassAsync(int classId);
    Task<ServiceResponse<bool>> RemoveStudentFromClassAsync(int classId, int studentId);
    Task<ServiceResponse<List<GetStudentsOfClassforInstructorDto>>> GetStudentsOfClassByInstructorAsync(int classId);
    Task<ServiceResponse<List<GetStudentsOfClassforStudentDto>>> GetStudentsOfClassByStudentAsync(int classId);

}