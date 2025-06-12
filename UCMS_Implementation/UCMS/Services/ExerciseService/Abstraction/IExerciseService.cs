using UCMS.DTOs;
using UCMS.DTOs.ExerciseDto;

namespace UCMS.Services.ExerciseService.Abstraction;

public interface IExerciseService
{
    Task<ServiceResponse<GetExerciseForInstructorDto>> CreateExerciseAsync(int classId, CreateExerciseDto dto);
    Task<ServiceResponse<GetExerciseForInstructorDto>> GetExerciseByIdForInstructorAsync(int exerciseId);
    Task<ServiceResponse<GetExerciseForInstructorDto>> UpdateExerciseAsync(int exerciseId, PatchExerciseDto dto);
    Task<ServiceResponse<string>>  DeleteExerciseAsync(int exerciseId);
    Task<ServiceResponse<FileDownloadDto>> HandleDownloadExerciseFileForInstructorAsync(int classId);
    Task<ServiceResponse<GetExerciseForStudentDto>> GetExerciseByIdForStudentAsync(int exerciseId);
    Task<ServiceResponse<FileDownloadDto>> HandleDownloadExerciseFileForStudentAsync(int exerciseId);
    Task<ServiceResponse<List<GetExercisesForStudentDto>>> GetExercisesForStudent();
    Task<ServiceResponse<List<GetExercisesForInstructorDto>>> GetExercisesForInstructor();
    Task<ServiceResponse<List<GetExercisesForStudentDto>>> GetExercisesOfClassForStudent(int classId);
    Task<ServiceResponse<List<GetExercisesForInstructorDto>>> GetExercisesOfClassForInstructor(int classId);
    
}