using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Mapster;
using static Consts;
using Microsoft.Extensions.FileProviders;


TypeAdapterConfig<PutMeetingDto, Meeting>.NewConfig().Map(d => d.SpeackerImage, s => s.SpeackerImage.JoinFileNames()); 
TypeAdapterConfig<PostMeetingDto, Meeting>.NewConfig().Map(d => d.SpeackerImage, s => s.SpeackerImage.JoinFileNames());


TypeAdapterConfig<PostMeetingDto, Meeting>.NewConfig().Map(d => d.Splecializations, s => String.Join(TOKENS_SEPORATOR, s.Splecializations));

TypeAdapterConfig<PutMeetingDto, Meeting>.NewConfig().Map(d => d.Splecializations, s => String.Join(TOKENS_SEPORATOR, s.Splecializations));


TypeAdapterConfig<PostMeetingDto, Meeting>.NewConfig().Map(d => d.Time, s => s.Time.ToUniversalTime());

TypeAdapterConfig<PutMeetingDto, Meeting>.NewConfig().Map(d => d.Time, s => s.Time.ToUniversalTime());
var dir = Directory.CreateDirectory("cyber-boom-files");
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddDbContext<ApplicationContext>(options =>
                options.UseNpgsql(builder.Configuration["CONNECTION_STRING"]));

builder.Services.AddIdentity<User, IdentityRole>()
.AddEntityFrameworkStores<ApplicationContext>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // указывает, будет ли валидироваться издатель при валидации токена
            ValidateIssuer = true,
            // строка, представляющая издателя
            ValidIssuer = AuthOptions.ISSUER,
            // будет ли валидироваться потребитель токена
            ValidateAudience = true,
            // установка потребителя токена
            ValidAudience = AuthOptions.AUDIENCE,
            // будет ли валидироваться время существования
            ValidateLifetime = true,
            // установка ключа безопасности
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            // валидация ключа безопасности
            ValidateIssuerSigningKey = true,
         };
    });
// .AddGoogle(googleOptions =>
// {
//     googleOptions.ClientId = configuration["Authentication:Google:ClientId"] ?? throw new NullReferenceException("");
//     googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? throw new NullReferenceException("");
// });

// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//     .AddEntityFrameworkStores<ApplicationContext>();
// builder.Services.AddRazorPages();



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(builder => builder.AllowAnyMethod());

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.ContentRootPath, "cyber-boom-files")),
    RequestPath = "/api/cyber-boom-files"
});

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();



app.UseAuthentication();    // подключение аутентификации
app.UseAuthorization();



app.MapControllers();
//app.MapRazorPages();

app.Run();

public class AuthOptions
{
    public const string ISSUER = "MyAuthServer"; // издатель токена
    public const string AUDIENCE = "MyAuthClient"; // потребитель токена
    const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => 
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}

public static class Consts
{
    public const char TOKENS_SEPORATOR = ';';
}
public static class DataHelpers
{
    public static string JoinFileNames(this IEnumerable<IFormFile> files) => files.Select(s => s.FileName).JoinStrings();

    public static string JoinStrings(this IEnumerable<string> files) => String.Join(TOKENS_SEPORATOR, files.Select(s => s));

    public static TimeSpan ParseDuration(this string duration) 
    {
        var durArr = duration.Split(TOKENS_SEPORATOR, StringSplitOptions.RemoveEmptyEntries);

        var hours = int.Parse(durArr.First());
        var minutes = int.Parse(durArr[1]);
        
        return new TimeSpan(hours, minutes, 0);

    }

    public static async Task WriteFileToDirectory(this IFormFile file)
    {
        var readStream = file.OpenReadStream();
        var memstream = new MemoryStream();
        await readStream.CopyToAsync(memstream);
        await File.WriteAllBytesAsync(Path.Combine("cyber-boom-files", file.FileName), memstream.ToArray());
    }

    public static async Task WriteFilesToDirectory(this IEnumerable<IFormFile> files)
    {   
        foreach(var file in files)
        {
            await file.WriteFileToDirectory();
        }
        
    }

    public static async Task<StatsData> GetStatistic(this ApplicationContext applicationContext, string id)
    {
        var specialities = await applicationContext.UserWriteToMetings.Where(c => c.UserId == id)
            .Join(applicationContext.Meetings, 
                m => m.MeetingId, 
                m => m.Id,
                (c,m) => new {
                    m.Tags,
                    m.Id,
                    m.Duration,
                    m.Time
                }
            ).Where(t => DateTime.UtcNow > t.Time).ToArrayAsync();

        var selectedSpecialities = specialities.Select(s => new {
            s.Id,
            Tags = s.Tags.Split(TOKENS_SEPORATOR, StringSplitOptions.RemoveEmptyEntries),
            Duration = s.Duration.ParseDuration().TotalHours
        });

        var allTags = selectedSpecialities.SelectMany(s => s.Tags).Distinct();
        var count = selectedSpecialities.Count();
        
        StatsData stats = new StatsData{
            Count = count,
            Hours = selectedSpecialities.Sum(m => m.Duration) * count
             
        };
        foreach(var tag in allTags)
        {
            //StatsData.TagStats
            var specByTag = selectedSpecialities.Where(f => f.Tags.Contains(tag));
            var countByTag = specByTag.Count();
            var hours = selectedSpecialities.Sum(s => s.Duration) * countByTag;

            var stat = new StatsData.TagStats
            {
                Count = countByTag,
                Tag = tag,
                Hours = hours
            };
            stats.StatsByTag.Add(stat);
        }

       
        
        return stats;
    }
}
