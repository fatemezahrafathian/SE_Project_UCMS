namespace UCMS.DTOs.ClassDto;

public class GetStudentsOfClassforInstructorDto
{
    public int UserId { get; set; }
    public int StudentId { get; set; }
    public string? ProfileImagePath { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? StudentNumber { get; set; }
}