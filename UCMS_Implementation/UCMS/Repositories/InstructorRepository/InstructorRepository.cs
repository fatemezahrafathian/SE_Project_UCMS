using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.InstructorRepository.Abstraction;

namespace UCMS.Repositories.InstructorRepository
{
    public class InstructorRepository : IInstructorRepository
    {
        private readonly DataContext _context;

        public InstructorRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Instructor?> GetInstructorById(int instructorId)
        {
            return await _context.Instructors.FirstOrDefaultAsync(i => i.Id == instructorId);
        }

        public async Task<Instructor?> GetInstructorByUserIdAsync(int userId)
        {
            return await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == userId);
        }

        public async Task UpdateInstructorAsync(Instructor instructor)
        {
            _context.Instructors.Update(instructor);
            await SaveChangesAsync();
        }

        public async Task<List<Instructor>> GetAllStudentsAsync()
        {
            return await _context.Instructors.ToListAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task AddInstructorAsync(Instructor instructor)
        {
            await _context.Instructors.AddAsync(instructor);
            await _context.SaveChangesAsync();
        }
    }
}
