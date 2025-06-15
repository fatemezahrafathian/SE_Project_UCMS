using UCMS.DTOs.ExerciseSubmissionDto;
using UCMS.DTOs.PhaseSubmissionDto;
using UCMS.Models;

namespace UCMS.Repositories.ExerciseSubmissionRepository.Abstraction;

public interface IExerciseSubmissionRepository
{
    Task AddExerciseSubmissionAsync(ExerciseSubmission exerciseSubmission);
    Task<ExerciseSubmission?> GetExerciseSubmissionForInstructorByIdAsync(int exerciseSubmissionId);
    Task<ExerciseSubmission?> GetExerciseSubmissionForStudentByIdAsync(int exerciseSubmissionId);
    Task<List<ExerciseSubmission>> GetExerciseSubmissionsAsync(int exerciseId); 
    Task<List<ExerciseSubmission>> GetExerciseSubmissionsForInstructorByPhaseIdAsync(int exerciseId,
        SortExerciseSubmissionByForInstructorOption sortBy,
        SortOrderOption sortOrder);
    Task<List<ExerciseSubmission>> GetExerciseSubmissionsForStudentByPhaseIdAsync(int exerciseId,
        SortExerciseSubmissionByForStudentOption sortBy,
        SortOrderOption sortOrder);
    Task<ExerciseSubmission?> GetFinalExerciseSubmissionsAsync(int studentId, int exerciseId);
    Task UpdateExerciseSubmissionAsync(ExerciseSubmission exerciseSubmission);

}