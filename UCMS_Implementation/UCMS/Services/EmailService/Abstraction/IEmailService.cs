namespace UCMS.Services.EmailService.Abstraction;

public interface IEmailService
{
    Task SendVerificationEmail(string email, string token); 
    Task SendOneTimeCode(string email, string code);
    Task SendNotificationEmailDeadLines(string email, string itemType, string classTitle, string title, DateTime deadline);
    Task SendNotificationEmailStart(string email, string itemType, string classTitle, string title, DateTime startDate, DateTime endDate);
}