using UCMS.DTOs.Instructor;
using UCMS.DTOs;
using UCMS.DTOs.Student;

namespace UCMS.Services.InstructorService.Abstraction
{
    public interface IInstructorService
    {
        Task<ServiceResponse<InstructorProfileDto>> GetInstructorProfileById(int userId);
        Task<ServiceResponse<GetInstructorDto>> GetSpecializedInfo();
        Task<ServiceResponse<GetInstructorDto>> EditInstructor(EditInstructorDto dto);
        Task<ServiceResponse<InstructorProfileDto>> GetCurrentInstructor();
    }
}
