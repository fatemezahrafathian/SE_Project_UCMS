using Microsoft.EntityFrameworkCore;
using UCMS.Data;
using UCMS.Models;
using UCMS.Repositories.StudentRepository.Abstraction;

namespace UCMS.Repositories.StudentRepository
{
    public class StudentRepository: IStudentRepository
    {
        private readonly DataContext _context;

        public StudentRepository(DataContext context)
        {
            _context = context;
        }

        public async Task AddStudentAsync(Student studnet)
        {
            await _context.Students.AddAsync(studnet);
            await _context.SaveChangesAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
        }

        public async Task<Student?> GetStudentByUserIdAsync(int userId)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task UpdateStudentAsync(Student student)
        {
            _context.Students.Update(student);
            await SaveChangesAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<List<Student>> GetAllStudentsAsync()
        {
            return await _context.Students.ToListAsync();
        }
        
        public async Task<List<Student>> GetStudentsByStudentNumbersAsync(List<string> studentNumbers)
        {
            return await _context.Students
                .Where(s => studentNumbers.Contains(s.StudentNumber!))
                .Include(s => s.User)
                .ToListAsync();
        }
    }
}
