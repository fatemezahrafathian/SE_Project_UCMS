using UCMS.DTOs;
using UCMS.DTOs.ClassDto;
using UCMS.Models;

namespace UCMS.Services.ClassService.Abstraction;

public interface IClassService
{
    Task<ServiceResponse<GetClassDto>> CreateClass(CreateClassDto dto);
    Task<ServiceResponse<GetClassDto>> GetClassById(int classId);
    Task<ServiceResponse<List<GetClassPreviewDto>>> GetClassesByInstructor();
    Task<ServiceResponse<string>> DeleteClass(int classId); // return strinig is not good
    Task<ServiceResponse<GetClassDto>> UpdateClass(int classId, UpdateClassDto dto);
}