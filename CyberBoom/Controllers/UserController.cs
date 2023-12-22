using System.IdentityModel.Tokens.Jwt;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CyberBoom.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UserController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<UserController> _logger;

    private readonly ApplicationContext _applicationContext;

    public UserController(ILogger<UserController> logger, ApplicationContext applicationContext)
    {
        _logger = logger;
        _applicationContext = applicationContext;
    }


    // [HttpGet("google-auth")]
    // public IActionResult Regiester()
    // {
    //     var properties = new AuthenticationProperties{
    //         RedirectUri = Url.Action("GoogleResponse")
    //     };
    //     return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    // }

    // [Route("google-response")]
    // public async Task<IActionResult> GoogleResponse()
    // {
    //     var result = await HttpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);

    //     var claims = result?.Principal?.Identities.First().Claims;
    //     var jwt = new JwtSecurityToken(
    //         issuer: AuthOptions.ISSUER,
    //         audience: AuthOptions.AUDIENCE,
    //         claims: claims,
    //         expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
    //         signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            
    //     var strJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

    //     return Ok(new {
    //         Token = strJwt
    //     });
    // }

    
}


[ApiController]
[Route("/api/[controller]")]
public class MeetingsController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<UserController> _logger;

    private readonly ApplicationContext _applicationContext;

    public MeetingsController(ILogger<UserController> logger, ApplicationContext applicationContext)
    {
        _logger = logger;
        _applicationContext = applicationContext;
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromForm]PostMeetingDto meeting)
    {
        await meeting.SpeackerImage.WriteFileToDirectory();
        var meetingWrite = meeting.Adapt<Meeting>();
        meetingWrite.SpeackerImage = meeting.SpeackerImage.JoinFileNames();
        await _applicationContext.Meetings.AddAsync(meetingWrite);

        await _applicationContext.SaveChangesAsync();

        
        return Ok(new {
            meetingWrite.Id
        });
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromForm]PutMeetingDto meeting)
    {
        await meeting.SpeackerImage.WriteFileToDirectory();
        var meetingWrite = meeting.Adapt<Meeting>();
        meetingWrite.SpeackerImage = meeting.SpeackerImage.JoinFileNames();
        var findedMeeting = await _applicationContext.Meetings.FirstAsync(s => s.Id == meeting.Id);
        findedMeeting = meetingWrite;

        await _applicationContext.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var meeting = await _applicationContext.Meetings.FirstOrDefaultAsync(s => s.Id == id);

        
        return Ok(meeting);
    }

    
    [HttpGet("list")]
    public IActionResult GetList(int offset, int limit)
    {
        var meetings = _applicationContext.Meetings.Skip(offset).Take(limit);

        
        return Ok(meetings);
    }
}
