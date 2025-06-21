using System.Net;
using System.Net.Mail;
using UCMS.Resources;
using UCMS.Services.EmailService.Abstraction;
using System.Globalization;

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
            From = new MailAddress("ucms.ui@gmail.com"),
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
    public async Task SendOneTimeCode(string email, string code)
    {
        string subject = EmailTemplates.OneTimeCode_subject;
        string bodyTemplate = EmailTemplates.OneTimeCode_body;
        string body = string.Format(bodyTemplate, code);

        var smtpSettings = _configuration.GetSection("SmtpSettings");

        var message = new MailMessage
        {
            From = new MailAddress("ucms.ui@gmail.com"),
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
    public async Task SendNotificationEmailDeadLines(string email, string itemType, string classTitle, string title, DateTime deadline)
    {
        string itemLabel = itemType.ToLower() switch
        {
            "exercise" => "تمرین",
            "phase" => "فاز",
            "exam" => "امتحان",
            _ => "مورد"
        };

        string subject = $"{itemLabel} «{title}» در حال اتمام";

        string body = $"{itemLabel} {title} در کلاس {classTitle} به زودی به پایان میرسد!<br/><br/>" +
                      $"مهلت ارسال پاسخ: {ToPersianDateTime(deadline)}";

        var smtpSettings = _configuration.GetSection("SmtpSettings");

        var message = new MailMessage
        {
            From = new MailAddress(smtpSettings["Username"]),
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
    public async Task SendNotificationEmailStart(
        string email,
        string itemType,
        string classTitle,
        string title,
        DateTime startDate,
        DateTime deadline)
    {
        string itemLabel = itemType.ToLower() switch
        {
            "exercise" => "تمرین",
            "phase" => "فاز",
            _ => "مورد"
        };

        string subject = $"{itemLabel} «{title}» شروع شد";

        string body = $"{itemLabel} {title} در کلاس {classTitle} به زودی شروع میشود!<br/><br/>" +
                      $"زمان شروع: {ToPersianDateTime(startDate)}<br/>" +
                      $"مهلت ارسال پاسخ: {ToPersianDateTime(deadline)}";

        var smtpSettings = _configuration.GetSection("SmtpSettings");

        var message = new MailMessage
        {
            From = new MailAddress(smtpSettings["Username"]),
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
    

    private string ToPersianDateTime(DateTime dateTime)
    {
        var persianCalendar = new PersianCalendar();
        var year = persianCalendar.GetYear(dateTime);
        var month = persianCalendar.GetMonth(dateTime);
        var day = persianCalendar.GetDayOfMonth(dateTime);
        var hour = persianCalendar.GetHour(dateTime);
        var minute = persianCalendar.GetMinute(dateTime);

        string[] persianMonths = {
            "فروردین", "اردیبهشت", "خرداد", "تیر",
            "مرداد", "شهریور", "مهر", "آبان",
            "آذر", "دی", "بهمن", "اسفند"
        };

        string monthName = persianMonths[month - 1];

        return $"{day} {monthName} {year} ساعت {hour:D2}:{minute:D2}";
    }




}
