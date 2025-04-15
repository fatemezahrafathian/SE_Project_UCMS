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
}