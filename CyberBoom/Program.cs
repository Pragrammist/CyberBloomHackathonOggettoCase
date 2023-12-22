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


TypeAdapterConfig<PostMeetingDto, Meeting>.NewConfig().Map(d => d.Splecializations, s => String.Join(FILES_SEPORATOR_IN_STORE, s.Splecializations));

TypeAdapterConfig<PutMeetingDto, Meeting>.NewConfig().Map(d => d.Splecializations, s => String.Join(FILES_SEPORATOR_IN_STORE, s.Splecializations));


TypeAdapterConfig<PostMeetingDto, Meeting>.NewConfig().Map(d => d.Time, s => s.Time.ToUniversalTime());

TypeAdapterConfig<PutMeetingDto, Meeting>.NewConfig().Map(d => d.Time, s => s.Time.ToUniversalTime());

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddDbContext<ApplicationContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    })
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = configuration["Authentication:Google:ClientId"] ?? throw new NullReferenceException("");
    googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? throw new NullReferenceException("");
});

// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//     .AddEntityFrameworkStores<ApplicationContext>();
// builder.Services.AddRazorPages();



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.ContentRootPath, "cyber-boom-files")),
    RequestPath = "/cyber-boom-files"
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



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
    public const char FILES_SEPORATOR_IN_STORE = ';';
}
public static class PhileDataHelpers
{
    public static string JoinFileNames(this IEnumerable<IFormFile> files) => files.Select(s => s.FileName).JoinStrings();

    public static string JoinStrings(this IEnumerable<string> files) => String.Join(FILES_SEPORATOR_IN_STORE, files.Select(s => s));


    public static async Task WriteFileToDirectory(this IEnumerable<IFormFile> files)
    {
        var dir = Directory.CreateDirectory("cyber-boom-files");
        
        foreach(var file in files)
        {
            var readStream = file.OpenReadStream();
            var memstream = new MemoryStream();
            await readStream.CopyToAsync(memstream);
            await File.WriteAllBytesAsync(Path.Combine(dir.FullName, file.FileName), memstream.ToArray());
        }
        
    }
}
