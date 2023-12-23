using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class User : IdentityUser
{
    public string AvatarUrl { get; set; } = null!;

    public string Fio { get; set; } = null!;

    public string Specialities { get; set; } = null!;

    public string TelegramBotUrl { get; set; } = null!;

    public int Level { get; set; }
}

public class UserPost
{
    public IFormFile Avatar { get; set; } = null!;

    public string Fio { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Specialities { get; set; } = null!;

    public string TelegramBotUrl { get; set; } = null!;
}

public class PostMeetingDto
{
    public DateTime Time { get; set; }

    public string Title { get; set; } = null!;

    public string Theme { get; set; } = null!;

    public string SpeakerName { get; set; } = null!;

    public IEnumerable<IFormFile> SpeackerImage { get; set; } = null!;
    
    public IEnumerable<IFormFile> PlaceImages { get; set; } = null!;


    public string Splecializations { get; set; } = null!;

    public string Type { get; set; } = "онлайн/офлайн";

    public string SpeakerTelephone { get; set; } = null!;

    public string SpeakerEmail { get; set; } = null!;

    public string Tags { get; set; } = null!;

    public string Urls { get; set; } = null!;

    public string PlaceAdress { get; set; } = null!;

    public string Duration { get; set; } = null!;
}


public class UserWriteToMeting
{
    public long Id { get; set; }

    public string UserId { get; set; } = null!;

    public long MeetingId { get; set; }
}


public class PostUserWriteToMetingDto
{
    public string UserId { get; set; } = null!;

    public long MeetingId { get; set; }
}


public class PutMeetingDto
{
    public long Id { get; set; }

    public DateTime Time { get; set; }

    public string Title { get; set; } = null!;

    public string Theme { get; set; } = null!;

    public string SpeakerName { get; set; } = null!;

    public IEnumerable<IFormFile> SpeackerImage { get; set; } = null!;

    public IEnumerable<IFormFile> PlaceImages { get; set; } = null!;

    public string Splecializations { get; set; } = null!;


    public string Type { get; set; } = "онлайн/офлайн";

    public string SpeakerTelephone { get; set; } = null!;

    public string SpeakerEmail { get; set; } = null!;

    public string Tags { get; set; } = null!;

    public string Urls { get; set; } = null!;

    public string PlaceAdress { get; set; } = null!;

    public string Duration { get; set; } = null!;
}


public class Meeting
{
    public long Id { get; set; }

    public DateTime Time { get; set; }

    public string Title { get; set; } = null!;

    public string Theme { get; set; } = null!;

    public string SpeakerName { get; set; } = null!;

    public string SpeackerImage { get; set; } = null!;

    public string Splecializations { get; set; } = null!;

    
    public string Type { get; set; } = "онлайн/офлайн";

    public string SpeakerTelephone { get; set; } = null!;


    public string PlaceImages { get; set; } = null!;

    public string SpeakerEmail { get; set; } = null!;

    public string Tags { get; set; } = null!;

    public string Urls { get; set; } = null!;

    public string PlaceAdress { get; set; } = null!;

    public string Duration { get; set; } = null!;
}   


public class PostReviewDto
{
    public long MeetingId { get; set; }
    
    public string UserId { get; set; } = null!;

    public string Text { get; set; } = null!;

    public int Score { get; set; } = 0;

    DateTime _date;
    
    public DateTime Date { get => _date; set => _date = value.ToUniversalTime(); }
}



public class PutReviewDto
{
    public long Id { get; set; }
    
    public string Text { get; set; } = null!;

    public int Score { get; set; } = 0;

    DateTime _date;
    
    public DateTime Date { get => _date; set => _date = value.ToUniversalTime(); }
}


public class Question
{
    public long Id { get; set; }

    public string Text { get; set; } = null!;

    public long MeetingId { get; set; }

    public string UserId { get; set; } = null!;
}


public class Review
{
    public long Id { get; set; }

    public long MeetingId { get; set; }

    public User User { get; set; } = null!;
    
    public string UserId { get; set; } = null!;

    public string Text { get; set; } = null!;

    public int Score { get; set; } = 0;

    DateTime _date;
    
    public DateTime Date { get => _date; set => _date = value.ToUniversalTime(); }
}



public class PostReactionDto
{

    public long QuestionId { get; set; }

    public string UserId { get; set; } = null!;

    public bool IsLike { get; set; } = true;
}



public class Reaction
{
    public long Id { get; set; }

    public long QuestionId { get; set; }

    public string UserId { get; set; } = null!;

    public bool IsLike { get; set; } = true;
}



public class ApplicationContext : IdentityDbContext<User>
{
    public DbSet<Meeting> Meetings { get; set; }

    public DbSet<Review> Reviews { get; set; }

    public DbSet<Reaction> Reactions { get; set; }
    
    public DbSet<UserWriteToMeting> UserWriteToMetings { get; set; }

    public DbSet<Question> Questions { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Meeting>().HasMany<Review>().WithOne().HasForeignKey(c => c.MeetingId);
        builder.Entity<Meeting>().HasMany<Question>().WithOne().HasForeignKey(c => c.MeetingId);

        builder.Entity<User>().HasMany<Review>().WithOne(r => r.User).HasForeignKey(c => c.UserId);
        builder.Entity<User>().HasMany<Reaction>().WithOne().HasForeignKey(c => c.UserId);
        builder.Entity<User>().HasMany<Question>().WithOne().HasForeignKey(c => c.UserId);

        builder.Entity<Question>().HasMany<Reaction>().WithOne().HasForeignKey(c => c.QuestionId);

        builder.Entity<Meeting>().HasMany<UserWriteToMeting>().WithOne().HasForeignKey(c => c.MeetingId);
        builder.Entity<User>().HasMany<UserWriteToMeting>().WithOne().HasForeignKey(c => c.UserId);
    }
}