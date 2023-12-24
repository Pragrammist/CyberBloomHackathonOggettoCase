using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
namespace CyberBoom.Controllers;

[ApiController]
[Route("/api/users/meetings")]
public class UserWriteToMetingController : ControllerBase
{
    private readonly ApplicationContext _applicationContext;

    public UserWriteToMetingController(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }


    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PostUserWriteToMetingDto write)
    {
        var dbWr = write.Adapt<UserWriteToMeting>();

        var meeting = await _applicationContext.Meetings.FirstAsync(m => m.Id == write.MeetingId);

        if (DateTime.UtcNow > meeting.Time)
            return BadRequest();

        await _applicationContext.UserWriteToMetings.AddAsync(dbWr);

        var user = await _applicationContext.Users.FirstAsync(u => u.Id == write.UserId);

        
        var delay = meeting.Time - DateTime.UtcNow - new TimeSpan(2, 0, 0);
        
        BackgroundJob.Schedule<IEmailService>((_emailService) => _emailService.SendEmailAsync(user.Email!, "Вход в Муза", $"Здравcтвуйте. У вас мероприятие через 2 часа под названием {meeting.Title}"), delay); // за начало до встречи

        BackgroundJob.Schedule<ApplicationContext>((context) => AfterEnd(write.UserId, context), meeting.Time - DateTime.UtcNow); // за начало до встречи

        await _applicationContext.SaveChangesAsync();

        return Ok(new { dbWr.Id});
    }

    async Task<List<Achievment>> WriteAchievment(StatsData stats, string userId, ApplicationContext context)
    {
        List<Achievment> achievments = new List<Achievment>();
        if (stats.Count > 0 && stats.Count % 5 == 0)
        {
            var achievment = new Achievment
            {
                Name = $"Редстоун Наблюдатель Level {stats.Count / 5}",
                Text =
                    "Вы cамый настоящий Редстоун Наблюдатель из игры Майнкрафт, который не пропускате ни единой всречи!",
                UserId = userId
            };
            achievments.Add(achievment);
            await context.Achievments.AddAsync(achievment);
        }
        
        var achievedTags = stats.StatsByTag.Where(st => st.Count > 0 && st.Count % 5 == 0);
        if (achievedTags.Count() > 0 && stats.Count % 5 == 0)
        {
            foreach (var tag in achievedTags)
            {
                var achievment = new Achievment
                {
                    Name = $"Вкачаю все в {tag.Tag} Level {tag.Count / 5}",
                    Text =
                        $"Вы нежалеете очки времени на ветку {tag.Tag}, продолжайте в том же духе!",
                    UserId = userId
                };
                achievments.Add(achievment);
                await context.Achievments.AddAsync(achievment);
            }
        }
        return achievments;
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Delete(string id)
    {
        var fReview = await _applicationContext.UserWriteToMetings.FirstAsync(r => r.Id == id);

        var meeting = await _applicationContext.Meetings.FirstAsync(m => m.Id == fReview.MeetingId);

        if (DateTime.UtcNow > meeting.Time)
            return BadRequest();

        _applicationContext.UserWriteToMetings.Remove(fReview);

        await _applicationContext.SaveChangesAsync();

        return Ok();
    }


    async Task AfterEnd(string userId, ApplicationContext context)
    {
        var newStats = await context.GetStatistic(userId);

        await WriteAchievment(newStats, userId, context);

        await context.SaveChangesAsync();
    }

    [HttpGet]
    public async Task<IActionResult> Get(string id)
    {
        var review = await _applicationContext.UserWriteToMetings.FirstAsync(s => s.Id == id);

        return Ok(review);
    }

    [HttpGet("list")]
    public IActionResult GetList(int offset, int limit)
    {
        var reviews = _applicationContext.UserWriteToMetings
            .AsNoTracking()
            .Skip(offset)
            .Take(limit);

        return Ok(reviews);
    }
}



public interface IEmailService
{
    public Task SendEmailAsync(string email, string subject, string message);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress(_configuration["Email:Name"], _configuration["Email:Address"]));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = message
        };

        using var client = new SmtpClient();
        client.Timeout = 15000;
        await client.ConnectAsync(_configuration["Email:Host"], int.Parse(_configuration["Email:Port"]!), SecureSocketOptions.None);
        await client.AuthenticateAsync(_configuration["Email:Address"], _configuration["Email:Password"]);
        await client.SendAsync(emailMessage);
        
        _logger.LogInformation("Письмо отправлено на почту {Email}", email);
        
        await client.DisconnectAsync(true);
    }
}