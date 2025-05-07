using UCMS.DTOs;
using UCMS.DTOs.ClassDto;
using UCMS.Models;

namespace UCMS.Services.ClassService.Abstraction;

public interface IClassService
{
    Task<ServiceResponse<GetClassForInstructorDto>> CreateClass(CreateClassDto dto);
    Task<ServiceResponse<GetClassForInstructorDto>> GetClassForInstructor(int classId);
    Task<ServiceResponse<GetClassPageDto>> GetClassesForInstructor(PaginatedFilterClassForInstructorDto dto);
    Task<ServiceResponse<string>> DeleteClass(int classId); // return strinig is not good
    Task<ServiceResponse<GetClassForInstructorDto>> UpdateClassPartial(int classId, PatchClassDto dto);

}