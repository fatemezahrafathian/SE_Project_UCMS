using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.Instructor;
using UCMS.DTOs.Student;
using UCMS.Models;
using UCMS.Repositories.InstructorRepository.Abstraction;
using UCMS.Services.InstructorService.Abstraction;

namespace UCMS.Services.InstructorService
{
    public class InstructorService : IInstructorService
    {
        private readonly IInstructorRepository _instructorRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InstructorService(IInstructorRepository instructorRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _instructorRepository = instructorRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ServiceResponse<GetInstructorDto>> GetInstructorById(int instructorId)
        {
            Instructor? instructor = await _instructorRepository.GetInstructorById(instructorId);

            if (instructor == null)
            {
                return new ServiceResponse<GetInstructorDto>
                {
                    Success = false,
                    //Message = String.Format(Messages.UserNotFound, userId) FIXME
                };
            }

            GetInstructorDto responseInstructor = _mapper.Map<GetInstructorDto>(instructor);
            return new ServiceResponse<GetInstructorDto>
            {
                Data = responseInstructor,
                Success = true,
                Message = "Found"
            };
        }


        public async Task<ServiceResponse<GetInstructorDto>> EditInstructor(EditInstructorDto editInstructorDto)
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
            var instructor = await _instructorRepository.GetInstructorByUserIdAsync(user.Id);

            if (instructor == null)
                return new ServiceResponse<GetInstructorDto>
                {
                    Success = false,
                    //Message = String.Format(Messages.UserNotFound, userId) FIXME
                };

            Instructor updatedInstructor = _mapper.Map(editInstructorDto, instructor);
            await _instructorRepository.UpdateInstructorAsync(updatedInstructor);

            GetInstructorDto responseStudent = _mapper.Map<GetInstructorDto>(updatedInstructor);
            return new ServiceResponse<GetInstructorDto>
            {
                Data = responseStudent,
                Success = true,
                //Message = message FIXME
            };
        }

        public async Task<ServiceResponse<InstructorProfileDto>> GetCurrentInstructor()
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
            Instructor? instructor = user.Instructor;

            if (instructor == null)
            {
                return new ServiceResponse<InstructorProfileDto>
                {
                    Success = false,
                    //Message = String.Format(Messages.UserNotFound, userId) FIXME
                };
            }

            InstructorProfileDto responseInstructor = _mapper.Map<InstructorProfileDto>(instructor);
            return new ServiceResponse<InstructorProfileDto>
            {
                Data = responseInstructor,
                Success = true,
                Message = "Found"
            };
        }
    }
}
