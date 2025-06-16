using UCMS.Repositories.ExamRepository.Abstraction;
using UCMS.Repositories.ExerciseRepository.Abstraction;
using UCMS.Repositories.PhaseRepository.Abstraction;
using UCMS.Services.EmailService.Abstraction;

namespace UCMS.Services.DeadlineNotifierService;

public class DeadlineNotifierService:BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    
    public DeadlineNotifierService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var phaseRepo = scope.ServiceProvider.GetRequiredService<IPhaseRepository>();
            var exerciseRepo = scope.ServiceProvider.GetRequiredService<IExerciseRepository>();
            var examRepo = scope.ServiceProvider.GetRequiredService<IExamRepository>();
            var now = DateTime.Now;
            int runHour = 01;
            int runMinute = 00;

            var nextRunTime = DateTime.Today.AddDays(
                now.Hour > runHour || (now.Hour == runHour && now.Minute >= runMinute) ? 1 : 0
            ).AddHours(runHour).AddMinutes(runMinute);

            var delay = nextRunTime - now;
            await Task.Delay(delay, stoppingToken);

            var currentTime = DateTime.UtcNow;
            var lowerBound = currentTime.AddHours(7);
            var upperBound = currentTime.AddHours(31);
            
            var phasesStartDate =await phaseRepo.GetPhasesCloseStartDate(lowerBound, upperBound,stoppingToken);

            foreach (var phase in phasesStartDate)
            {
                foreach (var student in phase.Project.Class.ClassStudents)
                {
                    await emailService.SendNotificationEmailStart(student.Student.User.Email,"phase",phase.Project.Class.Title,phase.Title,phase.StartDate,phase.EndDate);
                }
            }
            var assignmentsStartDate = await exerciseRepo.GetExercisesCloseStartDate(lowerBound, upperBound, stoppingToken);

            foreach (var assignment in assignmentsStartDate)
            {
                foreach (var student in assignment.Class.ClassStudents)
                {
                    await emailService.SendNotificationEmailStart(student.Student.User.Email,"Exercise",assignment.Class.Title,assignment.Title,assignment.StartDate,assignment.EndDate);
                }
            }

            var phases =await phaseRepo.GetPhasesCloseDeadLines(lowerBound, upperBound,stoppingToken);

            foreach (var phase in phases)
            {
                foreach (var student in phase.Project.Class.ClassStudents)
                {
                    await emailService.SendNotificationEmailDeadLines(student.Student.User.Email,"phase",phase.Project.Class.Title,phase.Title,phase.EndDate);
                }
            }
            var assignments = await exerciseRepo.GetExercisesCloseDeadLines(lowerBound, upperBound, stoppingToken);

            foreach (var assignment in assignments)
            {
                foreach (var student in assignment.Class.ClassStudents)
                {
                    await emailService.SendNotificationEmailDeadLines(student.Student.User.Email,"Exercise",assignment.Class.Title,assignment.Title,assignment.EndDate);
                }
            }

            var exams = await examRepo.GetExamsCloseDeadLines(lowerBound, upperBound, stoppingToken);
            foreach (var exam in exams)
            {
                foreach (var student in exam.Class.ClassStudents)
                {
                    await emailService.SendNotificationEmailDeadLines(student.Student.User.Email,"exam",exam.Class.Title,exam.Title,exam.Date);
                }
            }
            
        }
    }
}