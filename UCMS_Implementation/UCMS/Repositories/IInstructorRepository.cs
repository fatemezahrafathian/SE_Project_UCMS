using UCMS.Models;

namespace UCMS.Repositories;

public interface IInstructorRepository
{
    Task<Instructor?> GetInstructorByUserIdAsync(int userId);
}