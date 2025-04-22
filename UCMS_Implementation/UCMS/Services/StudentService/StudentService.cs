using UCMS.DTOs.Student;
using UCMS.Repositories.StudentRepository.Abstraction;

namespace UCMS.Services.StudentService
{
    public class StudentService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<bool> EditStudentAsync(int userId, EditStudentDto editStudentDto)
        {
            var student = await _studentRepository.GetStudentByUserIdAsync(userId);
            if (student == null) return false;

            student.Major = editStudentDto.Major;
            student.EnrollmentYear = editStudentDto.EnrollmentYear;

            await _studentRepository.UpdateStudentAsync(student);
            return true;
        }
    }
}
