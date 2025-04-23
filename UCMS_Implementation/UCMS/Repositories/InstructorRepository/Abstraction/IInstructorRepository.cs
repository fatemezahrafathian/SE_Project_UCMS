using UCMS.DTOs;
using UCMS.DTOs.Instructor;
using UCMS.Models;

namespace UCMS.Repositories.InstructorRepository.Abstraction
{
    public interface IInstructorRepository
    {
        Task AddInstructorAsync(Instructor instructor);
        Task<Instructor?> GetInstructorById(int instructorId);
        Task<Instructor?> GetInstructorByUserIdAsync(int userId);
        Task UpdateInstructorAsync(Instructor instructor);
        Task<List<Instructor>> GetAllStudentsAsync();
    }
}
