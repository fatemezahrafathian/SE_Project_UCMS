namespace UCMS.Services.EmailService.Abstraction;

public interface IEmailService
{
    Task SendVerificationEmail(string email, string token); 
}