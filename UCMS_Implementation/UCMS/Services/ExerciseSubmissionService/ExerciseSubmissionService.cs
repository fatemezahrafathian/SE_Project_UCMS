using UCMS.DTOs;
using UCMS.DTOs.ExerciseSubmissionDto;
using UCMS.Services.ExerciseSubmissionService.Abstraction;

namespace UCMS.Services.ExerciseSubmissionService;

public class ExerciseSubmissionService: IExerciseSubmissionService
{
    public Task<ServiceResponse<string>> CreateExerciseSubmission(int exerciseId, CreateExerciseSubmissionDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<FileDownloadDto>> GetExerciseSubmissionFileForInstructor(int exerciseSubmissionId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<FileDownloadDto>> GetExerciseSubmissionFileForStudent(int exerciseSubmissionId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<List<FileDownloadDto>>> GetExerciseSubmissionFiles(int exerciseId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<List<GetExerciseSubmissionPreviewForInstructorDto>>> GetExerciseSubmissionsForInstructor(SortExerciseSubmissionsForInstructorDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<List<GetExerciseSubmissionPreviewForStudentDto>>> GetExerciseSubmissionsForStudent(SortExerciseSubmissionsStudentDto sto)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResponse<string>> UpdateFinalSubmission(int exerciseSubmissionId)
    {
        throw new NotImplementedException();
    }
}