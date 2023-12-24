using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberBoom.Controllers;

[Authorize]
[ApiController]
[Route("/api/[controller]")]
public class MeetingsController : ControllerBase
{
    private readonly ApplicationContext _applicationContext;

    public MeetingsController(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }
    
    [HttpPost]
    public async Task<IActionResult> Post([FromForm] PostMeetingDto meeting)
    {
        await meeting.SpeackerImage.WriteFilesToDirectory();
        await meeting.PlaceImages.WriteFilesToDirectory();
        var meetingWrite = meeting.Adapt<Meeting>();

        meetingWrite.SpeackerImage = meeting.SpeackerImage.JoinFileNames();
        meetingWrite.PlaceImages = meeting.PlaceImages.JoinFileNames();

        await _applicationContext.Meetings.AddAsync(meetingWrite);

        await _applicationContext.SaveChangesAsync();

        return Ok(new { meetingWrite.Id });
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromForm] PutMeetingDto meeting)
    {
        await meeting.SpeackerImage.WriteFilesToDirectory();
        await meeting.PlaceImages.WriteFilesToDirectory();

        var meetingWrite = meeting.Adapt<Meeting>();

        meetingWrite.SpeackerImage = meeting.SpeackerImage.JoinFileNames();
        meetingWrite.PlaceImages = meeting.PlaceImages.JoinFileNames();

        var findedMeeting = await _applicationContext.Meetings.FirstAsync(s => s.Id == meeting.Id);
        findedMeeting = meetingWrite;

        await _applicationContext.SaveChangesAsync();
        return Ok();
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Get(string id)
    {
        var meeting = await _applicationContext.Meetings.FirstOrDefaultAsync(s => s.Id == id);

        return Ok(meeting);
    }

    [AllowAnonymous]
    [HttpGet("list")]
    public IActionResult GetList(int offset, int limit)
    {
        var meetings = _applicationContext.Meetings.AsNoTracking().Skip(offset).Take(limit);

        return Ok(meetings);
    }
}
