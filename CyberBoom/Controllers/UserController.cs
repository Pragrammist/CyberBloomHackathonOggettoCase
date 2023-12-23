using System.IdentityModel.Tokens.Jwt;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.IdentityModel.Tokens;
using static Consts;

namespace CyberBoom.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationContext _applicationContext;

    private readonly UserManager<User> _userManager;

    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(ApplicationContext applicationContext, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _applicationContext = applicationContext;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromForm]UserPost user)
    {
        await user.Avatar.WriteFileToDirectory();
        var userWr = new User {
            AvatarUrl = user.Avatar.FileName,
            Fio = user.Fio,
            Specialities = user.Specialities,
            TelegramBotUrl = user.TelegramBotUrl,
            UserName = user.Username
        };
        var result = await _userManager.CreateAsync(userWr);
        if(result.Succeeded)
            return Ok(
                new {
                    userWr.Id
                }
            );
        return BadRequest(result.Errors);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromForm]UserPut user)
    {
        await user.Avatar.WriteFileToDirectory();
        
        var fuser = await _userManager.FindByIdAsync(user.Id);

        if(fuser is null)
            throw new Exception("user not found");

        
        fuser.AvatarUrl = user.Avatar.FileName;
        fuser.Fio = user.Fio;
        fuser.Specialities = user.Specialities;
        fuser.TelegramBotUrl = user.TelegramBotUrl;
        fuser.UserName = user.Username;

        var result = await _userManager.UpdateAsync(fuser);
        if(result.Succeeded)
            return Ok(
               
            );
        return BadRequest(result.Errors);
    }

    [HttpPost("moderator")]
    public async Task<IActionResult> PostModerator([FromForm]UserPost user)
    {

        await user.Avatar.WriteFileToDirectory();
        var userWr = new User {
            AvatarUrl = user.Avatar.FileName,
            Fio = user.Fio,
            Specialities = user.Specialities,
            TelegramBotUrl = user.TelegramBotUrl,
            UserName = user.Username
        };
        
        

        var result = await _userManager.CreateAsync(userWr);
        
        if(!result.Succeeded)
            return BadRequest(result.Errors);
        
        var isExists = await _roleManager.RoleExistsAsync("модератор");

        if(!isExists){
            var roleResult = await _roleManager.CreateAsync(new IdentityRole("модератор"));
            if(!roleResult.Succeeded)
                throw new Exception("cannot create role");
        }
            
        var addingRole = await _userManager.AddToRoleAsync(userWr, "модератор");

        if(!addingRole.Succeeded)
            throw new Exception("cannot create role");

        return Ok(
                new {
                    userWr.Id
                }
            );
    }

    [HttpGet]
    public async Task<IActionResult> GetUserData(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if(user is null)
            return BadRequest();

        var role = await _userManager.GetRolesAsync(user);
        return Ok(new {
            user,
            role
        });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStats(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if(user is null)
            return BadRequest();

        var stats = await _applicationContext.GetStatistic(id);

        var achievmnets = _applicationContext.Achievments.Where(c => c.UserId == id);
       
        
        return Ok(new {
            Stats = stats,
            Achievments = achievmnets
        });
    } 
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
    public async Task<IActionResult> Get(string id)
    {
        var meeting = await _applicationContext.Meetings.FirstOrDefaultAsync(s => s.Id == id);

        
        return Ok(meeting);
    }

    
    [HttpGet("list")]
    public IActionResult GetList(int offset, int limit)
    {
        var meetings = _applicationContext.Meetings.AsNoTracking().Skip(offset).Take(limit);

        
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
    public async Task<IActionResult> Get(string id)
    {
        var review = await _applicationContext.Reviews.FirstAsync(s => s.Id == id);

        var user = await _applicationContext.Users.FirstAsync(s => s.Id == review.UserId);
        
        return Ok(new {
            review,
            user
        });
    }

    
    [HttpGet("list")]
    public IActionResult GetList(int offset, int limit)
    {
        var reviews = _applicationContext.Reviews.AsNoTracking()
            .Include(c => c.User)
            .Skip(offset)
            .Take(limit);

        // var userIds = reviews.Select(u => u.UserId).ToArray();
        // var users = _applicationContext.Users.Where(u => userIds.Contains(u.Id));
        
        return Ok(
            reviews
        );
    }
}



[ApiController]
[Route("/api/[controller]")]
public class QuestionsController : ControllerBase
{

    private readonly ApplicationContext _applicationContext;

    public QuestionsController(ApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PostQuestionDto question)
    {
        var dbWr = question.Adapt<Question>();
        await _applicationContext.Questions.AddAsync(dbWr);
        
        await _applicationContext.SaveChangesAsync();
        
        return Ok(new {
            dbWr.Id
        });
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody]PutQuestionDto question)
    {
        
        var fReview = await _applicationContext.Questions.FirstAsync(r => r.Id == question.Id);
        
        fReview.Text = question.Text;
        
        await _applicationContext.SaveChangesAsync();
        
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get(string id)
    {
        var question = await _applicationContext.Questions
            .FirstAsync(s => s.Id == id);

        
        return Ok(question);
    }

    
    [HttpGet("list")]
    public IActionResult GetList(int offset, int limit)
    {
        var questions = _applicationContext.Questions.AsNoTracking()
            .Include(c => c.User)
            .Skip(offset)
            .Take(limit);

        
        return Ok(questions);
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
    public async Task<IActionResult> Delete(string id)
    {
        
        var fReview = await _applicationContext.Reactions.FirstAsync(r => r.Id == id);
        
        
        _applicationContext.Reactions.Remove(fReview);


        await _applicationContext.SaveChangesAsync();
        
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get(string id)
    {
        var reaction = await _applicationContext.Reactions
            .FirstAsync(s => s.Id == id);

        
        return Ok(reaction);
    }

    
    [HttpGet("list")]
    public IActionResult GetList(int offset, int limit)
    {
        var reactions = _applicationContext.Reactions.AsNoTracking()
            .Skip(offset)
            .Take(limit);
        
        
        return Ok(reactions);
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
        
        var meeting = await _applicationContext.Meetings.FirstAsync(m => m.Id == write.MeetingId);



        if(DateTime.UtcNow > meeting.Time)
            return BadRequest();



        await _applicationContext.UserWriteToMetings.AddAsync(dbWr);
        
        
        var user = await _applicationContext.Users.FirstAsync(u => u.Id == write.UserId);


        var newStats = await _applicationContext.GetStatistic(write.UserId);

        
        var achievments = await WriteAchievment(newStats, write.UserId);        
        
        
        await _applicationContext.SaveChangesAsync();

        return Ok(new {
            dbWr.Id,
            Achievments = achievments
        });
        
    }

    async Task<List<Achievment>> WriteAchievment(StatsData stats, string userId)
    {
        List <Achievment> achievments = new List<Achievment>();
        if(stats.Count > 0 && stats.Count % 5 == 0)
        {
            var achievment = new Achievment{
                    Name = $"Редстоун Наблюдатель Level {stats.Count / 5}",
                    Text = "Вы cамый настоящий Редстоун Наблюдатель из игры Майнкрафт, который не пропускате ни единой всречи!",
                    UserId = userId
                };
            achievments.Add(achievment);
            await _applicationContext.Achievments.AddAsync(
                achievment
            );
        }
        var achievedTags = stats.StatsByTag.Where(st => st.Count > 0 && st.Count % 5 == 0);
        if(achievedTags.Count() > 0 && stats.Count % 5 == 0)
        {
            
            foreach(var tag in achievedTags)
            {
                var achievment = new Achievment{
                    Name = $"Вкачаю все в {tag.Tag} Level {tag.Count / 5}",
                    Text = $"Вы нежалеете очки времени на ветку {tag.Tag}, продолжайте в том же духе!",
                    UserId = userId
                };
                achievments.Add(achievment);
                await _applicationContext.Achievments.AddAsync(
                    achievment
                );
            }
            
        }
        return achievments;
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(string id)
    {
        var fReview = await _applicationContext.UserWriteToMetings.FirstAsync(r => r.Id == id);
        
        
        var meeting = await _applicationContext.Meetings.FirstAsync(m => m.Id == fReview.MeetingId);

        if(DateTime.UtcNow > meeting.Time)
            return BadRequest();

        _applicationContext.UserWriteToMetings.Remove(fReview);


        await _applicationContext.SaveChangesAsync();
        
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get(string id)
    {
        var review = await _applicationContext.UserWriteToMetings
            .FirstAsync(s => s.Id == id);

        
        return Ok(review);
    }

    
    [HttpGet("list")]
    public IActionResult GetList(int offset, int limit)
    {
        var reviews = _applicationContext.UserWriteToMetings.AsNoTracking()
            .Skip(offset)
            .Take(limit);

        
        return Ok(reviews);
    }
}

