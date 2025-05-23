using UCMS.DTOs;
using UCMS.DTOs.PhaseDto;

namespace UCMS.Services.PhaseService.Abstraction;

public interface IPhaseService
{
    Task<ServiceResponse<GetPhaseForInstructorDto>> CreatePhaseAsync(int projectId, CreatePhaseDto dto);
    Task<ServiceResponse<GetPhaseForInstructorDto>> GetPhaseByIdForInstructorAsync(int phaseId);
    Task<ServiceResponse<GetPhaseForInstructorDto>> UpdatePhaseAsync(int projectId, int phaseId, PatchPhaseDto dto);
    Task<ServiceResponse<string>>  DeletePhaseAsync(int projectId, int phaseId);
    Task<ServiceResponse<List<GetPhasesForInstructorDto>>> GetPhasesForInstructor(int projectId);
    Task<ServiceResponse<FileDownloadDto>> HandleDownloadPhaseFileAsync(int projectId);
}