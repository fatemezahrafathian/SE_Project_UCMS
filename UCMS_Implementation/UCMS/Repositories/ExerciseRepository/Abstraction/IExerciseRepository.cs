using UCMS.Models;

namespace UCMS.Repositories.ExerciseRepository.Abstraction;

public interface IExerciseRepository
{
    Task AddAsync(Exercise exercise);
    Task<Exercise?> GetExerciseByIdAsync(int exerciseId);
    Task<Exercise?> GetExerciseWithRelationsByIdAsync(int exerciseId);
    Task<Exercise?> GetExerciseWithClassRelationsByIdAsync(int exerciseId);
    Task<Exercise?> GetSimpleExerciseWithoutRelationsByIdAsync(int exerciseId);
    Task<List<Exercise>> GetExercisesByClassIdAsync(int classId);
    Task UpdateAsync(Exercise exercise);
    Task<bool> ExistsWithTitleExceptIdAsync(string title, int classId, int exerciseIdToExclude);
    Task DeleteAsync(Exercise exercise);
    Task<List<Exercise>> GetExercisesByStudentIdAsync(int studentId);
    Task<List<Exercise>> GetExercisesByInstructorIdAsync(int instructorId);
    Task<List<Exercise>> GetExercisesCloseDeadLines(DateTime lowerBound, DateTime upperBound,CancellationToken stoppingToken);
    Task<List<Exercise>> GetExercisesCloseStartDate(DateTime lowerBound, DateTime upperBound,CancellationToken stoppingToken);

}