using AutoMapper;
using UCMS.DTOs;
using UCMS.DTOs.Instructor;
using UCMS.DTOs.Student;
using UCMS.Models;
using UCMS.Repositories.InstructorRepository.Abstraction;
using UCMS.Resources;
using UCMS.Services.InstructorService.Abstraction;
using UCMS.Services.Utils;

namespace UCMS.Services.InstructorService
{
    public class InstructorService : IInstructorService
    {
        private readonly IInstructorRepository _instructorRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<InstructorService> _logger;
        private readonly UrlBuilder _urlBuilder;

        public InstructorService(IInstructorRepository instructorRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<InstructorService> logger, UrlBuilder urlBuilder)
        {
            _instructorRepository = instructorRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _urlBuilder = urlBuilder;
            _logger = logger;
        }

        public async Task<ServiceResponse<GetInstructorDto>> GetSpecializedInfo()
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
            var instructor = await _instructorRepository.GetInstructorByUserIdAsync(user.Id);

            if (instructor == null)
            {
                return new ServiceResponse<GetInstructorDto>
                {
                    Success = false,
                    Message = String.Format(Messages.UserNotFound, user.Id)
                };
            }

            GetInstructorDto responseInstructor = _mapper.Map<GetInstructorDto>(instructor);
            return new ServiceResponse<GetInstructorDto>
            {
                Data = responseInstructor,
                Success = true,
                Message = string.Format(Messages.UserFound, user.Id)
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
                    Message = String.Format(Messages.UserNotFound, user.Id)
                };

            Instructor updatedInstructor = _mapper.Map(editInstructorDto, instructor);
            await _instructorRepository.UpdateInstructorAsync(updatedInstructor);
            _logger.LogInformation("Instructor {userId} updated successfully", user.Id);

            GetInstructorDto responseStudent = _mapper.Map<GetInstructorDto>(updatedInstructor);
            return new ServiceResponse<GetInstructorDto>
            {
                Data = responseStudent,
                Success = true,
                Message = string.Format(Messages.UpdateUser, user.Id)
            };
        }

        public async Task<ServiceResponse<InstructorProfileDto>> GetCurrentInstructor()
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"] as User;
            Instructor? instructor = await _instructorRepository.GetInstructorByUserIdAsync(user.Id);

            if (instructor == null)
            {
                return new ServiceResponse<InstructorProfileDto>
                {
                    Success = false,
                    Message = String.Format(Messages.UserNotFound, user.Id)
                };
            }

            InstructorProfileDto responseInstructor = _mapper.Map<InstructorProfileDto>(instructor);
            responseInstructor.ProfileImagePath = _urlBuilder.BuildUrl(_httpContextAccessor, responseInstructor.ProfileImagePath);
            return new ServiceResponse<InstructorProfileDto>
            {
                Data = responseInstructor,
                Success = true,
                Message = string.Format(Messages.UserFound, user.Id)
            };
        }
    }
}
