using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;

namespace UCMS.Repositories;

public class InstructorRepository: IInstructorRepository
{
    private readonly DataContext _context;

    public InstructorRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Instructor?> GetInstructorByUserIdAsync(int userId)
    {
        return await _context.Instructors
            .FirstOrDefaultAsync(i => i.UserId == userId);
    }
}