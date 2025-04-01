using System.Net;
using System.Net.Mail;
using UCMS.Resources;
using UCMS.Services.EmailService.Abstraction;

namespace UCMS.Services.EmailService;
// check errors
// implement controller
public class EmailService: IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task SendVerificationEmail(string email, string confirmationLink)
    {
        string subject = EmailTemplates.ConfirmationEmail_Subject;
        string bodyTemplate = EmailTemplates.ConfirmationEmail_body;
        string body = string.Format(bodyTemplate, confirmationLink);

        var smtpSettings = _configuration.GetSection("SmtpSettings");

        var message = new MailMessage
        {
            From = new MailAddress("nrvtnhana@gmail.com"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(email);

        using var smtp = new SmtpClient(smtpSettings["Host"])
        {
            Port = int.Parse(smtpSettings["Port"]),
            Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
            EnableSsl = bool.Parse(smtpSettings["EnableSsl"]),
            UseDefaultCredentials = false
        };

        await smtp.SendMailAsync(message);
    }


}
