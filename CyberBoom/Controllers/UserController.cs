using System.IdentityModel.Tokens.Jwt;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CyberBoom.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UsersController : ControllerBase
{
   

    

    private readonly ApplicationContext _applicationContext;

    private readonly UserManager<User> _userManager;

    public UsersController(ApplicationContext applicationContext, UserManager<User> userManager)
    {
        _applicationContext = applicationContext;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromForm]UserPost user)
    {
        await user.Avatar.WriteFileToDirectory();
        var userWr = new User {
            AvatarUrl = user.Avatar.FileName,
            UserName = user.Username
        };
        await _userManager.CreateAsync(userWr);

        return Ok(
            new {
                userWr.Id
            }
        );
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
    


    private readonly ApplicationContext _applicationContext;

    public MeetingsController(ApplicationContext applicationContext)
    {
        
        
        _applicationContext = applicationContext;
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromForm]PostMeetingDto meeting)
    {
        await meeting.SpeackerImage.WriteFilesToDirectory();
        await meeting.PlaceImages.WriteFilesToDirectory();
        var meetingWrite = meeting.Adapt<Meeting>();
        
        meetingWrite.SpeackerImage = meeting.SpeackerImage.JoinFileNames();
        meetingWrite.PlaceImages = meeting.PlaceImages.JoinFileNames();

        await _applicationContext.Meetings.AddAsync(meetingWrite);

        await _applicationContext.SaveChangesAsync();

        
        return Ok(new {
            meetingWrite.Id
        });
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromForm]PutMeetingDto meeting)
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



[ApiController]
[Route("/api/[controller]")]
public class ReviewsController : ControllerBase
{

    private readonly ApplicationContext _applicationContext;

    public ReviewsController(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PostReviewDto review)
    {
        var dbWr = review.Adapt<Review>();
        
        await _applicationContext.Reviews.AddAsync(dbWr);
        
        await _applicationContext.SaveChangesAsync();
        
        return Ok(new {
            dbWr.Id
        });
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody]PutReviewDto review)
    {
        
        var fReview = await _applicationContext.Reviews.FirstAsync(r => r.Id == review.Id);
        
        
        fReview.Text = review.Text;
        fReview.Score = review.Score;
        fReview.Date = review.Date;


        await _applicationContext.SaveChangesAsync();
        
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var review = await _applicationContext.Reviews
            .Include(c => c.User)
            .FirstAsync(s => s.Id == id);

        
        return Ok(review);
    }

    
    [HttpGet("list")]
    public IActionResult GetList(int offset, int limit)
    {
        var reviews = _applicationContext.Reviews
            .Include(c => c.User)
            .Skip(offset)
            .Take(limit);

        
        return Ok(reviews);
    }
}



[ApiController]
[Route("/api/[controller]")]
public class ReactionsController : ControllerBase
{

    private readonly ApplicationContext _applicationContext;

    public ReactionsController(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PostReactionDto reaction)
    {
        var dbWr = reaction.Adapt<Reaction>();
        
        await _applicationContext.Reactions.AddAsync(dbWr);
        
        await _applicationContext.SaveChangesAsync();
        
        return Ok(new {
            dbWr.Id
        });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(long id)
    {
        
        var fReview = await _applicationContext.Reactions.FirstAsync(r => r.Id == id);
        
        
        _applicationContext.Reactions.Remove(fReview);


        await _applicationContext.SaveChangesAsync();
        
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var review = await _applicationContext.Reviews
            .Include(c => c.User)
            .FirstAsync(s => s.Id == id);

        
        return Ok(review);
    }

    
    [HttpGet("list")]
    public IActionResult GetList(int offset, int limit)
    {
        var reviews = _applicationContext.Reviews
            .Include(c => c.User)
            .Skip(offset)
            .Take(limit);

        
        return Ok(reviews);
    }
}





[ApiController]
[Route("/api/users/meetings")]
public class UserWriteToMetingController : ControllerBase
{

    private readonly ApplicationContext _applicationContext;

    public UserWriteToMetingController(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PostUserWriteToMetingDto write)
    {
        var dbWr = write.Adapt<UserWriteToMeting>();
        
        await _applicationContext.UserWriteToMetings.AddAsync(dbWr);
        
        await _applicationContext.SaveChangesAsync();
        
        return Ok(new {
            dbWr.Id
        });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(long id)
    {
        
        var fReview = await _applicationContext.UserWriteToMetings.FirstAsync(r => r.Id == id);
        
        
        _applicationContext.UserWriteToMetings.Remove(fReview);


        await _applicationContext.SaveChangesAsync();
        
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var review = await _applicationContext.UserWriteToMetings
            .FirstAsync(s => s.Id == id);

        
        return Ok(review);
    }

    
    [HttpGet("list")]
    public IActionResult GetList(int offset, int limit)
    {
        var reviews = _applicationContext.UserWriteToMetings
            .Skip(offset)
            .Take(limit);

        
        return Ok(reviews);
    }
}

