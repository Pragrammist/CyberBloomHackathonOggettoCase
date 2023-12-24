using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationContext : IdentityDbContext<User>
{
    public DbSet<Meeting> Meetings { get; set; }

    public DbSet<Review> Reviews { get; set; }

    public DbSet<Reaction> Reactions { get; set; }
    
    public DbSet<UserWriteToMeting> UserWriteToMetings { get; set; }

    public DbSet<Question> Questions { get; set; }


    public DbSet<Achievment> Achievments { get; set; }

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

        
        
        
        builder.Entity<Reaction>().HasOne<User>().WithMany().HasForeignKey(c => c.UserId);
        builder.Entity<Review>().HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId);

        builder.Entity<Question>().HasOne<Meeting>().WithMany().HasForeignKey(c => c.MeetingId);
        builder.Entity<Question>().HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId);
        
        builder.Entity<Reaction>().HasOne<Question>().WithMany().HasForeignKey(c => c.QuestionId);

        builder.Entity<Meeting>().HasMany<UserWriteToMeting>().WithOne().HasForeignKey(c => c.MeetingId);
        builder.Entity<UserWriteToMeting>().HasOne<User>().WithMany().HasForeignKey(c => c.UserId);
    
        builder.Entity<Achievment>().HasOne<User>().WithMany().HasForeignKey(c => c.UserId);
    }
}