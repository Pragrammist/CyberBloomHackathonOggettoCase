using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        var newStats = await _applicationContext.GetStatistic(write.UserId);

        var achievments = await WriteAchievment(newStats, write.UserId);

        await _applicationContext.SaveChangesAsync();

        return Ok(new { dbWr.Id, Achievments = achievments });
    }

    async Task<List<Achievment>> WriteAchievment(StatsData stats, string userId)
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
            await _applicationContext.Achievments.AddAsync(achievment);
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
                await _applicationContext.Achievments.AddAsync(achievment);
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
