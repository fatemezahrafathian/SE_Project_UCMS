using UCMS.DTOs.Instructor;
using UCMS.DTOs;
using UCMS.DTOs.Student;

namespace UCMS.Services.InstructorService.Abstraction
{
    public interface IInstructorService
    {
        Task<ServiceResponse<GetInstructorDto>> GetSpecializedInfo();
        Task<ServiceResponse<GetInstructorDto>> EditInstructor(EditInstructorDto dto);
        Task<ServiceResponse<InstructorProfileDto>> GetCurrentInstructor();
    }
}
