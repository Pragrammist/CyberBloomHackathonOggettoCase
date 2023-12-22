using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class User : IdentityUser
{
    
}

public class PostMeetingDto
{
    public DateTime Time { get; set; }

    public string Title { get; set; } = null!;

    public string Theme { get; set; } = null!;

    public string SpeakerName { get; set; } = null!;

    public IEnumerable<IFormFile> SpeackerImage { get; set; } = null!;

    public string Splecializations { get; set; } = null!;

    public string Type { get; set; } = "онлайн/офлайн";

    public string SpeakerTelephone { get; set; } = null!;

    public string SpeakerEmail { get; set; } = null!;

    public string Tags { get; set; } = null!;

    public string VideoUrl { get; set; } = null!;
}


public class PutMeetingDto
{
    public long Id { get; set; }

    public DateTime Time { get; set; }

    public string Title { get; set; } = null!;

    public string Theme { get; set; } = null!;

    public string SpeakerName { get; set; } = null!;

    public IEnumerable<IFormFile> SpeackerImage { get; set; } = null!;

    public string Splecializations { get; set; } = null!;


    public string Type { get; set; } = "онлайн/офлайн";

    public string SpeakerTelephone { get; set; } = null!;

    public string SpeakerEmail { get; set; } = null!;

    public string Tags { get; set; } = null!;

    public string VideoUrl { get; set; } = null!;
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

    public string SpeakerEmail { get; set; } = null!;

    public string Tags { get; set; } = null!;

    public string VideoUrl { get; set; } = null!;
}


public class ApplicationContext : IdentityDbContext<User>
{
    public DbSet<Meeting> Meetings { get; set; }
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Meeting>();
    }
}